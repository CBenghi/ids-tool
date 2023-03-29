using IdsLib.IdsSchema.IdsNodes;
using Microsoft.Extensions.Logging;

namespace IdsLib.IdsSchema;

internal interface IAuditAction
{
    Audit.Status Audit(IdsSimpleValue idsSimpleValue, ILogger? logger);
}
