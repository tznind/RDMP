// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.Copying;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs;

public delegate void ParameterEventHandler(object sender, ISqlParameter parameter);

/// <summary>
/// Part of ParameterCollectionUI, this control shows the SQL generated by the current configuration of SQL parameters.  You can edit the SQL in areas that do not have a grey background
/// (these reflect locked parameters - see ParameterCollectionUI).  editing the SQL changes the corresponding SQL parameter in ParameterCollectionUI.  You cannot add new Parameters by
/// directly typing in new SQL or change the line number of in the control (basically you should stick to editing the name/declaration and the value text.
/// </summary>
public partial class ParameterEditorScintillaControlUI : RDMPUserControl
{
    private Scintilla QueryEditor;

    public event ParameterEventHandler ParameterSelected = delegate { };
    public event ParameterEventHandler ParameterChanged = delegate { };
    public event Action ProblemObjectsFound = delegate { };

    public ParameterCollectionUIOptions Options { get; set; }

    public Dictionary<ISqlParameter,Exception> ProblemObjects { get; private set; }

    public bool IsBroken { get; private set; }

    public ParameterEditorScintillaControlUI()
    {
        InitializeComponent();

        if (VisualStudioDesignMode) //don't add the QueryEditor if we are in design time (visual studio) because it breaks
            return;

        QueryEditor = new ScintillaTextEditorFactory().Create(new RDMPCombineableFactory());
        gbCompiledView.Controls.Add(QueryEditor);

        QueryEditor.KeyDown += new KeyEventHandler(QueryEditor_KeyDown);
        QueryEditor.KeyUp += new KeyEventHandler(QueryEditor_KeyUp);
        QueryEditor.MouseUp += new MouseEventHandler(QueryEditor_MouseUp);

        QueryEditor.Leave += (s,e)=>RegenerateSQL();
        ProblemObjects = new Dictionary<ISqlParameter, Exception>();
    }


    private void QueryEditor_MouseUp(object sender, MouseEventArgs e)
    {
        UpdateEditability();
    }
    private void QueryEditor_KeyUp(object sender, KeyEventArgs e)
    {
        UpdateEditability();
    }
    private void QueryEditor_KeyDown(object sender, KeyEventArgs e)
    {
        UpdateEditability();
    }

    private void UpdateEditability()
    {
        var section = GetParameterOnLine(QueryEditor.CurrentLine);

        if (section == null)
            return;

        QueryEditor.ReadOnly = !section.Editable;
        ParameterSelected(this, section.Parameter);

        CheckForChanges();
    }


    private void CheckForChanges()
    {
        lblError.Visible = false;

        //for each parameter text section
        foreach (var section in Sections)
        {
            var sql = "";

            //get the lines that make up the selection (freetext sql)
            for (var i = section.LineStart; i <= section.LineEnd; i++)
                sql += QueryEditor.Lines[i].Text;

            //pass the section its sql text an it will tell us if it is borked or changed or unchanged
            var changed = section.CheckForChanges(sql);

            if (changed == FreeTextParameterChangeResult.ChangeRejected)
            {
                lblError.Visible = true;
                lblError.Text = "User tried to make illegal change to SQL";
                RegenerateSQL();
                return;
            }

            if (changed == FreeTextParameterChangeResult.ChangeAccepted)
                ParameterChanged(this,section.Parameter);
        }
    }

    private ParameterEditorScintillaSection GetParameterOnLine(int lineNumber)
    {
        return Sections.SingleOrDefault(s=>s.IncludesLine(lineNumber));
    }

    private List<ParameterEditorScintillaSection> Sections = new();
        
    /// <summary>
    /// Updates the Sql code for the current state of the <see cref="Options"/> 
    /// </summary>
    public void RegenerateSQL()
    {
        ProblemObjects = new Dictionary<ISqlParameter, Exception>();

        var parameterManager = Options.ParameterManager;
        Sections.Clear();

        try
        {
            IsBroken = false;
            QueryEditor.ReadOnly = false;
            var sql = "";
            var finalParameters = parameterManager.GetFinalResolvedParametersList().ToArray();

            var currentLine = 0;

            foreach (var parameter in finalParameters)
            {
                //if it's a user one
                if(AnyTableSqlParameter.HasProhibitedName(parameter) && !ProblemObjects.ContainsKey(parameter))
                    ProblemObjects.Add(parameter,new Exception(
                        $"Parameter name {parameter.ParameterName} is a reserved name for the RDMP software"));//advise them

                try
                {
                    parameter.Check(new ThrowImmediatelyCheckNotifier());
                }
                catch (SyntaxErrorException errorException)
                {
                    ProblemObjects.TryAdd(parameter, errorException);
                }


                var toAdd = QueryBuilder.GetParameterDeclarationSQL(parameter);

                var lineCount = GetLineCount(toAdd);

                Sections.Add(new ParameterEditorScintillaSection(Options.Refactorer,currentLine, currentLine += lineCount - 1, parameter, 
                        
                    !Options.ShouldBeReadOnly(parameter),
                        
                    toAdd));

                sql += toAdd;
                currentLine++;
            }

            QueryEditor.Text = sql.TrimEnd();

        }
        catch (Exception ex)
        {
            QueryEditor.Text = ex.ToString();
                
            IsBroken = true;

            if (ex is QueryBuildingException exception)
            {
                foreach (var p in exception.ProblemObjects.OfType<ISqlParameter>()) ProblemObjects.TryAdd(p, ex);

                ProblemObjectsFound();
            }
        }
        QueryEditor.ReadOnly = true;

        var highlighter = new ScintillaLineHighlightingHelper();
        ScintillaLineHighlightingHelper.ClearAll(QueryEditor);

        foreach (var section in Sections)
            if (!section.Editable)
                for (var i = section.LineStart; i <= section.LineEnd; i++)
                    ScintillaLineHighlightingHelper.HighlightLine(QueryEditor, i, Color.LightGray);
    }

        

    private static int GetLineCount(string s)
    {
        return s.Count(c => c.Equals('\n'));
    }


}