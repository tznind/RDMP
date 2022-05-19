﻿using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Alter;
using Rdmp.Core.CommandExecution.AtomicCommands.Automation;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Sharing;
using System;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    public class TestCommandsAreSupported : UnitTests
    {
        [TestCase(typeof(ExecuteCommandAlterColumnType))]
        [TestCase(typeof(ExecuteCommandAlterTableAddArchiveTrigger))]
        [TestCase(typeof(ExecuteCommandAlterTableCreatePrimaryKey))]
        [TestCase(typeof(ExecuteCommandAlterTableMakeDistinct))]
        [TestCase(typeof(ExecuteCommandAlterTableName))]
        [TestCase(typeof(ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration))]
        [TestCase(typeof(ExecuteCommandCreateNewCatalogueByImportingExistingDataTable))]
        [TestCase(typeof(ExecuteCommandCreateNewCatalogueByImportingFile))]
        [TestCase(typeof(ExecuteCommandCreateNewCatalogueFromTableInfo))]
        [TestCase(typeof(ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration))]
        [TestCase(typeof(ExecuteCommandCreateNewCohortFromCatalogue))]
        [TestCase(typeof(ExecuteCommandCreateNewCohortFromFile))]
        [TestCase(typeof(ExecuteCommandCreateNewCohortFromTable))]
        [TestCase(typeof(ExecuteCommandImportAlreadyExistingCohort))]
        [TestCase(typeof(ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer))]
        [TestCase(typeof(ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable))]
        [TestCase(typeof(ExecuteCommandAddCatalogueToCohortIdentificationSetContainer))]
        [TestCase(typeof(ExecuteCommandAddCatalogueToGovernancePeriod))]
         [TestCase(typeof(ExecuteCommandAddCohortSubContainer))]
         [TestCase(typeof(ExecuteCommandAddCohortToExtractionConfiguration))]
         [TestCase(typeof(ExecuteCommandAddDatasetsToConfiguration))]
         [TestCase(typeof(ExecuteCommandAddDimension))]
         [TestCase(typeof(ExecuteCommandAddExtractionProgress))]
         [TestCase(typeof(ExecuteCommandAddFavourite))]
         [TestCase(typeof(ExecuteCommandAddMissingParameters))]
         [TestCase(typeof(ExecuteCommandAddNewAggregateGraph))]
         [TestCase(typeof(ExecuteCommandAddNewCatalogueItem))]
         [TestCase(typeof(ExecuteCommandAddNewExtractionFilterParameterSet))]
         [TestCase(typeof(ExecuteCommandAddNewFilterContainer))]
         [TestCase(typeof(ExecuteCommandAddNewGovernanceDocument))]
         [TestCase(typeof(ExecuteCommandAddNewSupportingDocument))]
         [TestCase(typeof(ExecuteCommandAddNewSupportingSqlTable))]
         [TestCase(typeof(ExecuteCommandAddPackageToConfiguration))]
         [TestCase(typeof(ExecuteCommandAddParameter))]
         [TestCase(typeof(ExecuteCommandAddPipelineComponent))]
         [TestCase(typeof(ExecuteCommandAddPlugins))]
         [TestCase(typeof(ExecuteCommandAssociateCatalogueWithLoadMetadata))]
        [TestCase(typeof(ExecuteCommandAssociateCohortIdentificationConfigurationWithProject))]
         [TestCase(typeof(ExecuteCommandBulkImportTableInfos))]
         [TestCase(typeof(ExecuteCommandChangeExtractability))]
        [TestCase(typeof(ExecuteCommandChangeExtractionCategory))]
         [TestCase(typeof(ExecuteCommandChangeLoadStage))]
         [TestCase(typeof(ExecuteCommandCheck))]
         [TestCase(typeof(ExecuteCommandChooseCohort))]
         [TestCase(typeof(ExecuteCommandClearQueryCache))]
         [TestCase(typeof(ExecuteCommandCloneCohortIdentificationConfiguration))]
         [TestCase(typeof(ExecuteCommandCloneExtractionConfiguration))]
         [TestCase(typeof(ExecuteCommandClonePipeline))]
         [TestCase(typeof(ExecuteCommandConfirmLogs))]
         [TestCase(typeof(ExecuteCommandConvertAggregateConfigurationToPatientIndexTable))]
         [TestCase(typeof(ExecuteCommandCreateLookup))]
         [TestCase(typeof(ExecuteCommandCreateNewANOTable))]
         [TestCase(typeof(ExecuteCommandCreateNewCacheProgress))]
         [TestCase(typeof(ExecuteCommandCreateNewClassBasedProcessTask))]
         [TestCase(typeof(ExecuteCommandCreateNewCohortIdentificationConfiguration))]
         [TestCase(typeof(ExecuteCommandCreateNewCohortStore))]
         [TestCase(typeof(ExecuteCommandCreateNewDataLoadDirectory))]
         [TestCase(typeof(ExecuteCommandCreateNewEmptyCatalogue))]
         [TestCase(typeof(ExecuteCommandCreateNewExternalDatabaseServer))]
         [TestCase(typeof(ExecuteCommandCreateNewExtractableDataSetPackage))]
        [TestCase(typeof(ExecuteCommandCreateNewExtractionConfigurationForProject))]
         [TestCase(typeof(ExecuteCommandCreateNewFileBasedProcessTask))]
         [TestCase(typeof(ExecuteCommandCreateNewFilter))]
         [TestCase(typeof(ExecuteCommandCreateNewFilterFromCatalogue))]
         [TestCase(typeof(ExecuteCommandCreateNewGovernancePeriod))]
        [TestCase(typeof(ExecuteCommandCreateNewLoadMetadata))]
         [TestCase(typeof(ExecuteCommandCreateNewLoadProgress))]
         [TestCase(typeof(ExecuteCommandCreateNewPermissionWindow))]
         [TestCase(typeof(ExecuteCommandCreateNewRemoteRDMP))]
         [TestCase(typeof(ExecuteCommandCreateNewStandardRegex))]
         [TestCase(typeof(ExecuteCommandCreatePrivateKey))]
         [TestCase(typeof(ExecuteCommandDelete))]
         [TestCase(typeof(ExecuteCommandDeprecate))]
         [TestCase(typeof(ExecuteCommandDescribe))]
        [TestCase(typeof(ExecuteCommandDescribeCommand))]
         [TestCase(typeof(ExecuteCommandDisableOrEnable))]
         [TestCase(typeof(ExecuteCommandExecuteAggregateGraph))]
         [TestCase(typeof(ExecuteCommandExportLoggedDataToCsv))]
         [TestCase(typeof(ExecuteCommandExportObjectsToFile))]
         [TestCase(typeof(ExecuteCommandExportPlugins))]
         [TestCase(typeof(ExecuteCommandExtractMetadata))]
         [TestCase(typeof(ExecuteCommandFreezeCohortIdentificationConfiguration))]
         [TestCase(typeof(ExecuteCommandFreezeExtractionConfiguration))]
         [TestCase(typeof(ExecuteCommandGenerateReleaseDocument))]
         [TestCase(typeof(ExecuteCommandGuessAssociatedColumns))]
         [TestCase(typeof(ExecuteCommandImportCatalogueItemDescription))]
         [TestCase(typeof(ExecuteCommandImportCatalogueItemDescriptions))]
         [TestCase(typeof(ExecuteCommandImportCohortIdentificationConfiguration))]
         [TestCase(typeof(ExecuteCommandImportFilterContainerTree))]
         [TestCase(typeof(ExecuteCommandImportTableInfo))]
         [TestCase(typeof(ExecuteCommandLinkCatalogueItemToColumnInfo))]
         [TestCase(typeof(ExecuteCommandList))]
         [TestCase(typeof(ExecuteCommandListSupportedCommands))]
         [TestCase(typeof(ExecuteCommandListUserSettings))]
         [TestCase(typeof(ExecuteCommandMakeCatalogueItemExtractable))]
         [TestCase(typeof(ExecuteCommandMakeCatalogueProjectSpecific))]
         [TestCase(typeof(ExecuteCommandMakePatientIndexTableIntoRegularCohortIdentificationSetAgain))]
         [TestCase(typeof(ExecuteCommandMakeProjectSpecificCatalogueNormalAgain))]
         [TestCase(typeof(ExecuteCommandMergeCohortIdentificationConfigurations))]
         [TestCase(typeof(ExecuteCommandMoveAggregateIntoContainer))]
         [TestCase(typeof(ExecuteCommandMoveCohortAggregateContainerIntoSubContainer))]
         [TestCase(typeof(ExecuteCommandMoveContainerIntoContainer))]
         [TestCase(typeof(ExecuteCommandMoveFilterIntoContainer))]
         [TestCase(typeof(ExecuteCommandNewObject))]
        [TestCase(typeof(ExecuteCommandOverrideRawServer))]
         [TestCase(typeof(ExecuteCommandPrunePlugin))]
         [TestCase(typeof(ExecuteCommandPutCatalogueIntoCatalogueFolder))]
        [TestCase(typeof(ExecuteCommandQueryPlatformDatabase))]
         [TestCase(typeof(ExecuteCommandRefreshBrokenCohorts))]
         [TestCase(typeof(ExecuteCommandRename))]
         [TestCase(typeof(ExecuteCommandResetExtractionProgress))]
         [TestCase(typeof(ExecuteCommandRunSupportingSql))]
         [TestCase(typeof(ExecuteCommandScriptTable))]
         [TestCase(typeof(ExecuteCommandScriptTables))]
         [TestCase(typeof(ExecuteCommandSet))]
        [TestCase(typeof(ExecuteCommandSetAggregateDimension))]
         [TestCase(typeof(ExecuteCommandSetArgument))]
         [TestCase(typeof(ExecuteCommandSetAxis))]
         [TestCase(typeof(ExecuteCommandSetContainerOperation))]
         [TestCase(typeof(ExecuteCommandSetDataAccessContextForCredentials))]
         [TestCase(typeof(ExecuteCommandSetExtractionIdentifier))]
         [TestCase(typeof(ExecuteCommandSetFilterTreeShortcut))]
         [TestCase(typeof(ExecuteCommandSetGlobalDleIgnorePattern))]
         [TestCase(typeof(ExecuteCommandSetIgnoredColumns))]
         [TestCase(typeof(ExecuteCommandSetPermissionWindow))]
         [TestCase(typeof(ExecuteCommandSetPivot))]
         [TestCase(typeof(ExecuteCommandSetProjectExtractionDirectory))]
         [TestCase(typeof(ExecuteCommandSetQueryCachingDatabase))]
         [TestCase(typeof(ExecuteCommandSetUserSetting))]
         [TestCase(typeof(ExecuteCommandShow))]
         [TestCase(typeof(ExecuteCommandShowRelatedObject))]
         [TestCase(typeof(ExecuteCommandSimilar))]
         [TestCase(typeof(ExecuteCommandSyncTableInfo))]
         [TestCase(typeof(ExecuteCommandUnfreezeExtractionConfiguration))]
        [TestCase(typeof(ExecuteCommandUnMergeCohortIdentificationConfiguration))]
         [TestCase(typeof(ExecuteCommandUseCredentialsToAccessTableInfoData))]
         [TestCase(typeof(ExecuteCommandViewCatalogueData))]
         [TestCase(typeof(ExecuteCommandViewCohortIdentificationConfiguration))]
         [TestCase(typeof(ExecuteCommandViewCohortSample))]
         [TestCase(typeof(ExecuteCommandViewData))]
         [TestCase(typeof(ExecuteCommandViewExtractionSql))]
         [TestCase(typeof(ExecuteCommandViewFilterMatchData))]
         [TestCase(typeof(ExecuteCommandViewLogs))]
         [TestCase(typeof(ExecuteCommandViewSample))]
         [TestCase(typeof(ExecuteCommandExportInDublinCoreFormat))]
         [TestCase(typeof(ExecuteCommandImportCatalogueDescriptionsFromShare))]
         [TestCase(typeof(ExecuteCommandImportDublinCoreFormat))]
         [TestCase(typeof(ExecuteCommandImportFilterDescriptionsFromShare))]
         [TestCase(typeof(ExecuteCommandImportShareDefinitionList))]
        public void TestIsSupported(Type t)
        {
            var activator = GetActivator();
            var invoker = new CommandInvoker(activator);

            Assert.IsTrue(invoker.IsSupported(t), $"Type {t} was not supported by CommandInvoker");
        }
    }
}
