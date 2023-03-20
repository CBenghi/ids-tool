using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.IdsSchema
{
    internal class NameAttributeSpecification : IAuditAction
    {
        private readonly IfcSchema.IfcSchemaVersions requiredSchemaVersions;
        public NameAttributeSpecification(IdsSpecification spec)
        {
            requiredSchemaVersions = spec.SchemaVersions;
        }

        public static NameAttributeSpecification? FromNodes(List<BaseContext> nodes, ILogger? logger)
        {
            if (nodes[2] is not IdsSpecification spec)
            {
                logger?.ReportUnexpectedScenario($"Mismatch of node type for specification", nodes[2]);
                return null;
            }
            return new NameAttributeSpecification(spec);
        }

        public Audit.Status Audit(IdsSimpleValue idsSimpleValue, ILogger? logger)
        {
            var requiredAttributeName = idsSimpleValue.Content;
            var ifcObject = SchemaInfo.AllAttributes.FirstOrDefault(x => x.IfcAttributeName == requiredAttributeName);
            if (ifcObject is null)
            {
                logger?.LogError("Invalid attribute name {attributeName}.", requiredAttributeName);
                return IdsLib.Audit.Status.IdsContentError;
            }
            var match = (ifcObject.ValidSchemaVersions & requiredSchemaVersions) == requiredSchemaVersions;
            if (!match)
            {
                logger?.LogError("Mismatch in expected schema compatibility for {attributeName} and {schema}.", requiredAttributeName, requiredSchemaVersions);
                return IdsLib.Audit.Status.IdsContentError;
            }
            return IdsLib.Audit.Status.Ok;
        }
    }
}
