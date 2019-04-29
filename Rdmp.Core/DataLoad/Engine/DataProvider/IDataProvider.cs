// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.CatalogueLibrary;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Engine.DataProvider
{
    /// <summary>
    /// DLE component ostensibly responsible for 'fetching data'.  This typically involves fetching data and saving it into the ILoadDirectory (e.g. into 
    /// ForLoading) ready for loading by later components.
    /// </summary>
    public interface IDataProvider : IDisposeAfterDataLoad,ICheckable
    {
        void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo);
        ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken);
    }
}