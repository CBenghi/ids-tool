using System;
using System.Reflection;

namespace IdsLib.Generator
{
    internal static class SchemaHelper
    {
        public static Module GetModule(string schema)
        {
            if (schema == "Ifc2x3")
                return typeof(Xbim.Ifc2x3.Kernel.IfcProduct).Module;
            else if (schema == "Ifc4")
                return typeof(Xbim.Ifc4.Kernel.IfcProduct).Module;
            else if (schema == "Ifc4x3")
                return typeof(Xbim.Ifc4x3.Kernel.IfcProduct).Module;
            else
                throw new NotImplementedException(schema.ToString());
        }
    }
}
