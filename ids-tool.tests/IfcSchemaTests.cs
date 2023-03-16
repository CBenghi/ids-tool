using FluentAssertions;
using IdsLib.IfcSchema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace idsTool.tests
{
    public class IfcSchemaTests
    {
        [Fact]
        public void CanGetClasses()
        {
            var root = SchemaInfo.AllClasses.FirstOrDefault(x => x.IfcClassName == "IfcRoot");
            root.Should().NotBeNull();
            root.ValidSchemaVersions.Should().NotBe(IfcSchemaVersions.IfcNoVersion);
            root.ValidSchemaVersions.Should().Be(IfcSchemaVersions.IfcAllVersions);
        }
    }
}
