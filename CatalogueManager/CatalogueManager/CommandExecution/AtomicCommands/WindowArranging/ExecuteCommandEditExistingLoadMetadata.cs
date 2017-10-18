using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandEditExistingLoadMetadata : BasicCommandExecution, IAtomicCommandWithTarget
    {
        private readonly IActivateItems _activator;

        public LoadMetadata LoadMetadata{ get; set; }

        public ExecuteCommandEditExistingLoadMetadata(IActivateItems activator)
        {
            this._activator = activator;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.LoadMetadata, OverlayKind.Edit);
        }

        public void SetTarget(DatabaseEntity target)
        {
            LoadMetadata = (LoadMetadata)target;
        }

        public override string GetCommandHelp()
        {
            return "This will take you to the Data Load Configurations list and allow you to Edit the Load (change which modules execute, which Catalogues are loaded etc)." +
                   "\r\n" +
                   "You must choose a Load from the list before proceeding.";
        }

        public override string GetCommandName()
        {
            return "Edit/Execute Data Load Configuration";
        }

        public override void Execute()
        {
            if (LoadMetadata == null)
                SetImpossible("You must choose a LoadMetadata.");

            base.Execute();
            _activator.WindowArranger.SetupEditLoadMetadata(this, LoadMetadata);
        }
    }
}