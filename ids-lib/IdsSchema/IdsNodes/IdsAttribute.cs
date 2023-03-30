using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsAttribute : BaseContext
{
    private static readonly string[] SpecificationArray = { "specification" };

    public IdsAttribute(System.Xml.XmlReader reader) : base(reader)
    {
    }

    internal protected override Audit.Status Audit(ILogger? logger)
    {
        if (!TryGetUpperNodes(this, SpecificationArray, out var nodes))
        {
            IdsLoggerExtensions.ReportUnexpectedScenario(logger, "Missing specification for entity.", this);
            return IdsLib.Audit.Status.IdsStructureError;
        }
        if (nodes[0] is not IdsSpecification spec)
        {
            IdsLoggerExtensions.ReportUnexpectedScenario(logger, "Invalid specification for entity.", this);
            return IdsLib.Audit.Status.IdsContentError;
        }
        var requiredSchemaVersions = spec.SchemaVersions;
        var names = GetChildNodes("name");
        
        var ret = IdsLib.Audit.Status.Ok;
        foreach (var name in names)
        {
            // the first child must be a valid string matcher
            if (!name.Children.Any())
                return IdsLoggerExtensions.ReportNoStringMatcher(logger, this, "name");
            if (name.Children[0] is not IStringListMatcher sm)
                return IdsLoggerExtensions.ReportInvalidStringMatcher(logger, name.Children[0], "name");
            var ValidClassNames = SchemaInfo.AllAttributes
                .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
                .Select(y => y.IfcAttributeName);
            var result = sm.DoesMatch(ValidClassNames, false, logger, out var matches, "attribute names");
            ret |= result;
        }
        return ret;
    }
}
