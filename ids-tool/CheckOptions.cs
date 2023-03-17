using CommandLine;
using IdsLib;
using System.Collections.Generic;
using System.Linq;

namespace IdsTool
{
    [Verb("check", HelpText = "check files for issues.")]
    public class CheckOptions : ICheckOptions
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [Option('x', "xsd", Required = false, HelpText = "XSD schema(s) to load, this is useful when testing changes in the schema (e.g. GitHub repo).")]
        public IEnumerable<string> SchemaFiles { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [Option('s', "schema", Default = false, Required = false, HelpText = "Check validity of the xsd schemas.")]
        public bool CheckSchemaDefinition { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [Option('e', "extension", Default = "ids", Required = false, HelpText = "When passing a folder, as source, this can be used to define files to load by their extension.")]
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
        [Option('c', "omitContent", Required = false, HelpText = "Skips the check of the semantic aspects of the IDS.")]
        public bool OmitIdsContentCheck { get; set; } 
    }
}
