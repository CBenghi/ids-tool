using IdsLib.IfcSchema;
using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsSpecification : BaseContext
{
    private readonly MinMaxOccurr minMaxOccurr;
    internal readonly IfcSchemaVersions SchemaVersions = IfcSchemaVersions.IfcNoVersion;

    private BaseContext? parent;
    protected override internal BaseContext? Parent
    {
        get => parent;
        set
        {
            // we are not storing in this in the parent children, to save memory,
            // because it's not needed at this stage
            // 
            parent = value;
        }
    }


    public IdsSpecification(System.Xml.XmlReader reader, ILogger? logger) : base(reader)
    {
        minMaxOccurr = new MinMaxOccurr(reader);
        var vrs = reader.GetAttribute("ifcVersion") ?? string.Empty;
        SchemaVersions = vrs.GetSchemaVersions(this, logger);
    }

    internal protected override Audit.Status Audit(ILogger? logger)
    {
        var ret = IdsLib.Audit.Status.Ok;
        if (minMaxOccurr.Audit() != IdsLib.Audit.Status.Ok)
            ret |= logger.ReportInvalidOccurr(this, minMaxOccurr);
        if (SchemaVersions == IfcSchemaVersions.IfcNoVersion)
            ret |= logger.ReportInvalidSchemaVersion(SchemaVersions, this);
        return base.Audit(logger) | ret;
    }
}
