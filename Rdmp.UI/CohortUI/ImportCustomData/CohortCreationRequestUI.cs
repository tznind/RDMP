// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Checks;

using Point = System.Drawing.Point;

namespace Rdmp.UI.CohortUI.ImportCustomData
{
    /// <summary>
    /// Once you have created a cohort database, this dialog lets you upload a new cohort into it.  You will already have selected a file which contains the private patient identifiers of
    /// those you wish to be in the cohort.  Next you must create or choose an existing Project for which the cohort belongs.  
    /// 
    /// <para>Once you have chosen the project you can choose to either create a new cohort for use with the project (use this if you have multiple cohorts in the project e.g. 'Cases' and 
    /// 'Controls').  Or 'Revised version of existing cohort' for if you made a mistake with your first version of a cohort or if you are doing a refresh of the cohort (e.g. after 5 years
    /// it is likely there will be different patients that match the research study criteria so a new version of the cohort is appropriate).</para>
    /// </summary>
    public partial class CohortCreationRequestUI : RDMPForm
    {
        private readonly IExternalCohortTable _target;
        private IDataExportRepository _repository;
        
        public string CohortDescription
        {
            get { return tbDescription.Text; }
            set { tbDescription.Text = value; }
        }

        public CohortCreationRequestUI(IActivateItems activator,IExternalCohortTable target, IProject project =null):base(activator)
        {
            _target = target;

            InitializeComponent();
            
            if (_target == null)
                return;

            _repository = (IDataExportRepository)_target.Repository;

            lblExternalCohortTable.Text = _target.ToString();

            SetProject(project);
            
            pbProject.Image = CatalogueIcons.Project;
            pbCohortSource.Image = CatalogueIcons.ExternalCohortTable;
            taskDescriptionLabel1.SetupFor(new DialogArgs
            {
                TaskDescription = "Describe the cohort you are trying to create.  Which Project it will be extracted with and which ExternalCohortTable it should be stored in.",
            });
        }

        
        public CohortCreationRequest Result { get; set; }
        public IProject Project { get; set; }

        private void btnOk_Click(object sender, EventArgs e)
        {

            if (Project == null)
            {
                MessageBox.Show("You must select a project, if you do not have one yet then create one");
                return;
            }

            if (Project.ProjectNumber == null)
            {
                MessageBox.Show("Project " + Project +
                                " does not have a project number yet, you must asign it one before it can be involved in cohort creation");
                return;
            }

            string name;
            int version;
            if (rbNewCohort.Checked)
            {
                name = tbName.Text;
                version = 1;

                if (string.IsNullOrWhiteSpace(tbName.Text))
                {
                    MessageBox.Show("You must enter a name for your cohort");
                    return;
                }

            }
            else if (rbRevisedCohort.Checked)
            {

                var existing = ddExistingCohort.SelectedItem as ExtractableCohort;
                if (existing == null)
                {
                    MessageBox.Show("You must select an existing cohort");
                    return;
                }

                name = existing.GetExternalData().ExternalDescription;
                version = int.Parse(lblNewVersionNumber.Text);
            }
            else
            {
                MessageBox.Show("Select either new or existing cohort");
                return;
            }

            
            //construct the result
            Result = new CohortCreationRequest(Project, new CohortDefinition(null, name, version, (int)Project.ProjectNumber, _target), (IDataExportRepository)Project.Repository, tbDescription.Text);
            
            Result.NewCohortDefinition.CohortReplacedIfAny = ddExistingCohort.SelectedItem as ExtractableCohort;
            
            //see if it is passing checks
            ToMemoryCheckNotifier notifier = new ToMemoryCheckNotifier();
            Result.Check(notifier);
            if (notifier.GetWorst() <= CheckResult.Warning)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Checks must pass before continuing");

                //if it is not passing checks display the results of the failing checking
                ragSmiley1.Reset();
                Result.Check(ragSmiley1);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Result = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void rbNewCohort_CheckedChanged(object sender, EventArgs e)
        {
            gbNewCohort.Enabled = true;
            gbRevisedCohort.Enabled = false;
        }

        private void rbRevisedCohort_CheckedChanged(object sender, EventArgs e)
        {
            gbNewCohort.Enabled = false;
            gbRevisedCohort.Enabled = true;

            
            RefreshCohortsDropdown(true);
        }

        private void CohortCreationRequestUI_Load(object sender, EventArgs e)
        {
            _target.Check(ragSmiley1);
        }

        private void ddExistingCohort_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cohort = ddExistingCohort.SelectedItem as ExtractableCohort;

            if (cohort != null)
            {
                lblNewVersionNumber.Text = (cohort.ExternalVersion + 1).ToString();
                tbExistingCohortSource.Text = cohort.ExternalCohortTable.Name;
                tbExistingVersion.Text = cohort.ExternalVersion.ToString();

                if (!string.IsNullOrWhiteSpace(cohort.AuditLog))
                {
                    existingHelpIcon.SetHelpText("Comment", cohort.AuditLog);
                    existingHelpIcon.Enabled = true;
                }
                else
                    existingHelpIcon.Enabled = false;
            }
            else
            {
                lblNewVersionNumber.Text = "";
                tbExistingCohortSource.Text = "";
                tbExistingVersion.Text = "";
                existingHelpIcon.Enabled = false;
            }
        }

