using FluentAssertions;
using IdsLib;
using idsTool.tests.Helpers;
using IdsTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace idsTool.tests
{
    public class SchemaLoadingTests : BuildingSmartRepoFiles
    {
        [Theory]
        [MemberData(nameof(GetDevelopmentIdsFiles))]
        public void CanLoadEmbeddedResourceSchema(string idsFile)
        {
            FileInfo f = GetDevelopmentFile(idsFile);
            var c = new CheckOptions()
            {
                InputSource = f.FullName,
                OmitIdsContentCheck = true,
            };
            var checkResult = Check.Run(c);
            checkResult.Should().Be(Check.Status.Ok);
        }

        [Theory]
        [InlineData("InvalidFiles/InvalidSchemaLocation.ids", Check.Status.IdsStructureError)]
        [InlineData("InvalidFiles/InvalidElementInvalidContent.ids", Check.Status.IdsStructureError)]
        [InlineData("ValidFiles/IDS_aachen_example.ids", Check.Status.Ok)]
        public void CanFailInvalidFileLoadingEmbeddedResourceSchema(string file, Check.Status expected)
        {
            var f = new FileInfo(file);
            var c = new CheckOptions()
            {
                InputSource = f.FullName,
                OmitIdsContentCheck = true,
            };
            var checkResult = Check.Run(c);
            checkResult.Should().Be(expected);
        }
    }
}
