using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.IfcSchema
{
    internal class IfcClassInformation
    {
        public IfcClassInformation(string name, IEnumerable<string> schemas)
        {
            IfcClassName = name;
            foreach (var scheme in schemas)
            {
                IfcSchemaVersions v = scheme switch
                {
                    "Ifc2x3" => IfcSchemaVersions.Ifc2x3,
                    "Ifc4" => IfcSchemaVersions.Ifc4,
                    "Ifc4x3" => IfcSchemaVersions.Ifc4x3,
                    _ => IfcSchemaVersions.IfcNoVersion,
                };
                if (v == IfcSchemaVersions.IfcNoVersion)
                    continue;
                ValidSchemaVersions |= v;
            }
        }
    
        public string IfcClassName { get; set; } = string.Empty;

        public IfcSchemaVersions ValidSchemaVersions { get; set; } = IfcSchemaVersions.IfcNoVersion;
    }
}
