// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using MapsDirectlyToDatabaseTable.Injection;
using MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.CatalogueLibrary.Data.ImportExport;
using Rdmp.Core.CatalogueLibrary.Data.Serialization;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.Annotations;

namespace Rdmp.Core.CatalogueLibrary.Data
{
    /// <summary>
    /// A virtual column that is made available to researchers.  Each Catalogue has 1 or more CatalogueItems, these contain the descriptions of what is contained
    /// in the column as well as any outstanding/resolved issues with the column (see CatalogueItemIssue).
    /// 
    /// <para>It is important to note that CatalogueItems are not tied to underlying database tables/columns except via an ExtractionInformation.  This means that you can
    /// for example have multiple different versions of the same underlying ColumnInfo </para>
    /// 
    /// <para>e.g.
    /// CatalogueItem: PatientDateOfBirth (ExtractionInformation verbatim but 'Special Approval Required')
    /// CatalogueItem: PatientDateOfBirthApprox (ExtractionInformation rounds to nearest quarter but governance is 'Core')</para>
    /// 
    /// <para>Both the above would extract from the same ColumnInfo DateOfBirth</para>
    /// </summary>
    public class CatalogueItem : DatabaseEntity, IDeleteable, IComparable, IHasDependencies, IRevertable, INamed, IInjectKnown<ExtractionInformation>,IInjectKnown<ColumnInfo>, IInjectKnown<Catalogue>
    {
        #region Database Properties
        
        private string _Name;
        private string _Statistical_cons;
        private string _Research_relevance;
        private string _Description;
        private string _Topic;
        private string _Agg_method;
        private string _Limitations;
        private string _Comments;
        private int _catalogueID;
        private int? _columnInfoID;
        private Catalogue.CataloguePeriodicity _periodicity;

        private Lazy<ExtractionInformation> _knownExtractionInformation;
        private Lazy<ColumnInfo> _knownColumnInfo;
        private Lazy<Catalogue> _knownCatalogue;

        /// <summary>
        /// The ID of the parent <see cref="Catalogue"/> (dataset) to which this is a virtual column/column description
        /// </summary>
        [Relationship(typeof(Catalogue),RelationshipType.SharedObject)]
        [DoNotExtractProperty]
        public int Catalogue_ID
        {
            get { return _catalogueID; }
            set
            {
                SetField(ref _catalogueID, value);
                ClearAllInjections();
            }
        }
        /// <inheritdoc/>
        [NotNull]
        [DoNotImportDescriptions]
        public string Name {
            get { return _Name;}
            set {SetField(ref _Name,value);} 
        }

        /// <summary>
        /// User supplied field meant to identify any statistical anomalies with the data in the column described.  Not used for anything by RDMP.
        /// </summary>
        public string Statistical_cons {
            get { return _Statistical_cons; }
            set
            {
                SetField(ref _Statistical_cons, value);
            } 
        }

        /// <summary>
        /// User supplied field meant for describing research applicability/field of the data in the column.  Not used for anything by RDMP.
        /// </summary>
        public string Research_relevance
        {
            get { return _Research_relevance; }
            set { SetField(ref _Research_relevance , value);}
        }

        /// <summary>
        /// User supplied description of what is in the column
        /// </summary>
        public string Description
        {
            get { return _Description; }
            set { SetField(ref _Description , value);}
        }

