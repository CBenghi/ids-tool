using System.Collections.Generic;

namespace IdsLib
{
    /// <summary>
    /// This interface contains the parameters to configure the execution of the cheks.
    /// </summary>
    public interface ICheckOptions
    {
        /// <summary>
        /// XSD schema to load, this is useful when testing changes in the schema (e.g. GitHub repo).
        /// </summary>
        IEnumerable<string> SchemaFiles { get; set; }
        /// <summary>
        /// Check validity of the xsd schema(s) passed with the <see cref="SchemaFiles"/> parameter.
        /// </summary>
        bool CheckSchemaDefinition { get; set; }
        /// <summary>
        /// This is used to define the extension to load when passing a folder in the <see cref="InputSource"/> parameter. In doubt set it to "ids".
        /// </summary>
        string InputExtension { get; set; }
        /// <summary>
        /// Input IDS to be processed; it can be a file or a folder.
        /// </summary>
        string InputSource { get; set; }
    }
}