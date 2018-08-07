﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using CachingEngine.Factories;
using CachingEngine.Layouts;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CachingEngine.PipelineExecution.Destinations
{
    /// <summary>
    /// Time period for which cache chunks are stored / fetched.  Some caching tasks produce so many file system entries it is nessesary to subdivide the cache by Hour.
    /// </summary>
    public enum CacheFileGranularity
    {
        Hour,
        Day
    };

    /// <summary>
    /// Abstract implementation of ICacheFileSystemDestination. Includes checks for CacheLayout construction and read/write permissions to Cache directory.  To implement
    /// this class you should implement an ICacheLayout (or use an existing one) and then use ProcessPipelineData to populate the CacheDirectory with data according to the
    /// ICacheLayout
    /// </summary>
    public abstract class CacheFilesystemDestination : ICacheFileSystemDestination, IPluginDataFlowComponent<ICacheChunk>, IDataFlowDestination<ICacheChunk>
    {
        [DemandsInitialization("Root directory for the cache. This overrides the default HICProjectDirectory cache location. This might be needed if you are caching a very large data set which needs its own dedicated storage resource, for example.",DemandType.Unspecified,null)]
        public DirectoryInfo CacheDirectory { get; set; }
        
        public abstract ICacheChunk ProcessPipelineData(ICacheChunk toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken);
        
        public void PreInitialize(IHICProjectDirectory value, IDataLoadEventListener listener)
        {
            if (value.IsDesignTime)
                return;

            // CacheDirectory overrides HICProjectDirectory, so only set CacheDirectory if it is null (i.e. no alternative cache location has been configured in the destination component)
            if (CacheDirectory == null)
            {
                if (value.Cache == null)
                    throw new Exception("For some reason the HICProjectDirectory does not have a Cache specified and the FilesystemDestination component does not have an override CacheDirectory specified");

                CacheDirectory = value.Cache;
            }
        }

        /// <summary>
        /// Use CacheDirectory to create a new layout, this method should only be called after PreInitialize
        /// </summary>
        /// <returns></returns>
        public abstract ICacheLayout CreateCacheLayout();
        
        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public abstract void Abort(IDataLoadEventListener listener);

        public void Stop()
        {
        }

        public void Abort()
        {
        }

        public bool SilentRunning { get; set; }
        public virtual void Check(ICheckNotifier notifier)
        {
            if (CacheDirectory == null)
                throw new InvalidOperationException("CacheDirectory is null, ensure that pre-initialize has been called with a valid object before checking.");

            // If we have an overridden cache directory, ensure we can reach it and write to it
            if (CacheDirectory != null)
            {
                try
                {
                    var tempFilename = Path.Combine(CacheDirectory.FullName, ".test.txt");
                    var sw = File.CreateText(tempFilename);
                    sw.Close();
                    sw.Dispose();
                    File.Delete(tempFilename);
                    notifier.OnCheckPerformed(new CheckEventArgs("Confirmed could write to/delete from the overridden CacheDirectory: " + CacheDirectory.FullName, CheckResult.Success));
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not write to the overridden CacheDirectory: " + CacheDirectory.FullName, CheckResult.Fail, e));
                }
            }

            // Check CacheLayout creation
            var cacheLayout = CreateCacheLayout();
            if (cacheLayout == null)
                notifier.OnCheckPerformed(new CheckEventArgs("The CacheLayout object in CacheFilesystemDestination is not being constructed correctly", CheckResult.Fail));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("CacheLayout object in CacheFilesystemDestination is OK", CheckResult.Success));
        }
    }
}
