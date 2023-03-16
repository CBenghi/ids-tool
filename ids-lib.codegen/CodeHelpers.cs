using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.codegen
{
    internal class CodeHelpers
    {
        internal static string NewStringArray(IEnumerable<string> classes)
        {
            return @$"new[] {{ ""{string.Join("\", \"", classes)}"" }}";
        }
    }
}
