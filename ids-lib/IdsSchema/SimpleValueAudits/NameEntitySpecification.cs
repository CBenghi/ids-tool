using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.IdsSchema
{
    internal class NameEntitySpecification : IAuditAction
    {
        private readonly IfcSchema.IfcSchemaVersions requiredSchemaVersions;
        public NameEntitySpecification(IdsSpecification spec)
        {
            requiredSchemaVersions = spec.SchemaVersions;
        }

        public static NameEntitySpecification? FromNodes(List<BaseContext> nodes, ILogger? logger)
        {
            if (nodes[2] is not IdsSpecification spec)
            {
                logger?.ReportUnexpectedScenario($"Mismatch of node type for specification", nodes[2]);
                return null;
            }
            return new NameEntitySpecification(spec);
        }

        public Check.Status Audit(IdsSimpleValue idsSimpleValue, ILogger? logger)
        {
            var requiredClassName = idsSimpleValue.Content;
            var ifcClass = SchemaInfo.AllClasses.FirstOrDefault(x => x.IfcClassName.ToUpperInvariant() == requiredClassName);
            if (ifcClass is null)
            {
                logger?.LogError("Invalid class name {className}", requiredClassName);
                return Check.Status.IdsContentError;
            }
            var match = (ifcClass.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions;
            if (!match)
            {
                logger?.LogError("Mismatch in expected schema compatibility for {className} and {schema}", requiredClassName, requiredSchemaVersions);
                return Check.Status.IdsContentError;
            }
            return Check.Status.Ok;
        }
    }
}
