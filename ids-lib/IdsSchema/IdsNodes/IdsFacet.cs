using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema.IdsNodes;

internal class IdsFacet : BaseContext, IIdsRequirementFacet
{
    private readonly MinMaxOccurr minMaxOccurr;
    public IdsFacet(System.Xml.XmlReader reader) : base(reader)
    {
        minMaxOccurr = new MinMaxOccurr(reader);
    }

    public Audit.Status PerformAuditAsRequirement(ILogger? logger)
    {
        if (minMaxOccurr.Audit() != Audit.Status.Ok)
            return logger.ReportInvalidOccurr(this, minMaxOccurr);
        return Audit.Status.Ok;
    }
}
