// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations
{
    /// <summary>
    /// Pipeline component for renaming a single column in DataTables passing through the component.
    /// <para>Renames a column with a given name to have a new name e.g. 'mCHI' to 'CHI'</para>
    /// </summary>
    public class ColumnDropper : IPluginDataFlowComponent<DataTable>
    {
        [DemandsInitialization("Looks for a column with exactly this name", Mandatory = true)]
        public string ColumnNameToDrop { get; set; }


        [DemandsInitialization("In relaxed mode the pipeline will not be crashed if the column does not appear.  Default is false i.e. the column MUST appear.", Mandatory = true, DefaultValue = false)]
        public bool RelaxedMode { get; set; }



        public DataTable ProcessPipelineData( DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (!toProcess.Columns.Contains(ColumnNameToDrop))
                if (RelaxedMode)
                    return toProcess;
                else
                    throw new InvalidOperationException("The column to be dropped (" + ColumnNameToDrop + ") does not exist in the supplied data table and RelaxedMode is off. Check that this component is configured correctly, or if any upstream components are removing this column unexpectedly.");

            toProcess.Columns.Remove(ColumnNameToDrop);

            return toProcess;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {

        }

        public void Abort(IDataLoadEventListener listener)
        {

        }

        public void Check(ICheckNotifier notifier)
        {
            if (string.IsNullOrWhiteSpace(ColumnNameToDrop))
                notifier.OnCheckPerformed(new CheckEventArgs("No value specified for argument ColumnNameToDrop",CheckResult.Fail));
        }
    }
}
