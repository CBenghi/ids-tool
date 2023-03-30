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

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName)
    {
        matches =  Enumerable.Empty<string>();
        
        if ( // todo: do we want to force the base requirement?
            !string.IsNullOrWhiteSpace(baseAsString) &&
            baseAsString != "xs:string"
            )
            return IdsLoggerExtensions.ReportBadType(logger, this, baseAsString);
        var ret = IdsLib.Audit.Status.Ok;
        foreach (var child in Children)
        {
            // only matcher values are reported
            if (child is not IStringListMatcher ismv)
            {
                // ret |= IdsLoggerExtensions.ReportBadMatcher(logger, child, "string");
                // this would let xs:annotation pass with no issues
                continue;
            }
            ret |= ismv.DoesMatch(candidateStrings, ignoreCase, logger, out var thisM, listToMatchName);
            if (thisM.Any())
                matches = matches.Union(thisM);   
        }
        return ret;
    }

   

    protected internal override Audit.Status Audit(ILogger? logger)
    {
        Debug.WriteLine($"Children: {Children.Count}");
        return base.Audit(logger);
    }
}
