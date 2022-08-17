// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Autocomplete;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.AutoComplete;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Icons;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Settings;
using ScintillaNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.DataViewing
{
    /// <summary>
    /// Allows you to view and execute an SQL query generated by RDMP, this could be a preview of the top 100 records of a TableInfo or an extract from an Aggregate Configuration graph
    /// The purpose is to give you a quick view as to the content of the table/column without having to launch 'Sql Management Studio' or whatever other tool you use to query your data
    /// repository.  You can edit the SQL as you need but only a single DataTable return is supported, this form is not a replacement for an SQL IDE.
    /// 
    /// <para>The query is sent using DataAccessContext.InternalDataProcessing</para>
    /// </summary>
    public partial class ViewSQLAndResultsWithDataGridUI : RDMPUserControl, IObjectCollectionControl
    {
        private IViewSQLAndResultsCollection _collection;
        private Scintilla _scintilla;
        private Task _task;
        private DbCommand _cmd;
        private string _originalSql;
        private DiscoveredServer _server;
        private AutoCompleteProviderWin _autoComplete;
        
        ToolStripButton btnExecuteSql = new ToolStripButton("Run");
        ToolStripButton btnResetSql = new ToolStripButton("Restore Original SQL");

        readonly ToolStripTimeout _timeoutControls = new ToolStripTimeout();
        private ToolStripLabel _serverHeader;
        private DatabaseTypeIconProvider _databaseTypeIconProvider;

        public ViewSQLAndResultsWithDataGridUI()
        {
            InitializeComponent();


            btnExecuteSql.Click += (s,e) => RunQuery();
            btnResetSql.Click += btnResetSql_Click;

            dataGridView1.ColumnAdded += (s, e) => e.Column.FillWeight = 1;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;

            _serverHeader = new ToolStripLabel("Server:");
            _databaseTypeIconProvider = new DatabaseTypeIconProvider();

            lblHelp.Visible = !UserSettings.AutoRunSqlQueries;


            Guid splitterGuid = new Guid("f48582bd-2698-423a-bb86-5e91b91129bb");

            var distance = UserSettings.GetSplitterDistance(splitterGuid);

            if (distance == -1)
            {
                splitContainer1.SplitterDistance = (int)(splitContainer1.Height * 0.75f);
            }
            else
            {
                // don't let them set the distance to greater/less than the control size
                splitContainer1.SplitterDistance = Math.Max(5,Math.Min(distance ,(int)(splitContainer1.Height * 0.99f)));
            }

            splitContainer1.SplitterMoved += (s,e)=>
            {
                UserSettings.SetSplitterDistance(splitterGuid,splitContainer1.SplitterDistance);
            };
        }

        private void ScintillaOnKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.KeyCode == Keys.F5)
                RunQuery();
        }


        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            //if we don't exist!
            if (_collection.DatabaseObjects.Any())
                if(!((IRevertable)_collection.DatabaseObjects[0]).Exists())
                    if(ParentForm != null)
                        ParentForm.Close();
        }

        public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
        {
            _collection = (IViewSQLAndResultsCollection) collection;

            CommonFunctionality.ClearToolStrip();

            btnExecuteSql.Image = activator.CoreIconProvider.GetImage(RDMPConcept.SQL, OverlayKind.Execute).ToBitmap();

            var overlayer = new IconOverlayProvider();
            btnResetSql.Image = overlayer.GetOverlay(Image.Load<Rgba32>(FamFamFamIcons.text_align_left), OverlayKind.Problem).ToBitmap();

            if (_scintilla == null)
            {
                // figure out what DBMS we are targetting
                var syntax = _collection.GetQuerySyntaxHelper();

                // Create the SQL editor for that language
                ScintillaTextEditorFactory factory = new ScintillaTextEditorFactory();
                _scintilla = factory.Create(null, SyntaxLanguage.SQL, syntax);
                splitContainer1.Panel1.Controls.Add(_scintilla);
                _scintilla.TextChanged += _scintilla_TextChanged;
                _scintilla.KeyUp += ScintillaOnKeyUp;

                // Setup autocomplete menu for the DBMS language
                _autoComplete = new AutoCompleteProviderWin(syntax);
                _collection.AdjustAutocomplete(_autoComplete);
                _autoComplete.RegisterForEvents(_scintilla);
            }

            SetItemActivator(activator);
            
            CommonFunctionality.Add(btnExecuteSql);
            CommonFunctionality.Add(btnResetSql);

            foreach (var c in _timeoutControls.GetControls())
                CommonFunctionality.Add(c);

            foreach (DatabaseEntity d in _collection.GetToolStripObjects())
                CommonFunctionality.AddToMenu(new ExecuteCommandShow(activator, d, 0, true));
            
            CommonFunctionality.Add(new ToolStripSeparator());
            CommonFunctionality.Add(_serverHeader);

            try
            {
                var dap = _collection.GetDataAccessPoint();
                _serverHeader.Text = $"Server: {dap.Server} Database: {dap.Database}";
                _serverHeader.Image = _databaseTypeIconProvider.GetImage(dap.DatabaseType).ToBitmap();
            }
            catch (Exception)
            {
                _serverHeader.Text = "Server:Unknown";
            }
            

            RefreshUIFromDatabase();
        }

        private void RefreshUIFromDatabase()
        {
            try
            {
                _server = DataAccessPortal.GetInstance()
                    .ExpectServer(_collection.GetDataAccessPoint(), DataAccessContext.InternalDataProcessing);

                string sql = _collection.GetSql();
                _originalSql = sql;
                //update the editor to show the user the SQL
                _scintilla.Text = sql;
                
                _server.TestConnection();

                if(UserSettings.AutoRunSqlQueries)
                {
                    LoadDataTableAsync(_server, sql);
                }
            }
            catch (Exception ex)
            {
                ShowFatal(ex);
            }
        }

        private void ShowFatal(Exception exception)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => ShowFatal(exception)));
                return;
            }

            splitContainer1.Panel2.Controls.Remove(dataGridView1);
            splitContainer1.Panel2.Controls.Add(tbErrors);
            tbErrors.Visible = true;
            tbErrors.Text = exception.Message;
            tbErrors.Dock = DockStyle.Fill;
            
            base.CommonFunctionality.Fatal("Query failed",exception);
        }

        private void HideFatal()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(HideFatal));
                return;
            }

            splitContainer1.Panel2.Controls.Add(dataGridView1);
            splitContainer1.Panel2.Controls.Remove(tbErrors);
            base.CommonFunctionality.ResetChecks();

        }

        private void LoadDataTableAsync(DiscoveredServer server, string sql)
        {
            lblHelp.Visible = false;

            //it is already running and not completed
            if (_task != null && !_task.IsCompleted)
                return;

            HideFatal();
            pbLoading.Visible = true;
            llCancel.Visible = true;

            _task = Task.Factory.StartNew(() =>
            {

                int timeout = 1000;
                while (!IsHandleCreated && timeout > 0)
                {
                    timeout -= 10;
                    Thread.Sleep(10);
                }

                try
                {
                    //then execute the command
                    using (DbConnection con = server.GetConnection())
                    {
                        con.Open();

                        _cmd = server.GetCommand(sql, con);
                        _cmd.CommandTimeout = _timeoutControls.Timeout;

                        DbDataAdapter a = server.GetDataAdapter(_cmd);
                        
                        DataTable dt = new DataTable();

                        a.Fill(dt);

                        MorphBinaryColumns(dt);

                        Invoke(new MethodInvoker(() => { dataGridView1.DataSource = dt; }));
                        con.Close();
                    }
                }
                catch (Exception e)
                {
                    ShowFatal(e);
                }
                finally
                {
                    if (IsHandleCreated)
                        Invoke(new MethodInvoker(() =>
                        {
                            pbLoading.Visible = false;
                            llCancel.Visible = false;
                        }));
                }
            });
        }

        private void MorphBinaryColumns(DataTable table)
        {
            var targetNames = table.Columns.Cast<DataColumn>()
              .Where(col => col.DataType.Equals(typeof(byte[])))
              .Select(col => col.ColumnName).ToList();
            foreach (string colName in targetNames)
            {
                // add new column and put it where the old column was
                var tmpName = "new";
                table.Columns.Add(new DataColumn(tmpName, typeof(string)));
                table.Columns[tmpName].SetOrdinal(table.Columns[colName].Ordinal);

                // fill in values in new column for every row
                foreach (DataRow row in table.Rows)
                {
                    row[tmpName] = "0x" + string.Join("",
                      ((byte[])row[colName]).Select(b => b.ToString("X2")).ToArray());
                }

                // cleanup
                table.Columns.Remove(colName);
                table.Columns[tmpName].ColumnName = colName;
            }
        }

        public IPersistableObjectCollection GetCollection()
        {
            return _collection;
        }

        public string GetTabName()
        {
            return _collection.GetTabName();
        }

        public string GetTabToolTip()
        {
            return null;
        }

        private void llCancel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(_cmd != null)
                _cmd.Cancel();
        }

        void _scintilla_TextChanged(object sender, EventArgs e)
        {
            //enable the reset button only if the SQL has changed (e.g. user is typing stuff)
            btnResetSql.Enabled = !_originalSql.Equals(_scintilla.Text);
        }
        
        private void RunQuery()
        {
            var selected = _scintilla.SelectedText;

            //Run the full query or only the selected portion
            LoadDataTableAsync(_server, string.IsNullOrWhiteSpace(selected)?_scintilla.Text:selected);
        }

        private void btnResetSql_Click(object sender, EventArgs e)
        {
            _scintilla.Text = _originalSql;
        }

        void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            WideMessageBox.Show("Full Text", dataGridView1.Rows[e.RowIndex]);
        }
    }
}
