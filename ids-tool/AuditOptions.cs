﻿using CommandLine;
using IdsLib;
using System.Collections.Generic;
using System.Linq;

namespace IdsTool;

[Verb("audit", HelpText = "Audits ids files and/or their xsd schema.")]
public class AuditOptions : IAuditOptions
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Option('x', "xsd", Required = false, HelpText = "XSD schema(s) to load, this is useful when testing changes in the schema (e.g. GitHub repo). If this is not specified, an embedded schema is adopted depending on the each ids's declaration of version.")]
    public IEnumerable<string> SchemaFiles { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Option('s', "schema", Default = false, Required = false, HelpText = "Check validity of the xsd schema(s) passed with the `xsd` option. This is useful for the development of the schema and it is in use in the official repository for quality assurance purposes.")]
    public bool AuditSchemaDefinition { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Option('e', "extension", Default = "ids", Required = false, HelpText = "When passing a folder as source, this defines which files to audit by extension.")]
    public string InputExtension { get; set; } = "ids";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Value(0,
        MetaName = "source",
        HelpText = "Input IDS to be processed; it can be a file or a folder.",
        Required = false)]
    public string InputSource { get; set; } = string.Empty; 

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Option('c', "omitContent", Required = false, HelpText = "Skips the audit of the agreed limitation of IDS contents.")]
    public bool OmitIdsContentAudit { get; set; } 
}
