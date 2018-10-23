using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Attachers
{
    /// <summary>
    /// See Attacher
    /// </summary>
    public interface IAttacher: IDisposeAfterDataLoad, ICheckable
    {
        ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken);
        void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo);
        
        IHICProjectDirectory HICProjectDirectory { get; set; }
        bool RequestsExternalDatabaseCreation { get; }
    }
}