        private void RefreshCohortsDropdown(bool interactive)
        {
            ddExistingCohort.Items.Clear();

            if (Project == null)
            {
                if(interactive)
                {
                    MessageBox.Show("You must select a Project");
                }
                    
                return;
            }

            var cohorts = ((DataExportChildProvider)Activator.CoreChildProvider).Cohorts.Where(c => c.ExternalProjectNumber == Project.ProjectNumber);

            var maxVersionCohorts = cohorts.GroupBy(x => x.GetExternalData().ExternalDescription, (key, g) => g.OrderByDescending(e => e.ExternalVersion).First()).ToArray();
            ddExistingCohort.Items.AddRange(maxVersionCohorts);
        }

        private void btnNewProject_Click(object sender, EventArgs e)
        {
            try
            {
                ProjectUI.ProjectUI p = new ProjectUI.ProjectUI();
                RDMPForm dialog = new RDMPForm(Activator);

                p.SwitchToCutDownUIMode();

                Button ok = new Button();
                ok.Click += (s, ev) => { dialog.Close(); dialog.DialogResult = DialogResult.OK; };
                ok.Location = new Point(0,p.Height + 10);
                ok.Width = p.Width/2;
                ok.Height = 30;
                ok.Text = "Ok";

                Button cancel = new Button();
                cancel.Click += (s, ev) =>{dialog.Close();dialog.DialogResult = DialogResult.Cancel;};
                cancel.Location = new Point(p.Width / 2, p.Height + 10);
                cancel.Width = p.Width / 2;
                cancel.Height = 30;
                cancel.Text = "Cancel";

                dialog.Controls.Add(ok);
                dialog.Controls.Add(cancel);
                    
                dialog.Height = p.Height + 80;
                dialog.Width = p.Width + 10;
                dialog.Controls.Add(p);
                
                ok.Anchor = AnchorStyles.Bottom;
                cancel.Anchor  = AnchorStyles.Bottom;

                var project = new Project(_repository, "New Project");
                p.SetDatabaseObject(Activator,project);
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    project.SaveToDatabase();
                    SetProject(project);
                    Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(project));
                }
                else
                    project.DeleteInDatabase();

            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void SetProject(IProject project)
        {
            Project = project;

            lblProject.Text = Project != null ? Project.Name : "????";

            // Clear any old selected cohort because they have changed the Project
            ddExistingCohort.SelectedItem = null;
            RefreshCohortsDropdown(false);

            // Delay the cohort Type (new or update existing) until they have picked a Project
            gbChooseCohortType.Enabled = project != null;

            btnNewProject.Left = lblProject.Right;
            btnExisting.Left = btnNewProject.Right;

            btnClear.Left = lblProject.Right;

            btnNewProject.Visible = Project == null;
            btnExisting.Visible = Project == null;
            btnClear.Visible = Project != null;
            
            //if a project is selected and the project has no project number
            lblErrorNoProjectNumber.Visible = Project != null && Project.ProjectNumber == null;
            tbSetProjectNumber.Visible = Project != null && Project.ProjectNumber == null;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnExisting_Click(object sender, EventArgs e)
        {
            if(Activator.SelectObject(new DialogArgs
            {
                TaskDescription = "Choose a Project which this cohort will be associated with.  This will set the cohorts ProjectNumber.  A cohort can only be extracted from a Project whose ProjectNumber matches the cohort (multiple Projects are allowed to have the same ProjectNumber)"
            }, Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>(), out var proj))
            {
                SetProject(proj);
            }                
        }

        private void tbSetProjectNumber_TextChanged(object sender, EventArgs e)
        {
            try
            {
                tbSetProjectNumber.ForeColor = Color.Black;
                int newProjectNumber = int.Parse(tbSetProjectNumber.Text);
                Project.ProjectNumber = newProjectNumber;
                Project.SaveToDatabase();
            }
            catch (Exception)
            {
                tbSetProjectNumber.ForeColor = Color.Red;
            }

            lblErrorNoProjectNumber.ForeColor = tbSetProjectNumber.ForeColor;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            SetProject(null);
        }

    }
}
