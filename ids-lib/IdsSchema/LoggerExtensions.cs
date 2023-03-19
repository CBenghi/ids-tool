using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.IdsSchema
{
    internal static class IdsLoggerExtensions
    {
        internal static Check.Status ReportInvalidOccurr(this ILogger? logger, BaseContext context, MinMaxOccurr m) 
        {
            logger?.LogError("Invalid setting for occurrence on element {tp} at line {line}, position {pos}, {minMax}.", context.type, context.StartLineNumber, context.StartLinePosition, m);
            return Check.Status.IdsContentError;
        }

    }
}
