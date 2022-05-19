//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Rdmp.Core.CommandLine.Gui {
    using System;
    using Terminal.Gui;
    
    
    public partial class ConsoleGuiUserSettings : Terminal.Gui.Window {
        
        private Terminal.Gui.TabView tabView1;
        
        private Terminal.Gui.CheckBox cbHideCohortBuilderContainersInFind;
        
        private Terminal.Gui.CheckBox cbStrictValidationForContainers;
        
        private Terminal.Gui.CheckBox cbHideEmptyTableLoadRunAudits;
        
        private Terminal.Gui.CheckBox cbShowPipelineCompletedPopup;
        
        private Terminal.Gui.CheckBox cbSkipCohortBuilderValidationOnCommit;
        
        private Terminal.Gui.CheckBox cbAlwaysJoinEverything;
        
        private Terminal.Gui.CheckBox cbSelectiveRefresh;
        
        private Terminal.Gui.Label label1;
        
        private Terminal.Gui.Label label2;
        
        private Terminal.Gui.TextField tbCreateDatabaseTimeout;
        
        private Terminal.Gui.TextField tbArchiveTriggerTimeout;
        
        private Terminal.Gui.TableView tableview1;
        
        private void InitializeComponent() {
            this.Width = Dim.Fill(0);
            this.Height = Dim.Fill(0);
            this.X = 0;
            this.Y = 0;
            this.Text = "";
            this.Border.BorderStyle = Terminal.Gui.BorderStyle.Single;
            this.Border.BorderBrush = Terminal.Gui.Color.Blue;
            this.Border.Effect3D = false;
            this.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Title = "User Settings";
            this.tabView1 = new Terminal.Gui.TabView();
            this.tabView1.Width = Dim.Fill(0);
            this.tabView1.Height = Dim.Fill(0);
            this.tabView1.X = 0;
            this.tabView1.Y = 0;
            this.tabView1.Data = "tabView1";
            this.tabView1.Text = "";
            this.tabView1.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.tabView1.MaxTabTextWidth = 30u;
            this.tabView1.Style.ShowBorder = true;
            this.tabView1.Style.ShowTopLine = true;
            this.tabView1.Style.TabsOnBottom = true;
            Terminal.Gui.TabView.Tab tabView1Settings;
            tabView1Settings = new Terminal.Gui.TabView.Tab("Settings", new View());
            tabView1Settings.View.Width = Dim.Fill();
            tabView1Settings.View.Height = Dim.Fill();
            this.cbHideCohortBuilderContainersInFind = new Terminal.Gui.CheckBox();
            this.cbHideCohortBuilderContainersInFind.Width = 4;
            this.cbHideCohortBuilderContainersInFind.Height = 1;
            this.cbHideCohortBuilderContainersInFind.X = 0;
            this.cbHideCohortBuilderContainersInFind.Y = 0;
            this.cbHideCohortBuilderContainersInFind.Data = "cbHideCohortBuilderContainersInFind";
            this.cbHideCohortBuilderContainersInFind.Text = "Hide CohortBuilder Containers In Find";
            this.cbHideCohortBuilderContainersInFind.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.cbHideCohortBuilderContainersInFind.Checked = false;
            tabView1Settings.View.Add(this.cbHideCohortBuilderContainersInFind);
            this.cbStrictValidationForContainers = new Terminal.Gui.CheckBox();
            this.cbStrictValidationForContainers.Width = 4;
            this.cbStrictValidationForContainers.Height = 1;
            this.cbStrictValidationForContainers.X = 0;
            this.cbStrictValidationForContainers.Y = 1;
            this.cbStrictValidationForContainers.Data = "cbStrictValidationForContainers";
            this.cbStrictValidationForContainers.Text = "Strict Validation For Containers";
            this.cbStrictValidationForContainers.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.cbStrictValidationForContainers.Checked = false;
            tabView1Settings.View.Add(this.cbStrictValidationForContainers);
            this.cbHideEmptyTableLoadRunAudits = new Terminal.Gui.CheckBox();
            this.cbHideEmptyTableLoadRunAudits.Width = 4;
            this.cbHideEmptyTableLoadRunAudits.Height = 1;
            this.cbHideEmptyTableLoadRunAudits.X = 0;
            this.cbHideEmptyTableLoadRunAudits.Y = 2;
            this.cbHideEmptyTableLoadRunAudits.Data = "cbHideEmptyTableLoadRunAudits";
            this.cbHideEmptyTableLoadRunAudits.Text = "Hide Empty Table Load Run Audits";
            this.cbHideEmptyTableLoadRunAudits.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.cbHideEmptyTableLoadRunAudits.Checked = false;
            tabView1Settings.View.Add(this.cbHideEmptyTableLoadRunAudits);
            this.cbShowPipelineCompletedPopup = new Terminal.Gui.CheckBox();
            this.cbShowPipelineCompletedPopup.Width = 4;
            this.cbShowPipelineCompletedPopup.Height = 1;
            this.cbShowPipelineCompletedPopup.X = 0;
            this.cbShowPipelineCompletedPopup.Y = 3;
            this.cbShowPipelineCompletedPopup.Data = "cbShowPipelineCompletedPopup";
            this.cbShowPipelineCompletedPopup.Text = "Show Pipeline Completed Popup";
            this.cbShowPipelineCompletedPopup.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.cbShowPipelineCompletedPopup.Checked = false;
            tabView1Settings.View.Add(this.cbShowPipelineCompletedPopup);
            this.cbSkipCohortBuilderValidationOnCommit = new Terminal.Gui.CheckBox();
            this.cbSkipCohortBuilderValidationOnCommit.Width = 4;
            this.cbSkipCohortBuilderValidationOnCommit.Height = 1;
            this.cbSkipCohortBuilderValidationOnCommit.X = 0;
            this.cbSkipCohortBuilderValidationOnCommit.Y = 4;
            this.cbSkipCohortBuilderValidationOnCommit.Data = "cbSkipCohortBuilderValidationOnCommit";
            this.cbSkipCohortBuilderValidationOnCommit.Text = "Skip Cohort Builder Validation on Commit";
            this.cbSkipCohortBuilderValidationOnCommit.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.cbSkipCohortBuilderValidationOnCommit.Checked = false;
            tabView1Settings.View.Add(this.cbSkipCohortBuilderValidationOnCommit);
            this.cbAlwaysJoinEverything = new Terminal.Gui.CheckBox();
            this.cbAlwaysJoinEverything.Width = 4;
            this.cbAlwaysJoinEverything.Height = 1;
            this.cbAlwaysJoinEverything.X = 0;
            this.cbAlwaysJoinEverything.Y = 5;
            this.cbAlwaysJoinEverything.Data = "cbAlwaysJoinEverything";
            this.cbAlwaysJoinEverything.Text = "Always Join Everything";
            this.cbAlwaysJoinEverything.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.cbAlwaysJoinEverything.Checked = false;
            tabView1Settings.View.Add(this.cbAlwaysJoinEverything);
            this.cbSelectiveRefresh = new Terminal.Gui.CheckBox();
            this.cbSelectiveRefresh.Width = 4;
            this.cbSelectiveRefresh.Height = 1;
            this.cbSelectiveRefresh.X = 0;
            this.cbSelectiveRefresh.Y = 6;
            this.cbSelectiveRefresh.Data = "cbSelectiveRefresh";
            this.cbSelectiveRefresh.Text = "Selective Refresh";
            this.cbSelectiveRefresh.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.cbSelectiveRefresh.Checked = false;
            tabView1Settings.View.Add(this.cbSelectiveRefresh);
            this.label1 = new Terminal.Gui.Label();
            this.label1.Width = 24;
            this.label1.Height = 1;
            this.label1.X = 0;
            this.label1.Y = 8;
            this.label1.Data = "label1";
            this.label1.Text = "Create Database Timeout:";
            this.label1.TextAlignment = Terminal.Gui.TextAlignment.Left;
            tabView1Settings.View.Add(this.label1);
            this.label2 = new Terminal.Gui.Label();
            this.label2.Width = 24;
            this.label2.Height = 1;
            this.label2.X = 0;
            this.label2.Y = 9;
            this.label2.Data = "label2";
            this.label2.Text = "Archive Trigger Timeout:";
            this.label2.TextAlignment = Terminal.Gui.TextAlignment.Left;
            tabView1Settings.View.Add(this.label2);
            this.tbCreateDatabaseTimeout = new Terminal.Gui.TextField();
            this.tbCreateDatabaseTimeout.Width = 10;
            this.tbCreateDatabaseTimeout.Height = 1;
            this.tbCreateDatabaseTimeout.X = 25;
            this.tbCreateDatabaseTimeout.Y = 8;
            this.tbCreateDatabaseTimeout.Data = "tbCreateDatabaseTimeout";
            this.tbCreateDatabaseTimeout.Text = "";
            this.tbCreateDatabaseTimeout.TextAlignment = Terminal.Gui.TextAlignment.Left;
            tabView1Settings.View.Add(this.tbCreateDatabaseTimeout);
            this.tbArchiveTriggerTimeout = new Terminal.Gui.TextField();
            this.tbArchiveTriggerTimeout.Width = 10;
            this.tbArchiveTriggerTimeout.Height = 1;
            this.tbArchiveTriggerTimeout.X = 25;
            this.tbArchiveTriggerTimeout.Y = 9;
            this.tbArchiveTriggerTimeout.Data = "tbArchiveTriggerTimeout";
            this.tbArchiveTriggerTimeout.Text = "";
            this.tbArchiveTriggerTimeout.TextAlignment = Terminal.Gui.TextAlignment.Left;
            tabView1Settings.View.Add(this.tbArchiveTriggerTimeout);
            tabView1.AddTab(tabView1Settings, false);
            Terminal.Gui.TabView.Tab tabView1ErrorCodes;
            tabView1ErrorCodes = new Terminal.Gui.TabView.Tab("ErrorCodes", new View());
            tabView1ErrorCodes.View.Width = Dim.Fill();
            tabView1ErrorCodes.View.Height = Dim.Fill();
            this.tableview1 = new Terminal.Gui.TableView();
            this.tableview1.Width = Dim.Fill(0);
            this.tableview1.Height = Dim.Fill(0);
            this.tableview1.X = 0;
            this.tableview1.Y = 0;
            this.tableview1.Data = "tableview1";
            this.tableview1.Text = "";
            this.tableview1.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.tableview1.Style.AlwaysShowHeaders = false;
            this.tableview1.Style.ExpandLastColumn = true;
            this.tableview1.Style.InvertSelectedCellFirstCharacter = false;
            this.tableview1.Style.ShowHorizontalHeaderOverline = true;
            this.tableview1.Style.ShowHorizontalHeaderUnderline = true;
            this.tableview1.Style.ShowVerticalCellLines = true;
            this.tableview1.Style.ShowVerticalHeaderLines = true;
            System.Data.DataTable tableview1Table;
            tableview1Table = new System.Data.DataTable();
            System.Data.DataColumn tableview1TableCol0;
            tableview1TableCol0 = new System.Data.DataColumn();
            tableview1TableCol0.ColumnName = "Code";
            tableview1Table.Columns.Add(tableview1TableCol0);
            System.Data.DataColumn tableview1TableCol1;
            tableview1TableCol1 = new System.Data.DataColumn();
            tableview1TableCol1.ColumnName = "Treatment";
            tableview1Table.Columns.Add(tableview1TableCol1);
            System.Data.DataColumn tableview1TableCol2;
            tableview1TableCol2 = new System.Data.DataColumn();
            tableview1TableCol2.ColumnName = "Error Message";
            tableview1Table.Columns.Add(tableview1TableCol2);
            this.tableview1.Table = tableview1Table;
            tabView1ErrorCodes.View.Add(this.tableview1);
            tabView1.AddTab(tabView1ErrorCodes, false);
            this.tabView1.ApplyStyleChanges();
            this.Add(this.tabView1);
        }
    }
}