﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ANOStore.ANOEngineering;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using LoadModules.Generic.Attachers;
using LoadModules.Generic.LoadProgressUpdating;
using LoadModules.Generic.Mutilators.Dilution;
using MapsDirectlyToDatabaseTableUI;
using Newtonsoft.Json;
using ReusableLibraryCode;
using ReusableUIComponents;

namespace CatalogueManager.ANOEngineeringUIs
{
    /// <summary>
    /// Allows you to create an anonymous version of a Catalogue by selecting which columns to anonymise and which to drop etc.  This will create a new table in the
    /// database of your choice which will be imported as a new Catalogue and a new LoadMetadata will be created that will migrate and apply the anonymisations to the
    /// original Catalogue's data.
    /// </summary>
    public partial class ForwardEngineerANOCatalogueUI : ForwardEngineerANOCatalogueUI_Design
    {
        private bool _setup = false;
        private RDMPCollectionCommonFunctionality tlvANOTablesCommonFunctionality;
        private RDMPCollectionCommonFunctionality tlvTableInfoMigrationsCommonFunctionality;
        private ForwardEngineerANOCataloguePlanManager _planManager;
        
        public ForwardEngineerANOCatalogueUI()
        {
            InitializeComponent();
            serverDatabaseTableSelector1.HideTableComponents();

            olvSuffix.AspectGetter = (o) => o is ANOTable ? ((ANOTable) o).Suffix : null;
            olvNumberOfCharacters.AspectGetter = (o) => o is ANOTable ? (object) ((ANOTable)o).NumberOfCharactersToUseInAnonymousRepresentation: null;
            olvNumberOfDigits.AspectGetter = (o) => o is ANOTable ? (object) ((ANOTable)o).NumberOfIntegersToUseInAnonymousRepresentation : null;

            olvMigrationPlan.AspectGetter += MigrationPlanAspectGetter;
            
            olvPickedANOTable.HeaderImageKey = "ANOTable";
            olvPickedANOTable.AspectGetter += PickedANOTableAspectGetter;
            olvPickedANOTable.ImageGetter += PickedANOTable_ImageGetter;

            olvDilution.HeaderImageKey = "PreLoadDiscardedColumn";
            olvDilution.AspectGetter += DilutionAspectGetter;
            olvDilution.ImageGetter += Dilution_ImageGetter;
            
            olvDestinationType.AspectGetter += DestinationTypeAspectGetter;

            olvDestinationExtractionCategory.AspectGetter += DestinationExtractionCategoryAspectGetter;
            
            tlvTableInfoMigrations.CellEditStarting += tlvTableInfoMigrations_CellEditStarting;
            tlvTableInfoMigrations.CellEditFinishing += tlvTableInfoMigrations_CellEditFinishing;

            tlvTableInfoMigrations.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;

            AssociatedCollection = RDMPCollection.Catalogue;

            btnLoadPlan.Image = FamFamFamIcons.page_white_get;
            btnSavePlan.Image = FamFamFamIcons.page_white_put;
        }

        #region Aspect Getters and Setters

        private object MigrationPlanAspectGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;
            var table = rowobject as TableInfo;

            if (col != null)
                return _planManager.GetPlanForColumnInfo(col).Plan;

            if (_planManager.SkippedTables.Contains(table))
                return "Already Exists";
            
            return null;
        }

        private object PickedANOTable_ImageGetter(object rowObject)
        {
            var ci = rowObject as ColumnInfo;

            if (ci != null && _planManager.GetPlanForColumnInfo(ci).ANOTable != null)
                return imageList1.Images["ANOTable"];

            return null;
        }

        private object PickedANOTableAspectGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
            {
                var plan = _planManager.GetPlanForColumnInfo(col);

                if (plan.ANOTable != null)
                    return plan.ANOTable.ToString();

                if (plan.Plan  == Plan.ANO)
                    return "pick";
            }

