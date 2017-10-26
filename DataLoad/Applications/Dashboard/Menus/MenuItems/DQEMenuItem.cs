﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using CatalogueManager.Menus.MenuItems;
using Dashboard.CommandExecution.AtomicCommands;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace Dashboard.Menus.MenuItems
{
    public class DQEMenuItem:RDMPToolStripMenuItem
    {
        public DQEMenuItem(IActivateItems activator, Catalogue catalogue): base(activator, "Data Quality Engine")
        {
            var defaults = new ServerDefaults(activator.RepositoryLocator.CatalogueRepository);
            var dqeServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.DQE);
            var iconProvider = activator.CoreIconProvider;

            Image = activator.CoreIconProvider.GetImage(RDMPConcept.DQE);

            if (dqeServer == null)
            {
                var cmdCreateDb = new ExecuteCommandCreateNewExternalDatabaseServer(_activator, typeof(DataQualityEngine.Database.Class1).Assembly, ServerDefaults.PermissableDefaults.DQE);
                cmdCreateDb.OverrideCommandName = "Create DQE Database";
                Add(cmdCreateDb);

                DropDownItems.Add("Create link to existing DQE Database", iconProvider.GetImage(RDMPConcept.Database, OverlayKind.Link), (s, e) => LaunchManageExternalServers());
                DropDownItems.Add(new ToolStripSeparator());
            }
            else
            {
                Exception ex;

                if (!dqeServer.RespondsWithinTime(5, DataAccessContext.InternalDataProcessing, out ex))
                {
                    DropDownItems.Add(new ToolStripMenuItem("Data Quality Engine Server Broken!", _activator.CoreIconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Problem), (s, e) => ExceptionViewer.Show(ex)));
                    return;
                }
            }

            var timeCoverIdmissing = catalogue.TimeCoverage_ExtractionInformation_ID == null;

            string timeCoverageText = timeCoverIdmissing ? "Pick Time Coverage Column (Required)" : "Change Time Coverage Column";
            Image timeCoverageIcon = iconProvider.GetImage(RDMPConcept.TimeCoverageField, timeCoverIdmissing ? OverlayKind.Problem : OverlayKind.None);

            DropDownItems.Add(timeCoverageText, timeCoverageIcon, (s, e) => ChooseTimeCoverageExtractionInformation(catalogue));

            string pivotText = catalogue.PivotCategory_ExtractionInformation_ID == null ? "Pick Pivot Category Column (Optional)" : "Change Pivot Category Column";
            DropDownItems.Add(pivotText, CatalogueIcons.PivotField, (s, e) => ChoosePivotCategoryExtractionInformation(catalogue));

            Add(new ExecuteCommandConfigureCatalogueValidationRules(_activator).SetTarget(catalogue));

            DropDownItems.Add(new ToolStripSeparator());

            Add(new ExecuteCommandRunDQEOnCatalogue(_activator).SetTarget(catalogue));

            DropDownItems.Add(new ToolStripSeparator());

            Add(new ExecuteCommandViewDQEResultsForCatalogue(_activator).SetTarget(catalogue));
        }
        
        
        private void LaunchManageExternalServers()
        {
            var manageServers = new ManageExternalServers(_activator.CoreIconProvider);
            manageServers.RepositoryLocator = _activator.RepositoryLocator;
            manageServers.Show();
        }
        
        private void ChooseTimeCoverageExtractionInformation(Catalogue c)
        {
            //fire up a chooser for the current value
            DialogResult dr;

            var extractionInformation = SelectAppropriateExtractionInformation(c, c.TimeCoverage_ExtractionInformation_ID, out dr);

            //if the user chose a new value
            if (dr == DialogResult.OK)
            {
                //set the Catalogues property to the new value
                if (extractionInformation != null)
                    c.TimeCoverage_ExtractionInformation_ID = extractionInformation.ID;
                else
                    c.TimeCoverage_ExtractionInformation_ID = null;

                c.SaveToDatabase();
            }
        }
        private void ChoosePivotCategoryExtractionInformation(Catalogue c)
        {
            //fire up a chooser for the current value
            DialogResult dr;
            var extractionInformation = SelectAppropriateExtractionInformation(c, c.PivotCategory_ExtractionInformation_ID, out dr);

            //if the user chose a new value
            if (dr == DialogResult.OK)
            {
                //set the Catalogues property to the new value
                if (extractionInformation != null)
                    c.PivotCategory_ExtractionInformation_ID = extractionInformation.ID;
                else
                    c.PivotCategory_ExtractionInformation_ID = null;

                c.SaveToDatabase();
            }
        }

        private ExtractionInformation SelectAppropriateExtractionInformation(Catalogue cata, int? oldValue, out DialogResult dialogResult)
        {
            if (cata != null)
            {
                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(cata.GetAllExtractionInformation(ExtractionCategory.Any), true, false);
                dialogResult = dialog.ShowDialog();

                return dialog.Selected as ExtractionInformation;
            }

            dialogResult = DialogResult.Ignore;
            return null;
        }
    
    }
}
