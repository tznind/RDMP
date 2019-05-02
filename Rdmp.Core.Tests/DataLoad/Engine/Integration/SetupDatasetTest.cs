// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Diagnostics;
using Diagnostics.TestData;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Checks;
using Rdmp.Core.DataLoad.Engine.Checks.Checkers;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.LoadExecution;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration
{
    [Category("Database")]
    class SetupDatasetTest : TestsRequiringFullAnonymisationSuite, IDataLoadEventListener
    {

        /// <summary>
        /// Ensure that the SetupDatasetTest functionality cleans up properly
        /// </summary>
        [Test]
        public void TestTheTestDatasetSetup()
        {
            var testEnvironmentHelper = new UserAcceptanceTestEnvironmentHelper(
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder.ConnectionString,
                CatalogueRepository.ConnectionString,
                UnitTestLoggingConnectionString.ConnectionString,
                ANOStore_Database.Server.Builder.ConnectionString,
                IdentifierDump_Database.Server.Builder.ConnectionString,
                RepositoryLocator
            );

            testEnvironmentHelper.SetUp();

            // Has the test data table been created
            Assert.IsTrue(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("DMP_Test").Exists());
            
            testEnvironmentHelper.TearDown();

            // Has it cleaned up after itself?
            Assert.IsFalse(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("DMP_Test").Exists());
        }
        
        [Test]
        public void EndToEnd_TestDatasetEnvironment()
        {
            //stage1 is to setup the test environment
            //stage2 is to run the data load
            //stage3 is to setup the extraction 
            //stage4 is to perform the extraction
            var rootFolder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            var testFolder = rootFolder.CreateSubdirectory("TestTheTestDatasetSetup");
            var datasetFolder = testFolder.CreateSubdirectory("TestDataset");
            
            var stage1_setupCatalogue = new UserAcceptanceTestEnvironment(
                (SqlConnectionStringBuilder) DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder,
                datasetFolder.FullName, UnitTestLoggingConnectionString, "Internal",
                (SqlConnectionStringBuilder) ANOStore_Database.Server.Builder,
                (SqlConnectionStringBuilder) IdentifierDump_Database.Server.Builder,
                RepositoryLocator);
            
            try
            {
                stage1_setupCatalogue.Check(new AcceptAllCheckNotifier());

                Catalogue testCatalogue =
                    CatalogueRepository.GetAllObjects<Catalogue>().Single(c => c.Name.Equals(UserAcceptanceTestEnvironment.CatalogueName));

                var loadMetadata = testCatalogue.LoadMetadata;
                var configuration = new HICDatabaseConfiguration(loadMetadata);

                //run pre execution checker
                var loadConfigurationFlags = new HICLoadConfigurationFlags();
                CheckEntireDataLoadProcess checker = new CheckEntireDataLoadProcess(loadMetadata, configuration, loadConfigurationFlags, CatalogueRepository.MEF);
                checker.Check(new AcceptAllCheckNotifier());

                if(testCatalogue.LiveLoggingServer_ID == null)
                    throw new NullReferenceException("No logging server was configured for test catalogue");
                
                var loggingServer = DataAccessPortal.GetInstance().ExpectServer(testCatalogue.LiveLoggingServer, DataAccessContext.Logging);
                var logManager = new LogManager(loggingServer);
                var preExecutionChecker = new PreExecutionChecker(loadMetadata, configuration);
                var pipelineFactory = new HICDataLoadFactory(loadMetadata, configuration, loadConfigurationFlags, CatalogueRepository, logManager);
                var pipeline = pipelineFactory.Create(new ThrowImmediatelyDataLoadEventListener());
                var stage2_ExecuteDataLoad = new DataLoadProcess(RepositoryLocator,loadMetadata, preExecutionChecker, logManager, new ThrowImmediatelyDataLoadEventListener(), pipeline,configuration);
                
                stage2_ExecuteDataLoad.Run(new GracefulCancellationToken());
                Assert.IsTrue(stage2_ExecuteDataLoad.ExitCode == ExitCodeType.Success);

                Assert.AreEqual(0,exceptionsEncountered.Count);
            }
            finally
            {
                testFolder.Delete(true);
            }
            
            stage1_setupCatalogue.DestroyEnvironment();
        }

        List<Exception> exceptionsEncountered = new List<Exception>();
 
        public void OnNotify(object sender, NotifyEventArgs e)
        {
            Console.WriteLine(e.ProgressEventType + ":" + e.Message + Environment.NewLine + e.Exception);

            if (e.Exception != null)
                exceptionsEncountered.Add(e.Exception);

            if (e.ProgressEventType == ProgressEventType.Error)
                exceptionsEncountered.Add(new Exception(e.Message));
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            
        }

        [Test]
        public void TestDuplicationInDemography()
        {
            Random r = new Random();

            List<TestPerson> testPersons = new List<TestPerson>(9000);

            for(int i=0;i<testPersons.Capacity;i++)
                testPersons.Add(new TestPerson(r));

            Assert.AreEqual(testPersons.Select(t => t.ANOCHI).Distinct().Count(), testPersons.Count);
            Assert.AreEqual(testPersons.Select(t => t.CHI).Distinct().Count(), testPersons.Count);
        }
    }
}