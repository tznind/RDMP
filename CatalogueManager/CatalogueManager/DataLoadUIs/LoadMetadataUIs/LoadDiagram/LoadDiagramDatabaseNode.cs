// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;
using CatalogueManager.Icons.IconProvision;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using FAnsi.Discovery;
using ReusableLibraryCode;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram
{
    public class LoadDiagramDatabaseNode : IHasLoadDiagramState, IKnowWhatIAm
    {
        private readonly LoadBubble _bubble;
        public readonly DiscoveredDatabase Database;
        private readonly TableInfo[] _loadTables;
        private readonly HICDatabaseConfiguration _config;
        
        public LoadDiagramState State { get; set; }

        public string DatabaseName { get; private set; }

        public List<LoadDiagramTableNode> _anticipatedChildren = new List<LoadDiagramTableNode>();
        public List<UnplannedTable> _unplannedChildren = new List<UnplannedTable>();


        public LoadDiagramDatabaseNode(LoadBubble bubble, DiscoveredDatabase database, TableInfo[] loadTables, HICDatabaseConfiguration config)
        {
            _bubble = bubble;
            Database = database;
            _loadTables = loadTables;
            _config = config;

            DatabaseName = Database.GetRuntimeName();

            _anticipatedChildren.AddRange(_loadTables.Select(t => new LoadDiagramTableNode(this, t, _bubble, _config)));
        }
        
        public IEnumerable<object> GetChildren()
        {
            return _anticipatedChildren.Cast<object>().Union(_unplannedChildren);
        }

        public override string ToString()
        {
            return DatabaseName;
        }

        public Bitmap GetImage(ICoreIconProvider coreIconProvider)
        {
            return coreIconProvider.GetImage(_bubble);
        }

        public void DiscoverState()
        {
            _unplannedChildren.Clear();

            if (!Database.Exists())
            {
                State = LoadDiagramState.NotFound;
                foreach (var plannedChild in _anticipatedChildren)
                    plannedChild.SetStateNotFound();

                return;
            }

            //database does exist 
            State = LoadDiagramState.Found;

            //so check the children (tables) for state
            foreach (var plannedChild in _anticipatedChildren)
                plannedChild.DiscoverState();

            //also discover any unplanned tables if not live
            if(_bubble != LoadBubble.Live)
                foreach (DiscoveredTable discoveredTable in Database.DiscoverTables(true))
                {
                    //it's an anticipated one
                    if(_anticipatedChildren.Any(c=>c.TableName.Equals(discoveredTable.GetRuntimeName(),StringComparison.CurrentCultureIgnoreCase)))
                        continue;

                    //it's unplanned (maybe user created it as part of his load script or something)
                    _unplannedChildren.Add(new UnplannedTable(discoveredTable));
                }
        }
        
        #region equality
        protected bool Equals(LoadDiagramDatabaseNode other)
        {
            return string.Equals(DatabaseName, other.DatabaseName) && _bubble == other._bubble;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoadDiagramDatabaseNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DatabaseName != null ? DatabaseName.GetHashCode() : 0)*397) ^ (int) _bubble;
            }
        }

        public string WhatIsThis()
        {
            switch (_bubble)
            {
                case LoadBubble.Raw:
                    return "Depicts what database will be used for the RAW database and the tables/columns that are anticipated/found in that server currently";
                case LoadBubble.Staging:
                    return "Depicts what database will be used for the STAGING database and the tables/columns that are anticipated/found in that server currently";
                case LoadBubble.Live:
                    return "Depicts the current live database(s) that the load will target (based on which Catalogues are associated with the load)";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}