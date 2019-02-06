using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.Refreshing;
using CatalogueManager.Theme;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    /// <summary>
    /// TECHNICAL: base abstract class for all Controls which are concerned with a single root DatabaseEntity e.g. AggregateGraph is concerned only with an AggregateConfiguration
    /// and it's children.  The reason this class exists is to streamline lifetime publish subscriptions (ensuring multiple tabs editting one anothers database objects happens 
    /// in a seamless a way as possible). 
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [TechnicalUI]
    public abstract class RDMPSingleDatabaseObjectControl<T> : RDMPUserControl, IRDMPSingleDatabaseObjectControl where T : DatabaseEntity
    {
        private Control _colorIndicator;
        protected IActivateItems _activator;

        private BinderWithErrorProviderFactory _binder;

        protected ObjectSaverButton ObjectSaverButton1 = new ObjectSaverButton();

        public DatabaseEntity DatabaseObject { get; private set; }
        protected RDMPCollection AssociatedCollection = RDMPCollection.None;

        public virtual void SetDatabaseObject(IActivateItems activator, T databaseObject)
        {
            _activator = activator;
            _activator.RefreshBus.EstablishSelfDestructProtocol(this,activator,databaseObject);
            DatabaseObject = databaseObject;

            ClearToolStrip();

            if(_colorIndicator == null && AssociatedCollection != RDMPCollection.None)
            {
                var colorProvider = new BackColorProvider();
                _colorIndicator = new Control();
                _colorIndicator.Dock = DockStyle.Top;
                _colorIndicator.Location = new Point(0, 0);
                _colorIndicator.Size = new Size(150, BackColorProvider.IndiciatorBarSuggestedHeight);
                _colorIndicator.TabIndex = 0;
                _colorIndicator.BackColor = colorProvider.GetColor(AssociatedCollection);
                this.Controls.Add(this._colorIndicator);
            }

            if (_binder == null)
            {
                _binder = new BinderWithErrorProviderFactory(activator);
                SetBindings(_binder,databaseObject);
            }

            SetItemActivator(activator);

            if(this is ISaveableUI)
                ObjectSaverButton1.SetupFor(this, databaseObject, activator.RefreshBus);

        }

        protected virtual void SetBindings(BinderWithErrorProviderFactory rules, T databaseObject)
        {
            
        }

        HashSet<ComboBox> boundComboBoxes = new HashSet<ComboBox>();

        /// <summary>
        /// Performs data binding using default parameters (OnPropertyChanged), no formatting etc.  Getter must be a
        /// property of <see cref="DatabaseObject"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <param name="propertyName"></param>
        /// <param name="dataMember"></param>
        /// <param name="getter"></param>
        protected void Bind(Control c, string propertyName, string dataMember, Func<T, object> getter, bool formattingEnabled = true,DataSourceUpdateMode updateMode = DataSourceUpdateMode.OnPropertyChanged)
        {
            var box = c as ComboBox;

            //workaround for only comitting lists on loose focus
            if (box != null && box.DropDownStyle == ComboBoxStyle.DropDownList && propertyName.Equals("SelectedItem"))
            {
                boundComboBoxes.Add(box);
                box.SelectionChangeCommitted += (s,e)=>box.DataBindings["SelectedItem"].WriteValue();
            }
            
            _binder.Bind(c, propertyName, (T)DatabaseObject, dataMember, formattingEnabled, updateMode, getter);
        }


        /// <summary>
        /// Parses the datetime out of the <paramref name="tb"/> with blank being null.  If the string doesn't parse
        /// then the text will turn red.
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="p"></param>
        protected void SetDate(TextBox tb, Action<DateTime?> action)
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    action(null);
                    return;
                }

                DateTime dateTime = DateTime.Parse(tb.Text);
                action(dateTime);

                tb.ForeColor = Color.Black;

            }
            catch (Exception)
            {
                tb.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Parses the Uri out of the <paramref name="tb"/> with blank being null.  If the string doesn't parse
        /// then the text will turn red.
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="p"></param>
        protected void SetUrl(TextBox tb, Action<Uri> action)
        {
            try
            {
                Uri u = new Uri(tb.Text);
                action(u);
                tb.ForeColor = Color.Black;

            }
            catch (UriFormatException)
            {
                tb.ForeColor = Color.Red;
            }

        }

        protected override void InitializeToolStrip()
        {
            base.InitializeToolStrip();

            if (_colorIndicator != null)
                _colorIndicator.SendToBack();
        }

        public void SetDatabaseObject(IActivateItems activator, DatabaseEntity databaseObject)
        {
            SetDatabaseObject(activator,(T)databaseObject);
        }

        public Type GetTypeOfT()
        {
            return typeof (T);
        }

        public virtual string GetTabName()
        {
            var named = DatabaseObject as INamed;

            if (named != null)
                return named.Name;


            if (DatabaseObject != null)
                return DatabaseObject.ToString();

            return "Unamed Tab";
        }
        
        public void Publish(DatabaseEntity e)
        {
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(e));
        }

        public virtual void ConsultAboutClosing(object sender, FormClosingEventArgs e) {}

        /// <summary>
        /// Adds the given <paramref name="cmd"/> to the menu bar at the top of the control
        /// </summary>
        /// <param name="cmd"></param>
        protected void Add(IAtomicCommand cmd, string overrideCommandName, RDMPConcept overrideImage,OverlayKind overlayKind = OverlayKind.None)
        {
            Add(cmd,overrideCommandName,_activator.CoreIconProvider.GetImage(overrideImage,overlayKind));
        }


        /// <summary>
        /// Adds the all <see cref="IAtomicCommand"/> specified by <see cref="IActivateItems.PluginUserInterfaces"/> for the current control.  Commands
        /// will appear in the menu bar at the top of the control
        /// </summary>
        protected void AddPluginCommands()
        {
            foreach (IAtomicCommand cmd in GetPluginCommands())
                Add(cmd);
        }
        /// <summary>
        /// Adds the all <see cref="IAtomicCommand"/> specified by <see cref="IActivateItems.PluginUserInterfaces"/> for the current control.  Commands
        /// will appear in the menu drop down options at the top of the control
        /// </summary>
        protected void AddPluginCommandsToMenu()
        {
            foreach (IAtomicCommand cmd in GetPluginCommands())
                AddToMenu(cmd);
        }

        protected IEnumerable<IAtomicCommand> GetPluginCommands()
        {
            foreach (var p in _activator.PluginUserInterfaces)
            {
                var cmds = p.GetAdditionalCommandsForControl(this, DatabaseObject);

                if (cmds == null)
                    continue;

                foreach (var c in cmds)
                    yield return c;
            }
        }
           

        public virtual ObjectSaverButton GetObjectSaverButton()
        {
            return ObjectSaverButton1;
        }
    }
}
