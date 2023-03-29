using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.IdsSchema
{
    internal interface IStringListMatcher
    {
        Audit.Status DoesMatch(IEnumerable<string> strings, ILogger? logger, out IEnumerable<string> matches, string listToMatchName);
    }
}