        /// <summary>
        /// User supplied heading or keywords of what is in the column relates to.  Not used for anything by RDMP.
        /// </summary>
        public string Topic
        {
            get { return _Topic; }
            set { SetField(ref _Topic , value);}
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Agg_method
        {
            get { return _Agg_method; }
            set { SetField(ref _Agg_method , value);}
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Limitations
        {
            get { return _Limitations; }
            set{ SetField(ref _Limitations , value);}
        }

        /// <summary>
        /// User specified free text field.  Not used for anything by RDMP.
        /// </summary>
        public string Comments
        {
            get { return _Comments; }
            set { SetField( ref _Comments , value);}
        }

        /// <summary>
        /// The ID of the underlying <see cref="ColumnInfo"/> to which this CatalogueItem describes.  This can be null if the underlying column has been deleted / removed.
        /// You can have multiple <see cref="CatalogueItem"/>s in a <see cref="Catalogue"/> that share the same underlying <see cref="ColumnInfo"/> if one of them is a transform
        /// e.g. you might release the first 3 digits of a postcode to anyone (<see cref="ExtractionCategory.Core"/>) but only release the full postcode with 
        /// <see cref="ExtractionCategory.SpecialApprovalRequired"/>.
        /// </summary>
        [Relationship(typeof(ColumnInfo), RelationshipType.IgnoreableLocalReference)]  //will appear as empty, then the user can guess from a table
        public int? ColumnInfo_ID
        {
            get { return _columnInfoID; }
            set
            {
                //don't change it to the same value it already has
                if(value == ColumnInfo_ID )
                    return;

                SetField(ref _columnInfoID , value);
                ClearAllInjections();
            }
        }

        /// <summary>
        /// How frequently this column is updated... why this would be different from <see cref="CatalogueLibrary.Data.Catalogue.Periodicity"/>?
        /// </summary>
        public Catalogue.CataloguePeriodicity Periodicity
        {
            get { return _periodicity; }
            set { SetField(ref _periodicity , value); }
        }

        #endregion


        #region Relationships
        /// <inheritdoc cref="Catalogue_ID"/>
        [NoMappingToDatabase]
        public Catalogue Catalogue {
            get{return _knownCatalogue.Value;}
        }

        /// <summary>
        /// Fetches the <see cref="ExtractionInformation"/> (if any) that specifies how to extract this column.  This can be the underlying column name (fully specified) or a transform.
        /// <para>This will be null if the <see cref="CatalogueItem"/> is not extractable</para>
        /// </summary>
        [NoMappingToDatabase]
        public ExtractionInformation ExtractionInformation
        {
            get
            {
                return _knownExtractionInformation.Value;
            }
        }

        /// <inheritdoc cref="ColumnInfo_ID"/>
        [NoMappingToDatabase]
        public ColumnInfo ColumnInfo
        {
            get
            {
                return _knownColumnInfo.Value;
            }
        }
        
        internal bool IsColumnInfoCached()
        {
            return _knownColumnInfo.IsValueCreated;
        }

        #endregion

        /// <summary>
        /// Name of the parent <see cref="Catalogue"/>.  This property value will be cached once fetched for a given object thanks to Lazy/IInjectKnown&lt;Catalogue&gt;"/>.
        /// </summary>
        [UsefulProperty]
        [NoMappingToDatabase]
        [DoNotExtractProperty]
        public string CatalogueName { get { return Catalogue.Name; }}

        /// <summary>
        /// Creates a new virtual column description for for a column in the dataset (<paramref name="parent"/>) supplied with the given Name.
        /// <para><remarks>You should next choose which <see cref="ColumnInfo"/> powers it and optionally create an <see cref="ExtractionInformation"/> to
        /// make the column extractable</remarks></para>
        /// </summary>
        public CatalogueItem(ICatalogueRepository repository, Catalogue parent, string name)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", name},
                {"Catalogue_ID", parent.ID}
            });
            
            ClearAllInjections();
        }

        internal CatalogueItem(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Catalogue_ID = int.Parse(r["Catalogue_ID"].ToString()); //gets around decimals and other random crud number field types that sql returns
            Name = (string)r["Name"];
            Statistical_cons = r["Statistical_cons"].ToString();
            Research_relevance = r["Research_relevance"].ToString();
            Description = r["Description"].ToString();
            Topic = r["Topic"].ToString();
            Agg_method = r["Agg_method"].ToString();
            Limitations = r["Limitations"].ToString();
            Comments = r["Comments"].ToString();
            ColumnInfo_ID = ObjectToNullableInt(r["ColumnInfo_ID"]);

            //Periodicity - with handling for invalid enum values listed in database
            object periodicity = r["Periodicity"];
            if (periodicity == null || periodicity == DBNull.Value)
                Periodicity = Catalogue.CataloguePeriodicity.Unknown;
            else
            {
                Catalogue.CataloguePeriodicity periodicityAsEnum;

                if(Catalogue.CataloguePeriodicity.TryParse(periodicity.ToString(), true, out periodicityAsEnum))
                    Periodicity = periodicityAsEnum;
                else
                     Periodicity = Catalogue.CataloguePeriodicity.Unknown;
            }

            ClearAllInjections();
        }

        internal CatalogueItem(ShareManager shareManager, ShareDefinition shareDefinition)
        {
            shareManager.UpsertAndHydrate(this,shareDefinition);
        }

        /// <inheritdoc/>
        public void InjectKnown(Catalogue instance)
        {
            _knownCatalogue = new Lazy<Catalogue>(()=>instance);
        }

        /// <inheritdoc/>
        public void ClearAllInjections()
        {
            _knownColumnInfo = new Lazy<ColumnInfo>(FetchColumnInfoIfAny);
            _knownExtractionInformation = new Lazy<ExtractionInformation>(FetchExtractionInformationIfAny);
            _knownCatalogue = new Lazy<Catalogue>(FetchCatalogue); 
        }

        private Catalogue FetchCatalogue()
        {
            return Repository.GetObjectByID<Catalogue>(Catalogue_ID);
        }

        private ExtractionInformation FetchExtractionInformationIfAny()
        {
            return Repository.GetAllObjectsWithParent<ExtractionInformation>(this).SingleOrDefault();
        }

