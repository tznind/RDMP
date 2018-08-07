﻿using System;
using System.Data;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CohortManagerLibrary;
using CohortManagerLibrary.Execution;
using QueryCaching.Aggregation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline.Sources
{
    /// <summary>
    /// Executes a Cohort Identification Configuration query and releases the identifiers read into the pipeline as a single column DataTable.
    /// </summary>
    public class CohortIdentificationConfigurationSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<CohortIdentificationConfiguration>
    {
        private CohortIdentificationConfiguration _cohortIdentificationConfiguration;

        [DemandsInitialization("The length of time (in seconds) to wait before timing out the SQL command to execute the CohortIdentificationConfiguration, if you find it is taking exceptionally long for a CohortIdentificationConfiguration to execute then consider caching some of the subqueries",DemandType.Unspecified,10000)]
        public int Timeout { get; set; }

        [DemandsInitialization("If ticked, will Freeze the CohortIdentificationConfiguration if the import pipeline terminates successfully")]
        public bool FreezeAfterSuccessfulImport { get; set; }

        private bool haveSentData = false;
        private CancellationTokenSource _cancelGlobalOperations = new CancellationTokenSource();

        /// <summary>
        /// If you are refreshing a cohort or running a cic which was run and cached a long time ago you might want to clear out the cache.  This will mean that
        /// when run you will get a view of the live tables (which might be recached as part of building the cic) rather than the (potentially stale) current cache
        /// </summary>
        public bool ClearCohortIdentificationConfigurationCacheBeforeRunning { get; set; }

        public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {

            if(haveSentData)
                return null;

            haveSentData = true;

            return GetDataTable(listener);
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            //if it didn't crash
            if(pipelineFailureExceptionIfAny == null)
                if(FreezeAfterSuccessfulImport)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Freezing CohortIdentificationConfiguration"));
                    _cohortIdentificationConfiguration.Freeze();
                }
        }


        public void Abort(IDataLoadEventListener listener)
        {
            _cancelGlobalOperations.Cancel();
        }

        public DataTable TryGetPreview()
        {
            if (_cohortIdentificationConfiguration.IsDesignTime)
                return null;

            return GetDataTable(new ThrowImmediatelyDataLoadEventListener());
        }

        private DataTable GetDataTable(IDataLoadEventListener listener)
        {
            if(listener != null)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to lookup which server to interrogate for CohortIdentificationConfiguration " + _cohortIdentificationConfiguration));

            if(_cohortIdentificationConfiguration.RootCohortAggregateContainer_ID == null)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "CohortIdentificationConfiguration '" + _cohortIdentificationConfiguration + "' has no RootCohortAggregateContainer_ID, is it empty?"));

            var cohortCompiler = new CohortCompiler(_cohortIdentificationConfiguration);

            ICompileable rootContainerTask;
            //no caching set up so no point in running CohortCompilerRunner 
            if(_cohortIdentificationConfiguration.QueryCachingServer_ID == null)
                rootContainerTask = RunRootContainerOnlyNoCaching(cohortCompiler);
            else
                rootContainerTask =  RunAllTasksWithRunner(cohortCompiler,listener);

            if (rootContainerTask.State != CompilationState.Finished)
                throw new Exception("CohortIdentificationCriteria execution resulted in state '" + rootContainerTask.State + "'", rootContainerTask.CrashMessage);

            if(rootContainerTask == null)
                throw new Exception("Root container task was null, was the execution cancelled? / crashed");

            var execution = cohortCompiler.Tasks[rootContainerTask];

            if (execution.Identifiers == null || execution.Identifiers.Rows.Count == 0)
                throw new Exception("CohortIdentificationCriteria execution resulted in an empty dataset (there were no cohorts matched by the query?)");

            var dt = execution.Identifiers;

            foreach (DataColumn column in dt.Columns)
                column.ReadOnly = false;

            return dt;
        }


        private ICompileable RunRootContainerOnlyNoCaching(CohortCompiler cohortCompiler)
        {
            //add root container task
            var task = cohortCompiler.AddTask(_cohortIdentificationConfiguration.RootCohortAggregateContainer, _cohortIdentificationConfiguration.GetAllParameters());

            cohortCompiler.LaunchSingleTask(task, Timeout);

            //timeout is in seconds
            int countDown = Timeout * 1000;

            while (
                //hasn't timed out
                countDown > 0 &&
                (
                //state isn't a final state
                    task.State == CompilationState.Executing || task.State == CompilationState.NotScheduled || task.State == CompilationState.Scheduled)
                )
            {
                Thread.Sleep(100);
                countDown -= 100;
            }


            if (countDown <= 0)
                try
                {
                    throw new Exception("Cohort failed to reach a final state (Finished/Crashed) after " + Timeout + " seconds. Current state is " + task.State + ".  The task will be cancelled");
                }
                finally
                {
                    cohortCompiler.CancelAllTasks(true);
                }

            return task;
        }

        private ICompileable RunAllTasksWithRunner(CohortCompiler cohortCompiler, IDataLoadEventListener listener)
        {
            if (ClearCohortIdentificationConfigurationCacheBeforeRunning)
            {
                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Clearing Cohort Identifier Cache"));

                var cacheManager = new CachedAggregateConfigurationResultsManager(_cohortIdentificationConfiguration.QueryCachingServer);
                
                cohortCompiler.AddAllTasks(false);
                foreach (var cacheable in cohortCompiler.Tasks.Keys.OfType<ICacheableTask>())
                    cacheable.ClearYourselfFromCache(cacheManager);
            }

            var runner = new CohortCompilerRunner(cohortCompiler, Timeout);
            runner.RunSubcontainers = false;
            runner.PhaseChanged += (s,e)=> listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "CohortCompilerRunner entered Phase '" + runner.ExecutionPhase + "'"));
            return runner.Run(_cancelGlobalOperations.Token);
        }

        public void Check(ICheckNotifier notifier)
        {
            if (_cohortIdentificationConfiguration.IsDesignTime)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Checks not run because no CohortIdentificationConfiguration has been selected (IsDesignTime = true)",CheckResult.Warning));
                return;
            }

            try
            {
                if (_cohortIdentificationConfiguration.Frozen)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "CohortIdentificationConfiguration " + _cohortIdentificationConfiguration +
                            " is Frozen (By " + _cohortIdentificationConfiguration.FrozenBy + " on " +
                            _cohortIdentificationConfiguration.FrozenDate + ").  It might have already been imported once before.", CheckResult.Warning));

                
                var result = TryGetPreview();
                
                if(result.Rows.Count == 0)
                    throw new Exception("No Identifiers were returned by the cohort query");
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not build extraction SQL for " + _cohortIdentificationConfiguration, CheckResult.Fail,e));
            }
            
        }


        public void PreInitialize(CohortIdentificationConfiguration value, IDataLoadEventListener listener)
        {
            _cohortIdentificationConfiguration = value;
        }
    }
}
