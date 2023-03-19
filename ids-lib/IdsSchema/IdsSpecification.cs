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
        private readonly IfcSchemaVersions sVersion = IfcSchemaVersions.IfcNoVersion;

        public IdsSpecification(System.Xml.XmlReader reader, ILogger? logger) : base(reader)
        {
            m = new MinMaxOccurr(reader);
            var vrs = reader.GetAttribute("ifcVersion") ?? string.Empty;
            sVersion = vrs.GetSchemaVersions(this, logger);
        }

        internal protected override Check.Status Audit(ILogger? logger)
        {
            var ret = Check.Status.Ok;
            if (m.Audit() != Check.Status.Ok)
                ret |= logger.ReportInvalidOccurr(this, m);
            if (sVersion == IfcSchemaVersions.IfcNoVersion)
                ret |= logger.ReportInvalidSchemaVersion(sVersion, this);
            return base.Audit(logger) | ret;
        }
    }
}
