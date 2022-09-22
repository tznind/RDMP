// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.AtomicCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rdmp.Core.Curation.Data
{
    /// <summary>
    /// The virtual 'folder' in which to display objects.  This should be a helpful subdivision e.g. "\core datasets\labsystems\"
    ///  
    /// <para>CatalogueFolder are represented in the user interface as a tree of folders (calculated at runtime). You can't create an empty CatalogueFolder,
    /// just declare an <see cref="IHasFolder"/> (e.g. <see cref="Catalogue"/>) as being in a new folder and it will be automatically shown.</para>
    /// 
    /// <para>CatalogueFolder is a static class that contains helper methods to help prevent illegal paths and to calculate hierarchy based on multiple <see cref="IHasFolder"/> 
    /// (See <see cref="GetImmediateSubFoldersUsing"/>)</para>
    /// </summary>
    public static class FolderHelper
    {         
        /// <summary>
        /// The topmost folder under which all other folders reside
        /// </summary>
        public const string Root = "\\";
        

        public static string Adjust(string candidate)
        {
            if (IsValidPath(candidate, out var reason))
            {
                candidate = candidate.ToLower();

                //ensure it ends with a slash
                if (!candidate.EndsWith("\\"))
                    candidate += "\\";
            }
            else
                throw new NotSupportedException(reason);

            return candidate;
        }

        public static bool IsValidPath(string candidatePath, out string reason)
        {
            reason = null;

            if (string.IsNullOrWhiteSpace(candidatePath))
                reason = "An attempt was made to set Catalogue Folder to null, every Catalogue must have a folder, set it to \\ if you want the root";
            else
            if (!candidatePath.StartsWith("\\"))
                reason = "All catalogue paths must start with \\.  Invalid path was:" + candidatePath;
            else
            if (candidatePath.Contains("\\\\"))//if it contains double slash
                reason = "Catalogue paths cannot contain double slashes '\\\\', Invalid path was:" + candidatePath;
            else
            if (candidatePath.Contains("/"))//if it contains double slash
                reason = "Catalogue paths must use backwards slashes not forward slashes, Invalid path was:" + candidatePath;

            return reason == null;
        }

        /// <summary>
        /// Returns true if the specified path is valid for a <see cref="IHasFolder"/>.  Not blank, starts with '\' etc.
        /// </summary>
        /// <param name="candidatePath"></param>
        /// <returns></returns>
        public static bool IsValidPath(string candidatePath)
        {
            return IsValidPath(candidatePath, out _);
        }

        /// <summary>
        /// Returns true if the <paramref name="candidate"/> is in a subfolder of
        /// <paramref name="potentialParent"/>.
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="potentialParent"></param>
        /// <returns></returns>
        public static bool IsSubFolderOf(string candidate, string potentialParent)
        {
            if (potentialParent == null)
                return false;

            //they are the same folder so not subfoldres
            if (candidate.Equals(potentialParent))
                return false;

            //we contain the potential parents path therefore we are a child of them
            return candidate.StartsWith(potentialParent);
        }

        /// <summary>
        /// Returns the next level of folder down towards the Catalogues in collection - note that the next folder down might be empty 
        /// e.g.
        /// 
        /// <para>Pass in 
        /// CatalogueA - \2005\Research\
        /// CatalogueB - \2006\Research\</para>
        /// 
        /// <para>This is Root (\)
        /// Returns:
        ///     \2005\ - empty 
        ///     \2006\ - empty</para>
        /// 
        /// <para>Pass in the SAME collection but call on This (\2005\)
        /// Returns :
        /// \2005\Research\ - containing CatalogueA</para>
        /// </summary>
        /// <param name="currentFolder"></param>
        /// <param name="collection"></param>
        public static string[] GetImmediateSubFoldersUsing(string currentFolder,IEnumerable<IHasFolder> collection)
        {
            List<string> toReturn = new List<string>();


            var remoteChildren = collection.Where(c => IsSubFolderOf(c.Folder, currentFolder)).Select(c=>c.Folder).ToArray();

            //no subfolders exist
            if (!remoteChildren.Any())
                return toReturn.ToArray();//empty
            

            foreach (string child in remoteChildren)
            {
                // We are \bob\

                //we are looking at \bob\fish\smith\harry\

                //chop off \bob\
                string trimmed = child.Substring(currentFolder.Length);

                //trimmed = fish\smith\harry\

                string nextFolder = trimmed.Substring(0, trimmed.IndexOf('\\')+1);
                
                //nextFolder = fish\

                //add 
                toReturn.Add(currentFolder + nextFolder);
            }

            return toReturn.Distinct().ToArray();

        }

        public static FolderNode BuildFolderTree(IHasFolder[] objects, FolderNode currentBranch = null)
        {
            currentBranch ??= new FolderNode(Root);
            var currentBranchFullName = currentBranch.FullName;

            foreach (var g in objects.GroupBy(g => g.Folder).ToArray())
            {
                if(g.Key.Equals(currentBranchFullName, StringComparison.CurrentCultureIgnoreCase))
                {
                    // all these are in the exact folder we are looking at, they are our children
                    currentBranch.ChildObjects.AddRange(g);
                }
                else
                {
                    // these objects are in a subdirectory of us.  Find the next subdirectory name
                    // bearing in mind we may be at '\' and be seing '\dog\cat\fish' as the next
                    var idx = g.Key.IndexOf(currentBranchFullName, StringComparison.CurrentCultureIgnoreCase) + currentBranchFullName.Length;

                    // if we have objects that do not live under this full path thats a problem
                    
                    // or its also a problem if we found a full match to the end of the string
                    
                    // this branch deals with sub folders and that would mean the current group
                    // are not in any subfolders
                    if (idx == -1 || idx == g.Key.Length -1)
                    {
                        throw new Exception($"Unable to build folder groups.  Current group was not a child of the current branch.  Branch was '{currentBranch.FullName}' while Group was '{g.Key}'");
                    }
                    
                    var subFolders = g.Key.Substring(idx);
                    var nextFolder = subFolders.Split('\\',StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                    if(nextFolder == null)
                    {
                        throw new Exception($"Unable to build folder groups.  Current group had malformed Folder name.  Branch was '{currentBranch.FullName}' while Group was '{g.Key}'");
                    }
                    else
                    {
                        var f = new FolderNode(nextFolder,currentBranch);
                        currentBranch.ChildFolders.Add(f);

                        BuildFolderTree(g.ToArray(),f);
                    }
                }
            }

            return currentBranch;
        }
    }

    public class FolderNode
    {
        public string Name { get; set; }
        public List<IHasFolder> ChildObjects { get; set; } = new();
        public List<FolderNode> ChildFolders { get; set; } = new();

        public FolderNode Parent { get; set; }

        public string FullName => GetFullName();

        public FolderNode(string name, FolderNode parent = null)
        {
            Name = name;
            Parent = parent;
        }

        private string GetFullName()
        {
            // build the name by prepending each parent
            // but start with our name
            StringBuilder sb = new(Name);

            var p = Parent;            

            while(p != null)
            {
                if(p.Name.Equals(FolderHelper.Root))
                {
                    sb.Insert(0, p.Name);
                }
                else
                {
                    sb.Insert(0, p.Name + "\\");
                }
                                
                p = p.Parent;
            }

            return sb.ToString();
        }

        public FolderNode this[string key]
        {
            get => GetChild(key);
        }

        private FolderNode GetChild(string key)
        {
            return ChildFolders.FirstOrDefault(c => c.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                ?? throw new ArgumentOutOfRangeException($"Could not find a child folder with the key '{key}'");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
