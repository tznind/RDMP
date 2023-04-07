// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.DataRelease.Potential;

/// <summary>
/// Determines whether a given ExtractableDataSet in an ExtractionConfiguration, as generated by a Flat File extraction configuration is ready for Release. 
/// This includes making sure that the current configuration
/// in the database matches the extracted flat files that are destined for release.  It also checks that the user hasn't snuck some additional files into
/// the extract directory etc.
/// </summary>
public class FlatFileReleasePotential : ReleasePotential
{
    public FlatFileReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISelectedDataSets selectedDataSets): base(repositoryLocator, selectedDataSets)
    {
    }

    protected override Releaseability GetSupplementalSpecificAssessment(IExtractionResults supplementalExtractionResults)
    {
        if (File.Exists(supplementalExtractionResults.DestinationDescription))
            return Releaseability.Undefined;

        return Releaseability.ExtractFilesMissing;
    }

    protected override Releaseability GetSpecificAssessment(IExtractionResults extractionResults)
    {
        ExtractDirectory = new FileInfo(extractionResults.DestinationDescription).Directory;
        if (FilesAreMissing(extractionResults))
            return Releaseability.ExtractFilesMissing;
            
        ThrowIfPollutionFoundInConfigurationRootExtractionFolder();
        return Releaseability.Undefined;// Assesment = SqlDifferencesVsLiveCatalogue() ? Releaseability.ColumnDifferencesVsCatalogue : Releaseability.Releaseable;
    }

    private bool FilesAreMissing(IExtractionResults extractionResults)
    {
        ExtractFile = new FileInfo(extractionResults.DestinationDescription);
        var metadataFile = new FileInfo(extractionResults.DestinationDescription.Replace(".csv", ".docx"));

        if (!ExtractFile.Exists)
            return true;//extract is missing

        if (!ExtractFile.Extension.Equals(".csv"))
            throw new Exception("Extraction file had extension '" + ExtractFile.Extension + "' (expected .csv)");

        if (!metadataFile.Exists)
            return true;

        //see if there is any other polution in the extract directory
        FileInfo unexpectedFile = ExtractFile.Directory.EnumerateFiles().FirstOrDefault(f =>
            !(f.Name.Equals(ExtractFile.Name) || f.Name.Equals(metadataFile.Name)));

        if (unexpectedFile != null)
            throw new Exception("Unexpected file found in extract directory " + unexpectedFile.FullName + " (pollution of extract directory is not permitted)");

        DirectoryInfo unexpectedDirectory = ExtractFile.Directory.EnumerateDirectories().FirstOrDefault(d =>
            !(d.Name.Equals("Lookups") || d.Name.Equals("SupportingDocuments") || d.Name.Equals(SupportingSQLTable.ExtractionFolderName)));

        if (unexpectedDirectory != null)
            throw new Exception("Unexpected directory found in extraction directory " + unexpectedDirectory.FullName + " (pollution of extract directory is not permitted)");

        return false;
    }

    private void ThrowIfPollutionFoundInConfigurationRootExtractionFolder()
    {
        Debug.Assert(ExtractDirectory.Parent != null, "Dont call this method until you have determined that an extracted file was actually produced!");

        if (ExtractDirectory.Parent.GetFiles().Any())
            throw new Exception("The following pollutants were found in the extraction directory\" " +
                                ExtractDirectory.Parent.FullName +
                                "\" pollutants were:" +
                                ExtractDirectory.Parent.GetFiles().Aggregate("", (s, n) => s + "\"" + n + "\""));
    }
}