using FluentAssertions;
using IdsLib.Helpers;
using IdsLib.IdsSchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace idsTool.tests
{
    public class IdsInfoTests
    {
        [Theory]
        [InlineData("InvalidFiles/empty.ids", false)]
        [InlineData("InvalidFiles/InvalidSchemaLocation.ids", true)]
        [InlineData("InvalidFiles/notAnIdsElement.ids", false)]
        [InlineData("InvalidFiles/notAnXml.ids", false)]
        [InlineData("InvalidFiles/smallcross_gif.ids", false)]
        public async Task InvalidFilesDontBreak(string idsFile, bool isIds)
        {
            var f = new FileInfo(idsFile);
            var t = await IdsXmlHelpers.GetIdsInformationAsync(f);
            t.Should().NotBeNull();
            t.Version.Should().Be(IdsVersion.Invalid);
            t.IsIds.Should().Be(isIds);
        }


        [Theory]
        [MemberData(nameof(GetDevelopmentIdsFiles))]
        public async Task CanReadIdsDevelopmentFiles(string idsFile)
        {
            FileInfo f = GetDevelopmentFile(idsFile);
            var t = await IdsXmlHelpers.GetIdsInformationAsync(f);
            t.Should().NotBeNull();
            t.Version.Should().NotBe(IdsVersion.Invalid);
        }

        [Theory]
        [MemberData(nameof(GetTestCaseIdsFiles))]
        public async Task CanReadIdsTestCases(string idsFile)
        {
            FileInfo f = GetTestCaseFile(idsFile);
            var t = await IdsXmlHelpers.GetIdsInformationAsync(f);
            t.Should().NotBeNull();
            t.Version.Should().NotBe(IdsVersion.Invalid);
        }

        #region get test case files
        private const string IdsTestcasesPath = @"..\..\..\..\..\IDS\Documentation\testcases";

        private static FileInfo GetTestCaseFile(string idsFile)
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
        #endregion

        #region get Development files

        private const string IdsDevelopmentPath = @"..\..\..\..\..\IDS\Development";

        private static FileInfo GetDevelopmentFile(string idsFile)
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

        #endregion

    }
}
