using System;

namespace IdsLib.IfcSchema
{
    [Flags]
    public enum IfcSchemaVersions
    {
        [IfcSchema(false)]
        IfcNoVersion = 0,
        [IfcSchema(true)]
        Ifc2x3 = 1 << 0,
        [IfcSchema(true)]
        Ifc4 = 1 << 1,
        [IfcSchema(true)]
        Ifc4x3 = 1 << 2,
        [IfcSchema(false)]
        IfcAllVersions = (1 << 3) - 1
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class IfcSchemaAttribute : Attribute
    {
        public bool IsSpecificAttribute = false;
        public IfcSchemaAttribute(bool isSpecific)
        {
            IsSpecificAttribute = isSpecific;
        }
    }
}
