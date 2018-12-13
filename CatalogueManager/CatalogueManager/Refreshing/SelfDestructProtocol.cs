using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable.Revertable;

namespace CatalogueManager.Refreshing
{
    public class SelfDestructProtocol<T> : IRefreshBusSubscriber where T : DatabaseEntity
    {
        private readonly IActivateItems _activator;
        public RDMPSingleDatabaseObjectControl<T> User { get; private set; }
        public T OriginalObject { get; set; }

        public SelfDestructProtocol(RDMPSingleDatabaseObjectControl<T> user,IActivateItems activator, T originalObject)
        {
            _activator = activator;
            User = user;
            OriginalObject = originalObject;
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            var descendancy = e.DeletedObjectDescendancy ?? _activator.CoreChildProvider.GetDescendancyListIfAnyFor(e.Object);

           //implementation of the anoymous callback
            var o = e.Object as T;

            //if the descendancy contained our object Type we should also consider a refresh
            if (o == null && descendancy != null)
                o = (T)descendancy.Parents.LastOrDefault(p => p is T);

            //don't respond to events raised by the user themself!
            if(sender == User)
                return;

            //if the original object does not exist anymore (could be a CASCADE event so we do have to check it every time regardless of what object type is refreshing)
            if (!OriginalObject.Exists())//object no longer exists!
            {
                var parent = User.ParentForm;
                if (parent != null && !parent.IsDisposed)
                    parent.Close();//self destruct because object was deleted

                return;
            }
            
            if (o != null && o.ID == OriginalObject.ID && o.GetType() == OriginalObject.GetType())//object was refreshed, probably an update to some fields in it
            {
                //make sure the object is up-to-date with the database saved state.
                if (o.Exists())
                    o.RevertToDatabaseState();

                User.SetDatabaseObject(_activator, o); //give it the new object
            }
        }

    }
}