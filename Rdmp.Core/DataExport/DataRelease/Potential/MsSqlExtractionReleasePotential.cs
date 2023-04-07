// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataExport.DataRelease.Potential;

/// <summary>
/// Determines whether a given ExtractableDataSet in an ExtractionConfiguration, as generated by an Extracion To DB pipeline is ready for Release. 
/// This includes making sure that the current configuration in the database matches the extracted tables that are destined for release.
/// </summary>
public class MsSqlExtractionReleasePotential : ReleasePotential
{
    public MsSqlExtractionReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISelectedDataSets selectedDataSets): base(repositoryLocator, selectedDataSets)
    {
    }

    protected override Releaseability GetSupplementalSpecificAssessment(IExtractionResults supplementalExtractionResults)
    {
        if (supplementalExtractionResults.IsReferenceTo(typeof(SupportingDocument)))
            if (File.Exists(supplementalExtractionResults.DestinationDescription))
                return Releaseability.Undefined;
            else
                return Releaseability.ExtractFilesMissing;

        if (supplementalExtractionResults.IsReferenceTo(typeof (SupportingSQLTable)) ||
            supplementalExtractionResults.IsReferenceTo(typeof(TableInfo)))
            return GetSpecificAssessment(supplementalExtractionResults);

        return Releaseability.Undefined;
    }

    protected override Releaseability GetSpecificAssessment(IExtractionResults extractionResults)
    {
        var _extractDir = Configuration.GetProject().ExtractionDirectory;

        ExtractDirectory = new ExtractionDirectory(_extractDir, Configuration).GetDirectoryForDataset(DataSet);

        var externalServerId = int.Parse(extractionResults.DestinationDescription.Split('|')[0]);
        var externalServer = _repositoryLocator.CatalogueRepository.GetObjectByID<ExternalDatabaseServer>(externalServerId);
        var dbName = extractionResults.DestinationDescription.Split('|')[1];
        var tblName = extractionResults.DestinationDescription.Split('|')[2];
        var server = DataAccessPortal.GetInstance().ExpectServer(externalServer, DataAccessContext.DataExport, setInitialDatabase: false);
        var database = server.ExpectDatabase(dbName);
        if (!database.Exists())
        {
            return Releaseability.ExtractFilesMissing;
        }
        var foundTable = database.ExpectTable(tblName);
        if (!foundTable.Exists())
        {
            return Releaseability.ExtractFilesMissing;
        }
            
        return Releaseability.Undefined;
    }
}