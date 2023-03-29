using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal partial class IdsSimpleValue : BaseContext, IStringListMatcher
{
    internal string Content = string.Empty;

    public IdsSimpleValue(System.Xml.XmlReader reader) : base(reader)
    {
    }

    protected internal override void SetContent(string contentString)
    {
        Content = contentString ?? string.Empty;
    }

    protected internal override Audit.Status Audit(ILogger? logger)
    {
        // because the class is used in very many different scenarios,
        // what we actually have to check depends on the context
        // 
        var auditAction = GetAuditAction(logger);
        return auditAction?.Audit(this, logger) ?? base.Audit(logger);
    }

    private static readonly string[] NameEntitySpecificationArray = { "name", "entity", "specification" };
    private static readonly string[] NameAttributeSpecificationArray = { "name", "attribute", "specification" };

    private IAuditAction? GetAuditAction(ILogger? logger)
    {
        if (TryGetUpperNodes(this, NameEntitySpecificationArray, out var nameEntityNodes))
            return NameEntitySpecification.FromNodes(nameEntityNodes, logger);
        if (TryGetUpperNodes(this, NameAttributeSpecificationArray, out var nameAttributeNodes))
            return NameAttributeSpecification.FromNodes(nameAttributeNodes, logger);
        return null;
    }

    public Audit.Status DoesMatch(IEnumerable<string> strings, ILogger? logger, out IEnumerable<string> matches, string listToMatchName)
    {
        matches = strings.Where(x => x == Content).ToList();
        return matches.Any()
            ? IdsLib.Audit.Status.Ok
            : IdsLib.Audit.Status.IdsContentError;
    }
}
