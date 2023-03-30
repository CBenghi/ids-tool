using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsEnumeration : BaseContext, IStringListMatcher
{
    private readonly string value;
    public XsEnumeration(XmlReader reader) : base(reader)
    {
        value = reader.GetAttribute("value") ?? string.Empty;
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName)
    {
        var compCase = ignoreCase
                    ? System.StringComparison.OrdinalIgnoreCase
                    : System.StringComparison.Ordinal;
        matches = candidateStrings.Where(x => x.Equals(value, compCase)).ToList();
        if (!matches.Any())
            return IdsLoggerExtensions.ReportInvalidClassMatcher(this, value, logger, listToMatchName);
        return IdsLib.Audit.Status.Ok;
    }
}
