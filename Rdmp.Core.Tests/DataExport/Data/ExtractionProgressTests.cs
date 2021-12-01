﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport.Data
{
    class ExtractionProgressTests : DatabaseTests
    {

        [Test]
        public void ExtractionProgressConstructor_NoTimePeriodicity()
        {
            var cata = new Catalogue(CatalogueRepository, "MyCata");
            var eds = new ExtractableDataSet(DataExportRepository, cata);
            var project = new Project(DataExportRepository, "My Proj");
            var config = new ExtractionConfiguration(DataExportRepository, project);
            var sds = new SelectedDataSets(DataExportRepository, config,eds,null);

            var ex = Assert.Throws<ArgumentException>(()=> new ExtractionProgress(DataExportRepository, sds));
            Assert.AreEqual("Cannot create ExtractionProgress because Catalogue MyCata does not have a time coverage column", ex.Message);

        }

        [Test]
        public void ExtractionProgressConstructor_Normal()
        {
            Assert.DoesNotThrow(()=>CreateAnExtractionProgress());
        }

        [Test]
        public void ExtractionProgressConstructor_CannotHaveTwoAtOnce()
        {
            var progress = CreateAnExtractionProgress();

            var sds = progress.SelectedDataSets;

            // try to create a second progress for the same dataset being extracted
            var ex = Assert.Throws<SqlException>(() => new ExtractionProgress(DataExportRepository, sds));

            StringAssert.Contains("ix_OneExtractionProgressPerDataset", ex.Message);

            // now delete the original and make sure we can recreate it ok
            progress.DeleteInDatabase();
            Assert.DoesNotThrow(() => new ExtractionProgress(DataExportRepository, sds));
        }

        [Test]
        public void ExtractionProgressConstructor_DeleteSdsMustCascade()
        {
            var progress = CreateAnExtractionProgress();

            Assert.IsTrue(progress.Exists());
            progress.SelectedDataSets.DeleteInDatabase();
            Assert.IsFalse(progress.Exists());
        }

        private ExtractionProgress CreateAnExtractionProgress()
        {
            var cata = new Catalogue(CatalogueRepository, "MyCata");
            var cataItem = new CatalogueItem(CatalogueRepository, cata, "MyCol");
            var table = new TableInfo(CatalogueRepository, "MyTable");
            var col = new ColumnInfo(CatalogueRepository, "mycol", "datetime", table);

            var ei = new ExtractionInformation(CatalogueRepository, cataItem, col, "mycol");
            cata.TimeCoverage_ExtractionInformation_ID = ei.ID;
            cata.SaveToDatabase();

            var eds = new ExtractableDataSet(DataExportRepository, cata);
            var project = new Project(DataExportRepository, "My Proj");
            var config = new ExtractionConfiguration(DataExportRepository, project);
            var sds = new SelectedDataSets(DataExportRepository, config, eds, null);
            
            return new ExtractionProgress(DataExportRepository, sds);
        }
    }
}
