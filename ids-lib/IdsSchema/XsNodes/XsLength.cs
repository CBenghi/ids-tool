﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsLength : BaseContext, IStringListMatcher
{
    private readonly string value;
    public XsLength(XmlReader reader) : base(reader)
    {
        value = reader.GetAttribute("value") ?? string.Empty;
    }

    public Audit.Status DoesMatch(IEnumerable<string> candidateStrings, bool ignoreCase, ILogger? logger, out IEnumerable<string> matches, string listToMatchName)
    {
        if (!int.TryParse(value, out var len)) 
        {
            matches = Enumerable.Empty<string>();   
            return IdsLoggerExtensions.ReportInvalidClassMatcher(this, value, logger, listToMatchName);
        }
        matches = candidateStrings.Where(x=>x.Length == len).ToList();
        return matches.Any()
           ? Audit.Status.Ok
           : Audit.Status.IdsContentError;
    }


    protected internal override Audit.Status PerformAudit(ILogger? logger)
    {
        // Debug.WriteLine($"Children: {Children.Count}");
        return base.PerformAudit(logger);
    }
}
