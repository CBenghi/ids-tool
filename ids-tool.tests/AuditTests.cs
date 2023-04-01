using IdsLib;
using idsTool.tests.Helpers;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests;

public class AuditTests : BuildingSmartRepoFiles
{
    public AuditTests(ITestOutputHelper outputHelper)
    {
        XunitOutputHelper = outputHelper;
    }
    private ITestOutputHelper XunitOutputHelper { get; }

    [Theory]
    [MemberData(nameof(GetDevelopmentIdsFiles))]
    public void FullAuditOfDevelopmentFilesOk(string developmentIdsFile)
    {
        FileInfo f = GetDevelopmentFileInfo(developmentIdsFile);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

    [Theory]
    [MemberData(nameof(GetTestCaseIdsFiles))]
    public void OmitContentAuditOfDocumentationFilesOk(string developmentIdsFile)
    {
        FileInfo f = GetTestCaseFileInfo(developmentIdsFile);
        LoggerAndAuditHelpers.OmitContentAudit(f, XunitOutputHelper, Audit.Status.Ok, 0);
    }

    [Theory]
    [InlineData("InvalidFiles/InvalidIfcVersion.ids", 2, Audit.Status.IdsStructureError)]
    [InlineData("InvalidFiles/InvalidIfcOccurs.ids", 1, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidEntityNames.ids", 3, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidAttributeNames.ids", 2, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPattern.ids", 4, Audit.Status.IdsContentError)]
    [InlineData("InvalidFiles/InvalidIfcEntityPredefinedType.ids", 5, Audit.Status.IdsContentError)]
    public void FullAuditFail(string path, int numErr, Audit.Status status)
    {
        var f = new FileInfo(path);
        LoggerAndAuditHelpers.FullAudit(f, XunitOutputHelper, status, numErr);
    }
}
