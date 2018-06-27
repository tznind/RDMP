﻿using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.AutoComplete;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ObjectVisualisation;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace DataExportManager.DataViewing.Collections
{
    internal class ViewCohortExtractionUICollection : IViewSQLAndResultsCollection
    {
        public PersistStringHelper Helper { get; private set; }
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }

        public ViewCohortExtractionUICollection()
        {
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
            Helper = new PersistStringHelper();
        }

        public ViewCohortExtractionUICollection(ExtractableCohort cohort):this()
        {
            DatabaseObjects.Add(cohort);
        }

        public string SaveExtraText()
        {
            return null;
        }

        public void LoadExtraText(string s)
        {
            
        }
        
        public ExtractableCohort Cohort { get { return DatabaseObjects.OfType<ExtractableCohort>().SingleOrDefault(); } }

        public void SetupRibbon(RDMPObjectsRibbonUI ribbon)
        {
            ribbon.Add(Cohort);
        }

        public IDataAccessPoint GetDataAccessPoint()
        {
            return Cohort.ExternalCohortTable;
        }

        public string GetSql()
        {
            if (Cohort == null)
                return "";

            var tableName = Cohort.ExternalCohortTable.TableName;

            var response = GetQuerySyntaxHelper().HowDoWeAchieveTopX(100);

            switch (response.Location)
            {
                case QueryComponent.SELECT:
                    return "Select " + response.SQL + " * from " + tableName + " WHERE " + Cohort.WhereSQL();
                case QueryComponent.WHERE:
                    return "Select * from " + tableName + " WHERE " + response.SQL + " AND " + Cohort.WhereSQL();
                case QueryComponent.Postfix:
                    return "Select * from " + tableName + " " + response.SQL + " WHERE " + Cohort.WhereSQL();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetTabName()
        {
            return "Top 100 " + Cohort;
        }

        public void AdjustAutocomplete(AutoCompleteProvider autoComplete)
        {
            if(Cohort == null)
                return;

            var ect = Cohort.ExternalCohortTable;
            var table = ect.Discover().ExpectTable(ect.TableName);
            autoComplete.Add(table);
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            var c = Cohort;
            return c != null ? c.GetQuerySyntaxHelper() : null;
        }
    }
}