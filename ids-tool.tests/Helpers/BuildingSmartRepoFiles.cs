using FluentAssertions;
using System.Collections.Generic;
using System.IO;

namespace idsTool.tests.Helpers;

public class BuildingSmartRepoFiles
{
    private const string IdsTestcasesPath = @"..\..\..\..\..\IDS\Documentation\testcases";

    internal static FileInfo GetTestCaseFileInfo(string idsFile)
    {
        var d = new DirectoryInfo(IdsTestcasesPath);
        var comb = d.FullName + idsFile;
        var f = new FileInfo(comb);
        f.Exists.Should().BeTrue("test file must be found");
        return f;
    }

    public static IEnumerable<object[]> GetTestCaseIdsFiles()
    {
        // start from current directory and look in relative position for the bs IDS repository
        var d = new DirectoryInfo(IdsTestcasesPath);
        foreach (var f in d.GetFiles("*.ids", SearchOption.AllDirectories))
        {
            yield return new object[] { f.FullName.Replace(d.FullName, "") };
        }
    }

    private const string IdsDevelopmentPath = @"..\..\..\..\..\IDS\Development";

    internal static FileInfo GetDevelopmentFileInfo(string idsFile)
    {
        var d = new DirectoryInfo(IdsDevelopmentPath);
        var comb = d.FullName + idsFile;
        var f = new FileInfo(comb);
        f.Exists.Should().BeTrue("test file must be found");
        return f;
    }

    public static IEnumerable<object[]> GetDevelopmentIdsFiles()
    {
        // start from current directory and look in relative position for the bs IDS repository
        var d = new DirectoryInfo(IdsDevelopmentPath);
        foreach (var f in d.GetFiles("*.ids", SearchOption.AllDirectories))
        {
            yield return new object[] { f.FullName.Replace(d.FullName, "") };
        }
    }
}
