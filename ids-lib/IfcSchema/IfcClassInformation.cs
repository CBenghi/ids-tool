using System.Collections.Generic;

namespace IdsLib.IfcSchema
{
    internal class IfcClassInformation
    {
        public string IfcClassName { get; set; } = string.Empty;
        public IfcSchemaVersions ValidSchemaVersions { get; set; } = IfcSchemaVersions.IfcNoVersion;
        public IfcClassInformation(string name, IEnumerable<string> schemas)
        {
            IfcClassName = name;
            ValidSchemaVersions = IfcSchema.GetSchema(schemas);
        }
    }
}
