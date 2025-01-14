// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataProvider.FlatFileManipulation;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

[Category("Unit")]
public class ExcelConversionTest
{
    private readonly Stack<DirectoryInfo> _dirsToCleanUp = new();
    private DirectoryInfo _parentDir;

    [OneTimeSetUp]
    protected virtual void OneTimeSetUp()
    {
        var testDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        _parentDir = testDir.CreateSubdirectory("ExcelConversionTest");
        _dirsToCleanUp.Push(_parentDir);
    }

    [OneTimeTearDown]
    protected virtual void OneTimeTearDown()
    {
        while (_dirsToCleanUp.Count > 0)
        {
            var dir = _dirsToCleanUp.Pop();
            if (dir.Exists)
                dir.Delete(true);
        }
    }

    private LoadDirectory CreateLoadDirectoryForTest(string directoryName)
    {
        var loadDirectory = LoadDirectory.CreateDirectoryStructure(_parentDir, directoryName, true);
        _dirsToCleanUp.Push(loadDirectory.RootPath);
        return loadDirectory;
    }

    [Test]
    public void TestExcelFunctionality_OnSimpleXlsx()
    {
        var LoadDirectory = CreateLoadDirectoryForTest("TestExcelFunctionality_OnSimpleXlsx");

        //clean SetUp anything in the test project folders forloading directory
        foreach (var fileInfo in LoadDirectory.ForLoading.GetFiles())
            fileInfo.Delete();

        var targetFile = Path.Combine(LoadDirectory.ForLoading.FullName, "Test.xlsx");

        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "DataLoad", "Engine",
            "Resources", "Test.xlsx"));

        FileAssert.Exists(fi);

        fi.CopyTo(targetFile, true);

        TestConversionFor(targetFile, "*.xlsx", 5, LoadDirectory);
    }

    [Test]
    public void TestExcelFunctionality_DodgyFileExtension()
    {
        var LoadDirectory = CreateLoadDirectoryForTest("TestExcelFunctionality_DodgyFileExtension");

        //clean SetUp anything in the test project folders forloading directory
        foreach (var fileInfo in LoadDirectory.ForLoading.GetFiles())
            fileInfo.Delete();

        var targetFile = Path.Combine(LoadDirectory.ForLoading.FullName, "Test.xml");
        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "DataLoad", "Engine",
            "Resources", "XmlTestForExcel.xml"));

        FileAssert.Exists(fi);

        fi.CopyTo(targetFile, true);

        var ex = Assert.Throws<Exception>(() => TestConversionFor(targetFile, "*.fish", 1, LoadDirectory));

        Assert.That(ex.Message, Does.StartWith("Did not find any files matching Pattern '*.fish' in directory"));
    }

    private static void TestConversionFor(string targetFile, string fileExtensionToConvert, int expectedNumberOfSheets,
        LoadDirectory directory)
    {
        var f = new FileInfo(targetFile);

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(f.Exists);
                Assert.That(f.Length, Is.GreaterThan(100));
            });

            var converter = new ExcelToCSVFilesConverter();

            var job = new ThrowImmediatelyDataLoadJob(ThrowImmediatelyDataLoadEventListener.QuietPicky)
            {
                LoadDirectory = directory
            };

            converter.ExcelFilePattern = fileExtensionToConvert;
            converter.Fetch(job, new GracefulCancellationToken());

            var filesCreated = directory.ForLoading.GetFiles("*.csv");

            Assert.That(filesCreated, Has.Length.EqualTo(expectedNumberOfSheets));

            foreach (var fileCreated in filesCreated)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(Regex.IsMatch(fileCreated.Name, "Sheet[0-9].csv"));
                    Assert.That(fileCreated.Length, Is.GreaterThanOrEqualTo(100));
                });
                fileCreated.Delete();
            }
        }
        finally
        {
            f.Delete();
        }
    }
}