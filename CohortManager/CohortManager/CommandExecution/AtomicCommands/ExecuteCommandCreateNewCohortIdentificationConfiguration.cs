using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using CohortManager.Wizard;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CohortManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewCohortIdentificationConfiguration: BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandCreateNewCohortIdentificationConfiguration(IActivateItems activator) : base(activator)
        {
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration,OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();
            var wizard = new CreateNewCohortIdentificationConfigurationUI(Activator);

            if(wizard.ShowDialog() == DialogResult.OK)
            {
                var cic = wizard.CohortIdentificationCriteriaCreatedIfAny;
                if(cic == null)
                    return;

                Publish(cic);
                Activator.RequestItemEmphasis(this, new EmphasiseRequest(cic, int.MaxValue));
                Activate(cic);
            }   
        }

        public override string GetCommandHelp()
        {
            return
                "This will open a window which will guide you in the steps for creating a Cohort based on Inclusion and Exclusion criteria.\r\n" +
                "You will be asked to choose one or more Dataset and the associated column filters to use as inclusion or exclusion criteria for the cohort.";
        }
    }
}