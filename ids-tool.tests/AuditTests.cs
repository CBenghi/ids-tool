using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using IdsTool;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace idsTool.tests
{
    public class AuditTests : BuildingSmartRepoFiles
    {
        public AuditTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }
        private ITestOutputHelper OutputHelper { get; }

        internal ILogger GetXunitLogger()
        {
            var services = new ServiceCollection()
                        .AddLogging((builder) => builder.AddXUnit(OutputHelper));
            IServiceProvider provider = services.BuildServiceProvider();
            var logg = provider.GetRequiredService<ILogger<AuditTests>>();
            Assert.NotNull(logg);
            return logg;
        }

        [Theory]
        [MemberData(nameof(GetDevelopmentIdsFiles))]
        public void FullAuditOfDevelopmentFilesOk(string developmentIdsFile)
        {
            FileInfo f = GetDevelopmentFileInfo(developmentIdsFile);
            FullAudit(f, 0, Audit.Status.Ok);
        }

        [Theory]
        [MemberData(nameof(GetTestCaseIdsFiles))]
        public void OmitContentAuditOfDocumentationFilesOk(string developmentIdsFile)
        {
            FileInfo f = GetTestCaseFileInfo(developmentIdsFile);
            OmitContentAudit(f, 0, Audit.Status.Ok);
        }

        private void FullAudit(FileInfo f, int expectedWarnAndErrors, Audit.Status expectedOutcome)
        {
            var c = new AuditOptions()
            {
                InputSource = f.FullName,
                OmitIdsContentAudit = false,
            };
            AuditWithOptions(expectedWarnAndErrors, expectedOutcome, c);
        }

        private void AuditWithOptions(int expectedWarnAndErrors, Audit.Status expectedOutcome, AuditOptions c)
        {
            ILogger logg = GetXunitLogger();
            var checkResult = Audit.Run(c, logg); // run for xunit output of logging
            checkResult.Should().Be(expectedOutcome);

            var loggerMock = new Mock<ILogger<AuditTests>>();
            Audit.Run(c, loggerMock.Object); // run for testing of log errors and warnings
            CheckErrorsAndWarnings(loggerMock, expectedWarnAndErrors);
        }

        private void OmitContentAudit(FileInfo f, int expectedWarnAndErrors, Audit.Status expectedOutcome)
        {
            var c = new AuditOptions()
            {
                InputSource = f.FullName,
                OmitIdsContentAudit = true,
                SchemaFiles = new[] { "/bsFiles/ids093.xsd" }
            };
            AuditWithOptions(expectedWarnAndErrors, expectedOutcome, c);
        }

        [Theory]
        [InlineData("InvalidFiles/InvalidIfcVersion.ids", 2, Audit.Status.IdsStructureError)]
        [InlineData("InvalidFiles/InvalidIfcOccurs.ids", 1, Audit.Status.IdsContentError)]
        [InlineData("InvalidFiles/InvalidEntityNames.ids", 3, Audit.Status.IdsContentError)]
        [InlineData("InvalidFiles/InvalidAttributeNames.ids", 2, Audit.Status.IdsContentError)]
        public void FullAuditFail(string path, int numErr, Audit.Status status)
        {
            var f = new FileInfo(path);
            FullAudit(f, numErr, status);
        }

        private static void CheckErrorsAndWarnings(Mock<ILogger<AuditTests>> loggerMock, int expectedErrCount)
        {
            var loggingCalls = loggerMock.Invocations.Select(x => x.ToString() ?? "").ToArray(); // this creates the array of logging calls
            var errorAndWarnings = loggingCalls.Where(x => x.Contains("LogLevel.Error") || x.Contains("LogLevel.Warning"));
            errorAndWarnings.Count().Should().Be(expectedErrCount, "mismatch with expected error/warning count");
        }
    }
}
