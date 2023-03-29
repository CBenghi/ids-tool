using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsRestriction : BaseContext, IStringListMatcher
{
    private readonly string baseAsString;
    public XsRestriction(XmlReader reader) : base(reader)
    {
        baseAsString = reader.GetAttribute("base") ?? string.Empty;
    }

    public Audit.Status DoesMatch(IEnumerable<string> stringsToMatch, ILogger? logger, out IEnumerable<string> matches, string listToMatchName)
    {
        matches =  Enumerable.Empty<string>();
        if (baseAsString != "xs:string")
            return IdsLoggerExtensions.ReportBadType(logger, this, baseAsString);
        var ret = IdsLib.Audit.Status.Ok;
        foreach (var child in Children)
        {
            if (child is not IStringMatchValue imv)
            {
                ret |= IdsLoggerExtensions.ReportBadMatcher(logger, child, "string");
                continue;
            }
            var thisRes = imv.DoesMatch(stringsToMatch, logger, out var thisM);
            if (!thisM.Any())
            {
                ret |= IdsLoggerExtensions.ReportInvalidClassMatcher(child, imv, logger, listToMatchName);
            }
            else
            {
                matches = matches.Union(thisM);
            }
        }
        return ret;
    }

   

    protected internal override Audit.Status Audit(ILogger? logger)
    {
        Debug.WriteLine($"Children: {Children.Count}");
        return base.Audit(logger);
    }
}