            return null;
        }

        private object DilutionAspectGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
            {
                var plan = _planManager.GetPlanForColumnInfo(col);

                if (plan.Dilution != null)
                    return plan.Dilution;

                if (plan.Plan == Plan.Dilute)
                    return "pick";
            }

            return null;
        }
        private object Dilution_ImageGetter(object rowobject)
        {
            var col = rowobject as ColumnInfo;

            if (col != null)
            {
                var plan = _planManager.GetPlanForColumnInfo(col);

                if (plan.Dilution != null)
                    return "PreLoadDiscardedColumn";
            }

            return null;
        }

        private object DestinationTypeAspectGetter(object rowobject)
        {
            var ci = rowobject as ColumnInfo;
            try
            {
                if (ci != null)
                    return _planManager.GetPlanForColumnInfo(ci).GetEndpointDataType();
            }
            catch (Exception)
            {
                return "Error";
            }

            return null;
        }


        private object DestinationExtractionCategoryAspectGetter(object rowobject)
        {
            var ci = rowobject as ColumnInfo;
            try
            {
                if (ci != null)
                {
                    var plan = _planManager.GetPlanForColumnInfo(ci);

                    if (plan.ExtractionCategoryIfAny.HasValue)
                        return plan.ExtractionCategoryIfAny;

                    return null;
                }
            }
            catch (Exception)
            {
                return "Error";
            }

            return null;
        }


        #endregion

        void tlvTableInfoMigrations_CellEditStarting(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            if (e.RowObject is TableInfo)
                e.Cancel = true;

            if (e.Column == olvDestinationType)
                e.Cancel = true;

            var col = e.RowObject as ColumnInfo;

            if (e.Column == olvMigrationPlan)
                e.Control.Bounds = e.CellBounds;

            if (col != null)
            {
                var plan = _planManager.GetPlanForColumnInfo(col);

                if (e.Column == olvPickedANOTable)
                {
                    if (plan.Plan != Plan.ANO)
                    {
                        e.Cancel = true;
                        return;
                    }

                    var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(
                        _activator.CoreChildProvider.AllANOTables, true, false);
                    try
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                            plan.ANOTable = dialog.Selected as ANOTable;

                        Check();
                    }
                    catch (Exception exception)
                    {
                        ExceptionViewer.Show(exception);
                    }

                    e.Cancel = true;
                }

                if (e.Column == olvDilution)
                {

                    if (plan.Plan != Plan.Dilute)
                    {
                        e.Cancel = true;
                        return;
                    }

                    var cbx = new ComboBox();
                    cbx.DropDownStyle = ComboBoxStyle.DropDownList;
                    cbx.Bounds = e.CellBounds;
                    cbx.Items.AddRange(_planManager.DilutionOperations.ToArray());
                    e.Control = cbx;
                }
                if (e.Column == olvDestinationExtractionCategory)
                {
                    //if the plan is to drop
                    if (plan.Plan == Plan.Drop)
                    {
                        //don't let them edit it
                        e.Cancel = true;
                        return;
                    }

                    var cbx = new ComboBox();
                    cbx.DropDownStyle = ComboBoxStyle.DropDownList;
                    cbx.Bounds = e.CellBounds;

                    var list = Enum.GetValues(typeof (ExtractionCategory)).Cast<object>().Select(s=>s.ToString()).ToList();
                    list.Add("Clear");
                    
                    cbx.Items.AddRange(list.ToArray());
                    e.Control = cbx;

                    if(plan.ExtractionCategoryIfAny.HasValue)
                        cbx.SelectedItem = plan.ExtractionCategoryIfAny.Value.ToString();
                }

            }
        }
        
        void tlvTableInfoMigrations_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            try
            {
                var col = e.RowObject as ColumnInfo;

                if(col == null)
                    return;

                var plan = _planManager.GetPlanForColumnInfo(col);

                if(e.Column == olvMigrationPlan)
                    plan.Plan = (Plan) e.NewValue;

                if(e.Column == olvDilution)
                {
                    var cbx = (ComboBox)e.Control;
                    plan.Dilution = (IDilutionOperation)cbx.SelectedItem;
                }
                
                if (e.Column == olvDestinationExtractionCategory)
                {
                    var cbx = (ComboBox)e.Control;
                    if (cbx.SelectedItem == "Clear")
                        plan.ExtractionCategoryIfAny = null;
                    else
                    {
                        ExtractionCategory c;
                        ExtractionCategory.TryParse((string) cbx.SelectedItem, out c);
                        plan.ExtractionCategoryIfAny = c;
                    }
                        
                }
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }

            Check();
        }
        
        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            if (!_setup)
            {
                _planManager = new ForwardEngineerANOCataloguePlanManager(activator.RepositoryLocator,databaseObject);

                var settings = new RDMPCollectionCommonFunctionalitySettings {AddFavouriteColumn = false, AllowPinning = false};

                //Set up tree view to show ANO Tables that are usable
                tlvANOTablesCommonFunctionality = new RDMPCollectionCommonFunctionality();
                tlvANOTablesCommonFunctionality.SetUp(RDMPCollection.None, tlvANOTables, activator, olvANOTablesName, null, settings);
                
                tlvANOTables.AddObject(activator.CoreChildProvider.AllANOTablesNode);
                tlvANOTables.ExpandAll();
                
                //Setup tree view to show all TableInfos that you are trying to Migrate
                tlvTableInfoMigrationsCommonFunctionality = new RDMPCollectionCommonFunctionality();
                tlvTableInfoMigrationsCommonFunctionality.SetUp(RDMPCollection.None, tlvTableInfoMigrations, activator, olvTableInfoName, null, settings);
                
                //don't display anything below ColumnInfo
                tlvTableInfoMigrationsCommonFunctionality.AxeChildren = new[] {typeof (ColumnInfo)};
                
                rdmpObjectsRibbonUI1.SetIconProvider(activator.CoreIconProvider);
                rdmpObjectsRibbonUI1.Add(databaseObject);

                _setup = true;
            }
            else
                _planManager.RefreshTableInfos();

            //Add them and expand them
            tlvTableInfoMigrations.ClearObjects();
            tlvTableInfoMigrations.AddObjects(_planManager.TableInfos);
            tlvTableInfoMigrations.ExpandAll();

            ddDateColumn.DataSource =_planManager.TableInfos.SelectMany(t => t.ColumnInfos).Where(c => c.Data_type != null && c.Data_type.Contains("date")).ToArray();

            Check();
        }

        private void Check()
        {
            if (_planManager.TargetDatabase != null)
            {
                if (_planManager.TargetDatabase.Exists())
                {
                    _planManager.SkippedTables.Clear();

                    foreach (var t in _planManager.TableInfos)
                    {
                        var existing = _planManager.TargetDatabase.DiscoverTables(true);

                        //it is already migrated
                        if (existing.Any(e => e.GetRuntimeName().Equals(t.GetRuntimeName())))
                            _planManager.SkippedTables.Add(t);
                    }
                }
            }
            
            ragSmiley1.StartChecking(_planManager);

            DisableObjects();
        }

        private void tlvTableInfoMigrations_FormatCell(object sender, FormatCellEventArgs e)
        {
            
            var ci = e.Model as ColumnInfo;

            if (ci != null)
                if (_planManager.GetPlanForColumnInfo(ci).IsMandatory)
                    e.SubItem.BackColor = lblMandatory.BackColor;

            if(e.Column == olvMigrationPlan)
                if(e.Model is ColumnInfo)
                    e.SubItem.Font = new Font(e.Item.Font, FontStyle.Underline);
                else
                {
                    e.SubItem.Font = new Font(e.Item.Font, FontStyle.Italic);
                    e.SubItem.ForeColor = Color.Gray;
                }

            if (e.CellValue as string == "pick")
            {
                e.SubItem.ForeColor = Color.Blue;
                e.SubItem.Font = new Font(e.Item.Font, FontStyle.Underline);
            }
        }

        private void btnRefreshChecks_Click(object sender, EventArgs e)
        {
            _planManager.TargetDatabase = serverDatabaseTableSelector1.GetDiscoveredDatabase();
            Check();
        }

        private void serverDatabaseTableSelector1_SelectionChanged()
        {
            _planManager.TargetDatabase = serverDatabaseTableSelector1.GetDiscoveredDatabase();
            
            Check();
        }

        private void DisableObjects()
        {
            List<object> toDisable = new List<object>();

            toDisable.AddRange(_planManager.SkippedTables);
            toDisable.AddRange(_planManager.SkippedTables.SelectMany(t=>t.ColumnInfos));

            tlvTableInfoMigrations.DisabledObjects = toDisable;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                var engine = new ForwardEngineerANOCatalogueEngine(_activator.RepositoryLocator, _planManager);
                engine.Execute();

                if(engine.NewCatalogue != null && engine.LoadMetadata != null)
                {
                    foreach (KeyValuePair<TableInfo, QueryBuilder> sqls in engine.SelectSQLForMigrations)
                        CreateAttacher(sqls.Key, sqls.Value, engine.LoadMetadata, engine.LoadProgressIfAny);

                    foreach (KeyValuePair<PreLoadDiscardedColumn, IDilutionOperation> dilutionOps in engine.DilutionOperationsForMigrations)
                        CreateDilutionMutilation(dilutionOps,engine.LoadMetadata);
                    

                    Publish(engine.NewCatalogue);

                    if(MessageBox.Show("Successfully created Catalogue '" + engine.NewCatalogue + "', close form?","Success",MessageBoxButtons.YesNo) == DialogResult.Yes)
                        _activator.WindowArranger.SetupEditLoadMetadata(this,engine.LoadMetadata);
                }
                else
                    throw new Exception("Engine did not create a NewCatalogue/LoadMetadata");
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }

        private void CreateAttacher(TableInfo t, QueryBuilder qb, LoadMetadata lmd, LoadProgress loadProgressIfAny)
        {
            var pt = new ProcessTask(RepositoryLocator.CatalogueRepository, lmd, LoadStage.Mounting);
            pt.ProcessTaskType = ProcessTaskType.Attacher;
            pt.Name = "Read from " + t;
            pt.Path = typeof(RemoteTableAttacher).FullName;
            pt.SaveToDatabase();

            pt.CreateArgumentsForClassIfNotExists<RemoteTableAttacher>();


            pt.SetArgumentValue("RemoteServer", t.Server);
            pt.SetArgumentValue("RemoteDatabaseName", t.GetDatabaseRuntimeName());
            pt.SetArgumentValue("RemoteTableName", t.GetRuntimeName());
            pt.SetArgumentValue("DatabaseType", DatabaseType.MicrosoftSQLServer);
            pt.SetArgumentValue("RemoteSelectSQL", qb.SQL);

            pt.SetArgumentValue("RAWTableName", t.GetRuntimeName(LoadBubble.Raw));

            if(loadProgressIfAny != null)
            {
                pt.SetArgumentValue("Progress", loadProgressIfAny);
//              pt.SetArgumentValue("ProgressUpdateStrategy", DataLoadProgressUpdateStrategy.UseMaxRequestedDay);
                pt.SetArgumentValue("LoadNotRequiredIfNoRowsRead",true);
            }

            /*

                public DataLoadProgressUpdateInfo { get; set; }
            */
        }

        private void CreateDilutionMutilation(KeyValuePair<PreLoadDiscardedColumn, IDilutionOperation> dilutionOp,LoadMetadata lmd)
        {
            var pt = new ProcessTask(RepositoryLocator.CatalogueRepository, lmd, LoadStage.AdjustStaging);
            pt.CreateArgumentsForClassIfNotExists<Dilution>();
            pt.ProcessTaskType = ProcessTaskType.MutilateDataTable;
            pt.Name = "Dilute " + dilutionOp.Key.GetRuntimeName();
            pt.Path = typeof(Dilution).FullName;
            pt.SaveToDatabase();

            pt.SetArgumentValue("ColumnToDilute",dilutionOp.Key);
            pt.SetArgumentValue("Operation",dilutionOp.Value.GetType());

        }

        private void ddDateColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            _planManager.DateColumn = cbDateBasedLoad.Checked ? ddDateColumn.SelectedItem as ColumnInfo : null;
        }

        private void cbDateBasedLoad_CheckedChanged(object sender, EventArgs e)
        {
            ddDateColumn.Enabled = cbDateBasedLoad.Checked;
            tbStartDate.Enabled = cbDateBasedLoad.Checked;
            _planManager.DateColumn = cbDateBasedLoad.Checked ? ddDateColumn.SelectedItem as ColumnInfo : null;
            _planManager.StartDate = GetStartDate();
            Check();
        }

        private void tbStartDate_TextChanged(object sender, EventArgs e)
        {
            _planManager.StartDate = cbDateBasedLoad.Checked ? GetStartDate():null;
        }

        private DateTime? GetStartDate()
        {

            if (cbDateBasedLoad.Checked)
            {
                try
                {
                    var dt = DateTime.Parse(tbStartDate.Text);
                    tbStartDate.ForeColor = Color.Black;
                    return dt;
                }
                catch (Exception)
                {
                    tbStartDate.ForeColor = Color.Red;
                }
            }

            return null;
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            tlvTableInfoMigrations.UseFiltering = true;
            tlvTableInfoMigrations.ModelFilter = new TextMatchFilter(tlvTableInfoMigrations,tbFilter.Text);
        }

        private void btnSavePlan_Click(object sender, EventArgs e)
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Plans (*.plan)|*.plan";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var fi = new FileInfo(sfd.FileName);
                
                var cmdAnoTablesToo = new ExecuteCommandExportObjectsToFileUI(_activator, RepositoryLocator.CatalogueRepository.GetAllObjects<ANOTable>().ToArray(), fi.Directory);

                if (!cmdAnoTablesToo.IsImpossible)
                    cmdAnoTablesToo.Execute();

                var json = JsonConvertExtensions.SerializeObject(_planManager, _activator.RepositoryLocator);
                File.WriteAllText(fi.FullName,json);

            }
        }

        private void btnLoadPlan_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Plans (*.plan)|*.plan";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var fi = new FileInfo(ofd.FileName);
                    var json = File.ReadAllText(fi.FullName);
                    _planManager = (ForwardEngineerANOCataloguePlanManager)
                        JsonConvertExtensions.DeserializeObject(json, typeof(ForwardEngineerANOCataloguePlanManager), _activator.RepositoryLocator);

                    if (_planManager.StartDate != null)
                        tbStartDate.Text = _planManager.StartDate.Value.ToString();

                    cbDateBasedLoad.Checked = _planManager.DateColumn != null;
                    ddDateColumn.SelectedItem = _planManager.DateColumn;

                }
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ForwardEngineerANOCatalogueUI_Design, UserControl>))]
    public abstract class ForwardEngineerANOCatalogueUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {
    }
}
