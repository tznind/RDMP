﻿
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using System;
using System.Linq;
using Rdmp.Core.CommandExecution;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandCreateDatasetTests : CommandCliTests
{
    [Test]
    public void TestDatasetCreationOKParameters() {
        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(),"dataset");
        Assert.DoesNotThrow(()=>cmd.Execute());
    }

    [Test]
    public void TestDatasetCreationNoParameters()
    {
        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), null);
        Assert.Throws<ImpossibleCommandException>(cmd.Execute);
    }

    [Test]
    public void TestDatasetCreationOKExtendedParameters()
    {
        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), "dataset2","somedoi","some source");
        Assert.DoesNotThrow(cmd.Execute);
        var founddataset = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Dataset>().Where(ds => ds.Name == "dataset2" && ds.DigitalObjectIdentifier == "somedoi" && ds.Source == "some source").First();
        Assert.That(founddataset,Is.Not.Null);
    }
}
