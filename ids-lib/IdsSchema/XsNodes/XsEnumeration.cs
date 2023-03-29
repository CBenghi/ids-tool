using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace IdsLib.IdsSchema.XsNodes;

internal class XsEnumeration : BaseContext, IStringMatchValue
{
    private readonly string value;
    public XsEnumeration(XmlReader reader) : base(reader)
    {
        value = reader.GetAttribute("value") ?? string.Empty;
    }

    public string Value => value;

    public Audit.Status DoesMatch(IEnumerable<string> strings, ILogger? logger, out IEnumerable<string> matches)
    {
        matches = strings.Where(x => x == value).ToList();
        return matches.Any()
            ? IdsLib.Audit.Status.Ok
            : IdsLib.Audit.Status.IdsContentError;
    }

    protected internal override Audit.Status Audit(ILogger? logger)
    {
        Debug.WriteLine($"Children: {Children.Count}");
        return base.Audit(logger);
    }
}
