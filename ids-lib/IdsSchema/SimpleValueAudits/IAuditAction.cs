using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema
{
    internal interface IAuditAction
    {
        Check.Status Audit(IdsSimpleValue idsSimpleValue, ILogger? logger);
    }
}
