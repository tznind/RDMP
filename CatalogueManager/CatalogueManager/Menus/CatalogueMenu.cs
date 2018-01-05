﻿using System;
using System.Windows.Forms;
using CatalogueLibrary.Cloning;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.ChecksUI;
using CatalogueLibrary.Data;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class CatalogueMenu:RDMPContextMenuStrip
    {
        public CatalogueMenu(RDMPContextMenuStripArgs args, CatalogueFolder folder) : base(args,null)
        {
            AddImportOptions();
        }

        public CatalogueMenu(RDMPContextMenuStripArgs args, Catalogue catalogue):base(args,catalogue)
        {
            //create right click context menu
            Add(new ExecuteCommandViewCatalogueExtractionSql(_activator).SetTarget(catalogue));

            Items.Add("View Checks", CatalogueIcons.Warning, (s, e) => PopupChecks(catalogue));

            Items.Add(new ToolStripSeparator());

            var addItem = new ToolStripMenuItem("Add", null);
            Add(new ExecuteCommandAddNewSupportingSqlTable(_activator, catalogue), Keys.None, addItem);
            Add(new ExecuteCommandAddNewSupportingDocument(_activator, catalogue), Keys.None, addItem);
            Add(new ExecuteCommandAddNewAggregateGraph(_activator, catalogue), Keys.None, addItem);
            Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, catalogue,null), Keys.None, addItem);
            Add(new ExecuteCommandAddNewCatalogueItem(_activator, catalogue), Keys.None, addItem);

            Items.Add(addItem);

            Items.Add(new ToolStripSeparator());

            Items.Add("Clone Catalogue", null, (s, e) => CloneCatalogue(catalogue));
            /////////////////////////////////////////////////////////////Catalogue Items sub menu///////////////////////////
            Items.Add(new ToolStripSeparator());

            AddImportOptions();
        }

        private void AddImportOptions()
        {
            //Things that are always visible regardless
            Add(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator));
            Add(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator, true));
            Add(new ExecuteCommandCreateNewEmptyCatalogue(_activator));
        }


        private void CloneCatalogue(Catalogue c)
        {

            if (c != null)
            {
                if (DialogResult.Yes ==
                    MessageBox.Show("Confirm creating a duplicate of Catalogue \"" + c.Name + "\"?",
                                    "Create Duplicate?", MessageBoxButtons.YesNo))
                {

                    try
                    {
                        CatalogueCloner cloner = new CatalogueCloner(RepositoryLocator.CatalogueRepository, RepositoryLocator.CatalogueRepository);
                        cloner.CreateDuplicateInSameDatabase(c);

                    }
                    catch (Exception exception)
                    {
                        if (exception.Message.Contains("runcated"))
                            //skip the t because unsure what capitalisation it will be
                            MessageBox.Show(
                                "The name of the Catalogue to clone was too long, when we tried to put _DUPLICATE on the end it resulted in error:" +
                                exception.Message);
                        else
                            MessageBox.Show(exception.Message);
                    }
                }
            }
            else
                MessageBox.Show("Select a Catalogue first (on the left)");
        }

        public void PopupChecks(ICheckable checkable)
        {
            var popupChecksUI = new PopupChecksUI("Checking " + checkable, false);
            popupChecksUI.StartChecking(checkable);
        }
    }
}
