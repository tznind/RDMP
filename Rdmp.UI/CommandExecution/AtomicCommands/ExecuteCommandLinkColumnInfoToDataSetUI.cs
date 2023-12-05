﻿// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandLinkColumnInfoToDataSetUI : BasicUICommandExecution
{
    private readonly ColumnInfo _columnInfo;
    private Dataset _selectedDataset;
    private readonly IActivateItems _activateItems;

    public ExecuteCommandLinkColumnInfoToDataSetUI(IActivateItems activator, ColumnInfo columnInfo) : base(activator)
    {
        _columnInfo = columnInfo;
        _activateItems = activator;
    }

    [NotNull]
    public override string GetCommandHelp() =>
        "Link this column to an existing dataset";

    public override void Execute()
    {
        base.Execute();
        var datasets = _activateItems.RepositoryLocator.CatalogueRepository.GetAllObjects<Dataset>();
        DialogArgs da =  new()
        {
            WindowTitle = "Link a dataset with this column",
            TaskDescription =
             "Select the Dataset that this column information came from"
        };
        _selectedDataset = SelectOne(da, datasets);
        var backfill = YesNo("Link all other columns that match the source table?","Do you want to link this dataset to all other columns that reference the same table as this column?");
        var cmd = new ExecuteCommandLinkColumnInfoToDataset(_activateItems,_columnInfo, _selectedDataset,backfill);
        cmd.Execute();

    }

    public override Image<Rgba32> GetImage([NotNull] IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Dataset, OverlayKind.Link);
}