        private ColumnInfo FetchColumnInfoIfAny()
        {
            if (!ColumnInfo_ID.HasValue)
                return null;

            return Repository.GetObjectByID<ColumnInfo>(ColumnInfo_ID.Value);
        }

        /// <inheritdoc/>
        public void InjectKnown(ExtractionInformation instance)
        {
            _knownExtractionInformation = new Lazy<ExtractionInformation>(()=>instance);
        }
        /// <inheritdoc/>
        public void InjectKnown(ColumnInfo instance)
        {
            _knownColumnInfo = new Lazy<ColumnInfo>(()=>instance);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Sorts alphabetically by <see cref="Name"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj is CatalogueItem)
            {
                return -(obj.ToString().CompareTo(this.ToString())); //sort alphabetically (reverse)
            }

            throw new Exception("Cannot compare " + this.GetType().Name + " to " + obj.GetType().Name);
        }
        
        /// <summary>
        /// Copies the descriptive metadata from one <see cref="CatalogueItem"/> (this) into a new <see cref="CatalogueItem"/> in the supplied <paramref name="cataToImportTo"/>
        /// </summary>
        /// <param name="cataToImportTo">The <see cref="Catalogue"/> to import into (cannot be the current <see cref="CatalogueItem"/> parent)</param>
        /// <returns></returns>
        public CatalogueItem CloneCatalogueItemWithIDIntoCatalogue(Catalogue cataToImportTo)
        {
            if(this.Catalogue_ID == cataToImportTo.ID)
                throw new ArgumentException("Cannot clone a CatalogueItem into it's own parent, specify a different catalogue to clone into");

            var clone = new CatalogueItem((ICatalogueRepository)cataToImportTo.Repository, cataToImportTo, this.Name);
            
            //Get all the properties           
            PropertyInfo[] propertyInfo = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite && !property.Name.Equals("ID") && !property.Name.Equals("Catalogue_ID"))
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                        property.SetValue(clone, property.GetValue(this, null), null);
            }

            clone.SaveToDatabase();
            
            return clone;
        }

        /// <summary>
        /// Guesses which <see cref="ColumnInfo"/> from a collection is probably the right one for underlying this <see cref="CatalogueItem"/>.  This is done
        /// by looking for a <see cref="ColumnInfo"/> whose name is the same as the <see cref="CatalogueItem.Name"/> if not then it gets more flexible looking
        /// for .Contains etc.
        /// </summary>
        /// <param name="guessPool"></param>
        /// <param name="allowPartial">Set to false to avoid last-resort match based on String.Contains</param>
        /// <returns></returns>
        public IEnumerable<ColumnInfo> GuessAssociatedColumn(ColumnInfo[] guessPool, bool allowPartial = true)
        {
            //exact matches exist so return those
            ColumnInfo[] Guess = guessPool.Where(col => col.GetRuntimeName().Equals(this.Name)).ToArray();
            if (Guess.Any())
                return Guess;

            //ignore caps match instead
            Guess = guessPool.Where(col => col.GetRuntimeName().ToLower().Equals(this.Name.ToLower())).ToArray();
            if (Guess.Any())
                return Guess;

            //ignore caps and remove spaces match instead
            Guess = guessPool.Where(col => col.GetRuntimeName().ToLower().Replace(" ", "").Equals(this.Name.ToLower().Replace(" ", ""))).ToArray();
            if (Guess.Any())
                return Guess;

            if (allowPartial)
                //contains match is final last resort
                return guessPool.Where(col =>
                    col.GetRuntimeName().ToLower().Contains(this.Name.ToLower())
                    ||
                    Name.ToLower().Contains(col.GetRuntimeName().ToLower()));

            return new ColumnInfo[0];
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return null;
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            List<IHasDependencies> dependantObjects = new List<IHasDependencies>();

            var exInfo = ExtractionInformation;

            if(exInfo != null)
                dependantObjects.Add(exInfo);

            if(ColumnInfo_ID != null)
                dependantObjects.Add(ColumnInfo);

            dependantObjects.Add(Catalogue);
            return dependantObjects.ToArray();
        }

        /// <summary>
        /// Changes the CatalogueItem in the database to be based off of the specified ColumnInfo (or none if null is specified).  This will
        /// likely result in the ExtractionInformation being corrupt / out of sync in terms of the SQL appearing in it's
        /// <see cref="IColumn.SelectSQL"/>.
        /// </summary>
        /// <param name="columnInfo"></param>
        public void SetColumnInfo(ColumnInfo columnInfo)
        {
            ColumnInfo_ID = columnInfo == null ? (int?) null : columnInfo.ID;
            SaveToDatabase();
            InjectKnown(columnInfo);
        }

        public CatalogueItem ShallowClone(Catalogue into)
        {
            var clone = new CatalogueItem(CatalogueRepository, into, Name);
            CopyShallowValuesTo(clone);
            return clone;
        }
    }
}
