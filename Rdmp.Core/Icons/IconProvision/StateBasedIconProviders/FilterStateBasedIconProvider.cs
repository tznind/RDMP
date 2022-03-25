﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;
using System.Drawing;
using System.Linq;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class FilterStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _basicIcon;
        private IconOverlayProvider _overlayProvider;

        public FilterStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _basicIcon = CatalogueIcons.Filter;
            _overlayProvider = overlayProvider;

        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (o is ExtractionFilter f)
            {
                // has known parameter values?
                if(f.ExtractionFilterParameterSets.Any())
                {
                    return _overlayProvider.GetOverlay(_basicIcon, OverlayKind.Parameter);
                }

                // just a regular filter then
                return _basicIcon;
            }

            if (CatalogueIconProvider.ConceptIs(typeof(IFilter), o))
                return _basicIcon;

            return null;

        }
    }
}