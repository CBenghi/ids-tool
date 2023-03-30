using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsEntity : BaseContext
{
    private static readonly string[] SpecificationArray = { "specification" };

    public IdsEntity(System.Xml.XmlReader reader) : base(reader)
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
        var name = GetChildNodes("name").FirstOrDefault();

        // one child must be a valid string matcher
        var sm = name.Children.OfType<IStringListMatcher>().FirstOrDefault();
        if (sm is null)
            return IdsLoggerExtensions.ReportNoStringMatcher(logger, this, "name");
        var ValidClassNames = SchemaInfo.AllClasses
            .Where(x => (x.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions)
            .Select(y => y.IfcClassName.ToUpperInvariant());
        var ret = sm.DoesMatch(ValidClassNames, false, logger, out var validClasses, "entity names");

        // predefined types have to match the set of the types that are
        // valid across all schemas of the specification
        // 
        var type = GetChildNodes("predefinedType");



        return ret;
    }
}
