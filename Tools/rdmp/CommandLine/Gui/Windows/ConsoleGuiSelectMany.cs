﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui.Windows
{
    /// <summary>
    /// Allows the user to select one or more objects in a modal console dialog
    /// </summary>
    public class ConsoleGuiSelectMany : Window
    {
        private ListView lv;
        private readonly IBasicActivateItems _activator;

        /// <summary>
        /// The final object selection made.  Note that you should
        /// also check <see cref="ResultOk"/> in case of user cancellation
        /// </summary>
        public object[] Result { get { return _available.Where((e, idx) => lv.Source.IsMarked(idx)).ToArray(); } }

        /// <summary>
        /// True if the user made a final selection or False if they exited without
        /// making a choice e.g. Hit Ctrl+Q
        /// </summary>
        public bool ResultOk { get; internal set; }
        private object[] _available;

        public ConsoleGuiSelectMany(IBasicActivateItems activator,string prompt, object[] available)
        {
            _available = available;

            lv = new ListView(available);
            lv.AllowsMarking = true;
            lv.AllowsMultipleSelection = true;

            lv.Width = Dim.Fill();
            lv.Height = Dim.Fill();
            lv.KeyPress += Lv_KeyPress;

            Text = prompt;
            Add(lv);
            this._activator = activator;
        }

        private void Lv_KeyPress(KeyEventEventArgs obj)
        {
            if(obj.KeyEvent.Key == Key.Enter)
            {
                // if there are no selected objects (user hits space to select many)
                // then they probably want a single object selection
                if(Result.Length == 0)
                {
                    lv.MarkUnmarkRow();
                }

                obj.Handled = true;
                ResultOk = true;
                Application.RequestStop();
            }
        }
    }
}