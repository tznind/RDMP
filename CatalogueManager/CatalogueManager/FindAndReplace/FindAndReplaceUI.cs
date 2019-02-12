// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Providers;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using Sharing.Dependency.Gathering;

namespace CatalogueManager.FindAndReplace
{
    /// <summary>
    /// Allows you to perform database wide find and replace operations.  This is a useful but very dangerous feature, it is possible to very easily break your Catalogue.  The 
    /// feature is primarily intended for system wide operations such as when you change a network UNC folder location or mapped network drive and you need to change ALL references
    /// to the root path to the new location.
    /// 
    /// <para>Sql properties are also exposed but this is even more dangerous to modify.  For example if you change a database name and want to perform a system wide rename on all
    /// references including filters, extractable columns, extracted project definitions etc etc</para>
    /// 
    /// <para>You should always back up both your Catalogue and DataExport databases before embarking on a Find and Replace</para>
    /// </summary>
    public partial class FindAndReplaceUI : UserControl
    {
        private readonly IActivateItems _activator;
        private HashSet<IMapsDirectlyToDatabaseTable> _allObjects = new HashSet<IMapsDirectlyToDatabaseTable>();

        private IAttributePropertyFinder _adjustableLocationPropertyFinder;
        private List<FindAndReplaceNode> _locationNodes = new List<FindAndReplaceNode>(); 
        
        private IAttributePropertyFinder _sqlPropertyFinder;
        private List<FindAndReplaceNode> _sqlNodes = new List<FindAndReplaceNode>();

        
        public FindAndReplaceUI(IActivateItems activator)
        {
            _activator = activator;

            GetAllObjects(activator);

            _adjustableLocationPropertyFinder = new AttributePropertyFinder<AdjustableLocationAttribute>(_allObjects);
            _sqlPropertyFinder = new AttributePropertyFinder<SqlAttribute>(_allObjects);

            InitializeComponent();

            olvObject.ImageGetter += ImageGetter;
            olvProperty.AspectGetter += PropertyAspectGetter;
            olvValue.AspectGetter += ValueAspectGetter;
            
            olvAllObjects.AlwaysGroupByColumn = olvProperty;

            //allow editing
            olvAllObjects.CellEditFinished += OlvAllObjectsCellEditFinished;

            //Create all the nodes up front
            foreach (IMapsDirectlyToDatabaseTable o in _allObjects.Where(_adjustableLocationPropertyFinder.ObjectContainsProperty))
                foreach (PropertyInfo propertyInfo in _adjustableLocationPropertyFinder.GetProperties(o))
                    _locationNodes.Add( new FindAndReplaceNode(o,propertyInfo));

            foreach (IMapsDirectlyToDatabaseTable o in _allObjects.Where(_sqlPropertyFinder.ObjectContainsProperty))
                foreach (PropertyInfo propertyInfo in _sqlPropertyFinder.GetProperties(o))
                    _sqlNodes.Add(new FindAndReplaceNode(o, propertyInfo));

            olvAllObjects.AddObjects(_locationNodes);
        }

        private void GetAllObjects(IActivateItems activator)
        {

            Gatherer g = new Gatherer(activator.RepositoryLocator);

            //We get these from the child provider because some objects (those below go off looking stuff up if you get them
            //and do not inject known good values first)
            foreach (var o in _activator.CoreChildProvider.AllExtractionInformations)
                _allObjects.Add(o);

            foreach (var o in _activator.CoreChildProvider.AllCatalogueItems)
                _allObjects.Add(o);

            var dxmChildProvider = _activator.CoreChildProvider as DataExportChildProvider;

            if (dxmChildProvider != null)
                foreach (var o in dxmChildProvider.GetAllExtractableColumns(_activator.RepositoryLocator.DataExportRepository))
                    _allObjects.Add(o);

            foreach (var o in g.GetAllObjectsInAllDatabases())
                _allObjects.Add(o);
        }

        void OlvAllObjectsCellEditFinished(object sender, CellEditEventArgs e)
        {
            if( e == null || e.RowObject == null)
                return;
            
            var node = (FindAndReplaceNode)e.RowObject;
            node.SetValue(e.NewValue);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs((DatabaseEntity) node.Instance));
        }

        private object ValueAspectGetter(object rowobject)
        {
            return ((FindAndReplaceNode)rowobject).GetCurrentValue();
        }

        private object PropertyAspectGetter(object rowobject)
        {
            var node = ((FindAndReplaceNode) rowobject);

            return node.PropertyName;
        }

        private object ImageGetter(object rowObject)
        {
            return _activator.CoreIconProvider.GetImage(((FindAndReplaceNode) rowObject).Instance);
        }


        private void CheckedChanged(object sender, EventArgs e)
        {
            var cb = (RadioButton)sender;

            if(cb.Checked)
            {
                olvAllObjects.ClearObjects();
                olvAllObjects.SuspendLayout();

                if (sender == rbLocationsAttribute)
                    olvAllObjects.AddObjects(_locationNodes);
                else
                    olvAllObjects.AddObjects(_sqlNodes);

                olvAllObjects.ResumeLayout();
            }
        }
        
        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show(
                    "Are you sure you want to do a system wide find and replace? This operation cannot be undone",
                    "Are you sure", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (FindAndReplaceNode node in olvAllObjects.FilteredObjects)
                {
                    node.FindAndReplace(tbFind.Text, tbReplace.Text,!cbMatchCase.Checked);

                }
            }
        }

        private void tlvAllObjects_ItemActivate(object sender, EventArgs e)
        {
            var node = olvAllObjects.SelectedObject as FindAndReplaceNode;

            if (node != null)
            {
                var cmd = new ExecuteCommandActivate(_activator,node.Instance);
                if(!cmd.IsImpossible)
                    cmd.Execute();
            }
        }

        private TextMatchFilter _textMatchFilter;
        private void btnFind_Click(object sender, EventArgs e)
        {
            var all = olvAllObjects.ModelFilter as CompositeAllFilter;

            if(all == null)
                olvAllObjects.ModelFilter = all = new CompositeAllFilter(new List<IModelFilter>());
            
            if (_textMatchFilter != null && all.Filters.Contains(_textMatchFilter))
                all.Filters.Remove(_textMatchFilter);

            _textMatchFilter = new TextMatchFilter(olvAllObjects, tbFind.Text, cbMatchCase.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);

            all.Filters.Add(_textMatchFilter);

            olvAllObjects.ModelFilter = all;
        }
    }
}
