using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Allows you to limit the number of rows returned by an aggregate graph built by AggregateBuilder (or the number of PIVOT lines in a graph).  If your AggregateConfiguration has no pivot
    /// (and no axis) then the SELECT query that is generated will have a 'TOP X' and it's 'ORDER BY' will be decided by this class.  The most common use of this is to limit the results according
    /// to the count column e.g. 'Only graph the top 10 most prescribed drugs'.  You can change the direction of the TopX to turn it into 'Only graph the 10 LEAST prescribed drugs' or you can
    /// change your count(*) SQL on the Aggregate to AVERAGE(dose) and then you would have 'Top 10 most prescribed drugs by average prescription amount... or something like that anyway'.
    /// 
    /// Also if you have a very strange requirement you can pick an AggregateDimension for the TopX to apply to other than the count column e.g. top 10 drug names by ascending would give
    ///  Asprin, Aardvarksprin, A... etc
    /// </summary>
    public class AggregateTopX : DatabaseEntity, IAggregateTopX
    {
        private int _topX;
        private int? _orderByDimensionIfAny_ID;
        private AggregateTopXOrderByDirection _orderByDirection;
        private int _aggregateConfigurationID;

        #region Database Properties

        /// <summary>
        /// ID of the aggregate graph that this topX applies to
        /// </summary>
        public int AggregateConfiguration_ID
        {
            get { return _aggregateConfigurationID; }
            set { SetField(ref _aggregateConfigurationID , value); }
        }

        /// <summary>
        /// The number of records to return from the TopX e.g. Top 10
        /// </summary>
        public int TopX
        {
            get { return _topX; }
            set { SetField(ref _topX, value); }
        }

        /// <summary>
        /// The dimension which the top X applies to, if null it will be the count / sum etc column (The AggregateCountColumn)
        /// </summary>
        public int? OrderByDimensionIfAny_ID
        {
            get { return _orderByDimensionIfAny_ID; }
            set { SetField(ref _orderByDimensionIfAny_ID, value); }
        }

        /// <summary>
        /// When applying a TopX to an aggregate, this is the direction (Ascending/Descending) for the ORDER BY statement.  Descending means pick top X where
        /// count / sum etc is highest.
        /// </summary>
        public AggregateTopXOrderByDirection OrderByDirection
        {
            get { return _orderByDirection; }
            set { SetField(ref _orderByDirection, value); }
        }
        #endregion

        /// <inheritdoc cref="OrderByDimensionIfAny_ID"/>
        #region Relationships
        /// <inheritdoc cref="OrderByDimensionIfAny_ID"/>
        [NoMappingToDatabase]
        public AggregateDimension OrderByDimensionIfAny
        {
            get
            {
                if (OrderByDimensionIfAny_ID == null)
                    return null;

                return Repository.GetObjectByID<AggregateDimension>(OrderByDimensionIfAny_ID.Value);
            }
        }

        #endregion

        /// <inheritdoc cref="OrderByDimensionIfAny_ID"/>
        [NoMappingToDatabase]
        public IColumn OrderByColumn { get { return OrderByDimensionIfAny; } }

        /// <summary>
        /// Creates an instance by reading it out of the database for the provided reader
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="r"></param>
        internal AggregateTopX(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            AggregateConfiguration_ID = (int)r["AggregateConfiguration_ID"];
            TopX = (int)r["TopX"];
            OrderByDimensionIfAny_ID = ObjectToNullableInt(r["OrderByDimensionIfAny_ID"]);
            OrderByDirection = (AggregateTopXOrderByDirection) Enum.Parse(typeof (AggregateTopXOrderByDirection), r["OrderByDirection"].ToString());
        }

        /// <summary>
        /// Defines that the given AggregateConfiguration should only return the top X records / pivot categories.  You can only have a single AggregateTopX declared
        /// for a given AggregateConfiguration (enforced with database constraints).
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="forConfiguration"></param>
        /// <param name="topX"></param>
        public AggregateTopX(ICatalogueRepository repository, AggregateConfiguration forConfiguration, int topX)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"AggregateConfiguration_ID", forConfiguration.ID},
                {"TopX", topX},
            });
        }

        
    }
}
