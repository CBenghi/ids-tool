using CommandLine;
using IdsLib;
using System.Collections.Generic;
using System.IO;

namespace IdsTool
{
    [Verb("check", HelpText = "check files for issues.")]
    public class CheckOptions : ICheckOptions
    {
        /// <summary>
        /// XSD schema to load, this is useful when testing changes in the schema (e.g. GitHub repo).
        /// </summary>
        [Option('x', "xsd", Required = true, HelpText = "XSD schema(s) to load, this is useful when testing changes in the schema (e.g. GitHub repo).")]
        public IEnumerable<string> SchemaFiles { get; set; }

        /// <summary>
        /// Check validity of the xsd schema(s) passed with the <see cref="SchemaFiles"/> parameter.
        /// </summary>
        [Option('s', "schema", Default = false, Required = false, HelpText = "Check validity of the xsd schemas.")]
        public bool CheckSchemaDefinition { get; set; }

        [Option('e', "extension", Default = "ids", Required = false, HelpText = "When passing a folder, as source, this can be used to define files to load by their extension.")]
        public string InputExtension { get; set; }

        [Value(0,
            MetaName = "source",
            HelpText = "Input IDS to be processed; it can be a file or a folder.",
            Required = false)]
        public string InputSource { get; set; }
    }
}
