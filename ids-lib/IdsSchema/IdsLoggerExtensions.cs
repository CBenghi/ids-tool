using Microsoft.Extensions.Logging;
using System;

namespace IdsLib.IdsSchema;

internal static class IdsLoggerExtensions
{
    internal static void ReportUnexpectedScenario(this ILogger? logger, string scenarioMessage, BaseContext context)
    {
        logger?.LogCritical("Unhandled scenario: {message} on element {tp} at line {line}, position {pos}.", scenarioMessage, context.type, context.StartLineNumber, context.StartLinePosition);
    }
    internal static Audit.Status ReportInvalidOccurr(this ILogger? logger, BaseContext context, MinMaxOccurr m) 
    {
        logger?.LogError("Invalid setting for occurrence on element {tp} at line {line}, position {pos}, {minMax}.", context.type, context.StartLineNumber, context.StartLinePosition, m);
        return Audit.Status.IdsContentError;
    }

    /// <param name="alternative">no end fullstop</param>
    internal static Audit.Status ReportInvalidXsFacet(this ILogger? logger, BaseContext context, string alternative)
    {
        if (string.IsNullOrWhiteSpace(alternative))
            logger?.LogError("Invalid context for {tp} at line {line}, position {pos}.", context.type, context.StartLineNumber, context.StartLinePosition);
        else
            logger?.LogError("Invalid context for {tp} at line {line}, position {pos}; {alternative}.", context.type, context.StartLineNumber, context.StartLinePosition, alternative);

        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidStringMatcher(this ILogger? logger, BaseContext context, string field)
    {
        logger?.LogError("Invalid string matcher for `{field}` at element `{tp}`, line {line}, position {pos}.", field, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportNoStringMatcher(this ILogger? logger, BaseContext context, string field)
    {
        logger?.LogError("Empty string matcher for `{field}` on element `{tp}` at line {line}, position {pos}.", field, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportBadType(ILogger? logger, BaseContext context, string baseAsString)
    {
        logger?.LogError("Invalid type `{base}` on element `{tp}` at line {line}, position {pos}.", baseAsString, context.type, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportBadMatcher(ILogger? logger, BaseContext context, string expected)
    {
        logger?.LogError("Invalid type `{tp}` to match `{expected}` at line {line}, position {pos}.", context.type, expected, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }

    internal static Audit.Status ReportInvalidClassMatcher(BaseContext context, string value, ILogger? logger, string listToMatchName)
    {
        logger?.LogError("Invalid value `{val}` in {tp} to match `{expected}` at line {line}, position {pos}.", value, context.type, listToMatchName, context.StartLineNumber, context.StartLinePosition);
        return Audit.Status.IdsContentError;
    }
}
