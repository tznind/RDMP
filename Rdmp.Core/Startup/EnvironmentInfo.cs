﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rdmp.Core.Startup
{

    [Flags]
    public enum PluginFolders
    {
        None = 0,
        Main = 1,
        Windows = 4,
    }

    /// <summary>
    /// Class for describing the runtime environment in which <see cref="Startup"/> is executing e.g. under
    /// Windows / Linux in net461 or netcoreapp2.2.  This determines which plugin binary files are loaded
    /// </summary>
    public class EnvironmentInfo
    {
        /// <summary>
        /// The target framework of the running application e.g. "netcoreapp2.2", "net461".  This determines which
        /// plugins versions are loaded.  Leave blank to not load any plugins.
        /// </summary>
        public PluginFolders PluginsToLoad;

        /// <summary>
        /// Creates a new instance specifying which plugins should be loaded.
        /// </summary>
        public EnvironmentInfo(PluginFolders pluginsToLoad):this()
        {
            PluginsToLoad = pluginsToLoad;
        }

        /// <summary>
        /// Creates a new instance in which plugins are not loaded.
        /// </summary>
        public EnvironmentInfo()
        {
            PluginsToLoad = PluginFolders.None;
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return p == 4 || p == 6 || p == 128;
            }
        }

        /// <summary>
        /// Returns the nupkg archive subdirectory that should be loaded with the current environment 
        /// e.g. /lib/net461
        /// </summary>
        internal IEnumerable<DirectoryInfo> GetPluginSubDirectories(DirectoryInfo root, ICheckNotifier notifier)
        {
            if(!root.Name.Equals("lib"))
                throw new ArgumentException("Expected " + root.FullName + " to be the 'lib' directory");

            // if we are loading the main codebase of plugins
            if (PluginsToLoad.HasFlag(PluginFolders.Main))
            {
                // find the main dir
                var mainDir = root.GetDirectories().FirstOrDefault(d=>string.Equals("main",d.Name,StringComparison.CurrentCultureIgnoreCase));

                if (mainDir != null)
                {
                    // great, go load the dlls in there
                    yield return mainDir;
                }
                else
                {
                    // plugin has no main directory, maybe it is not built correctly
                    notifier.OnCheckPerformed(new CheckEventArgs($"Could not find an expected folder called '/lib/main' in folder:" + root, CheckResult.Warning));
                }   
            }

            // if we are to load the windows specific (e.g. winforms) plugins too?
            if (PluginsToLoad.HasFlag(PluginFolders.Windows))
            {
                // see if current plugin has winforms stuff
                var winDir = root.GetDirectories().FirstOrDefault(d => string.Equals("main", d.Name, StringComparison.CurrentCultureIgnoreCase));

                if (winDir != null)
                {
                    //yes
                    yield return winDir;
                }

                // if no then no big dael
            }
        }
    }
}