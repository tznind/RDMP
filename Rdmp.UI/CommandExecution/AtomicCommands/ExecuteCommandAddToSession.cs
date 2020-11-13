﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.Collections
{
    public class ExecuteCommandAddToSession : BasicUICommandExecution,IAtomicCommand
    {
        private IMapsDirectlyToDatabaseTable[] _toAdd;
        private readonly SessionCollectionUI session;

        public ExecuteCommandAddToSession(IActivateItems activator, IMapsDirectlyToDatabaseTable[] toAdd, SessionCollectionUI session):base(activator)
        {
            this._toAdd = toAdd;
            this.session = session;

            if(session == null && !activator.GetSessions().Any())
                SetImpossible("There are no active Sessions");

        }
        public override void Execute()
        {
            base.Execute();
            var ses = session;

            if(ses == null)
            {
                var sessions = Activator.GetSessions().ToArray();
                
                if(sessions.Length == 1)
                    ses = sessions[0];
                else
                {
                    var dlg = new PickOneOrCancelDialog<SessionCollectionUI>(sessions,"Session");
                    if(dlg.ShowDialog() == DialogResult.OK)
                        ses = dlg.Picked;
                }    
            }
            
            if(ses == null)
                return;
            ses.Add(_toAdd);
        }
    }
}