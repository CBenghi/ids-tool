using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.IdsSchema
{
    internal class IdsSpecification : BaseContext
    {
        private readonly MinMaxOccurr m;
        internal readonly IfcSchemaVersions SchemaVersions = IfcSchemaVersions.IfcNoVersion;

        public IdsSpecification(System.Xml.XmlReader reader, ILogger? logger) : base(reader)
        {
            m = new MinMaxOccurr(reader);
            var vrs = reader.GetAttribute("ifcVersion") ?? string.Empty;
            SchemaVersions = vrs.GetSchemaVersions(this, logger);
        }

        internal protected override Audit.Status Audit(ILogger? logger)
        {
            var ret = IdsLib.Audit.Status.Ok;
            if (m.Audit() != IdsLib.Audit.Status.Ok)
                ret |= logger.ReportInvalidOccurr(this, m);
            if (SchemaVersions == IfcSchemaVersions.IfcNoVersion)
                ret |= logger.ReportInvalidSchemaVersion(SchemaVersions, this);
            return base.Audit(logger) | ret;
        }
    }
}
