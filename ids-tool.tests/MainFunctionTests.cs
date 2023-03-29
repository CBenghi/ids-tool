using IdsTool;
using System.Collections.Generic;
using System.IO;
using Xunit;
using static IdsLib.Audit;

namespace idsTool.tests;

public class MainFunctionTests
{
    private const string schemaFile = @"bsFiles/ids093.xsd";
    private const string idsFile = @"bsFiles/IDS_ucms_prefab_pipes_IFC2x3.ids";

    [Fact]
    public void CanRunProvidingSchema()
    {
        var c = new AuditOptions
        {
            SchemaFiles = new List<string> { schemaFile },
            InputSource = idsFile
        };

        // to adjust once we fix the xml file in the other repo.
        var ret = Run(c);
        Assert.Equal(Status.Ok, ret);
    }

    [Fact]
    public void CanRunWithNoSchema()
    {
        var c = new AuditOptions
        {
            InputSource = idsFile
        };

        // to adjust once we fix the xml file in the other repo.
        var ret = Run(c);
        Assert.Equal(Status.Ok, ret);
    }

    [Fact]
    public void DoesNotBlockFiles()
    {
        // prepare the file to delete in the end
        var tmp = Path.GetTempFileName();
        File.Copy(idsFile, tmp, true);

        var c = new AuditOptions
        {
            SchemaFiles = new List<string> { schemaFile },
            InputSource = tmp
        };
        Run(c);
        File.Delete(tmp);
    }
}