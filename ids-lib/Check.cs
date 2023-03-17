using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace IdsLib
{
    public static class Check
    {
        [Flags]
        public enum Status
        {
            Ok = 0,
            NotImplemented = 1,
            CommandLineError = 2,
            NotFoundError = 4,
            ContentError = 8,
            XsdSchemaError = 16,
        }

        public static Status Run(ICheckOptions opts, ILogger? logger = null)
        {
            Status retvalue = Status.Ok;

            // if no check is required than check default
            if (
                opts.InputSource == null
                )
            {
                opts.CheckSchemaDefinition = true;
                logger?.LogInformation("Performing default checks.");
            }
            else
            {
                var checkList = new List<string>();
                if (opts.InputSource != null)
                    checkList.Add("XML content");
                if (opts.CheckSchemaDefinition)
                    checkList.Add("Xsd schemas correctness");
                logger?.LogInformation("Checking: {checks}.", string.Join(", ", checkList.ToArray()));
            }

            // start checking
            if (opts.CheckSchemaDefinition)
            {
                retvalue |= PerformSchemaCheck(opts, logger);
                if (retvalue != Status.Ok)
                    return retvalue;
            }

            if (Directory.Exists(opts.InputSource))
            {
                var t = new DirectoryInfo(opts.InputSource);
                var ret = ProcessFolder(t, new CheckInfo(opts, logger), logger);
                return CompleteWith(ret, logger);
            }
            else if (File.Exists(opts.InputSource))
            {
                var t = new FileInfo(opts.InputSource);
                var ret = ProcessSingleFile(t, new CheckInfo(opts, logger), logger);
                return CompleteWith(ret, logger);
            }
            logger?.LogError("Error: Invalid input source '{missingSource}'", opts.InputSource);
            return Status.NotFoundError;
        }

        private static Status CompleteWith(Status ret, ILogger? writer)
        {
            writer?.LogInformation("Completed with status: {status}.", ret);
            return ret;
        }

        private class CheckInfo
        {
            public ICheckOptions Options { get; }

            public CheckInfo(ICheckOptions opts, ILogger? writer)
            {
                Options = opts;
                Writer = writer;
            }

            public string? ValidatingFile { get; set; }

            public Status Status { get; internal set; }

            internal ILogger? Writer;

            public void ValidationReporter(object? sender, ValidationEventArgs e)
            {
                var location = "";
                if (sender is IXmlLineInfo rdr)
                {
                    location = $"Line: {rdr.LineNumber}, Position: {rdr.LinePosition}, ";
                }
                if (e.Severity == XmlSeverityType.Warning)
                {
                    Writer?.LogWarning($"XML WARNING", $"{ValidatingFile}\t{location}{e.Message}");
                    Status |= Status.ContentError;
                }
                else if (e.Severity == XmlSeverityType.Error)
                {
                    Writer?.LogError($"XML ERROR", $"{ValidatingFile}\t{location}{e.Message}");
                    Status |= Status.ContentError;
                }
            }
        }


        private static Status CheckSchemaCompliance(CheckInfo c, FileInfo theFile, ILogger? logger)
        {
            c.ValidatingFile = theFile.FullName;
            XmlReaderSettings rSettings = GetSchemaSettings(c, logger);
            rSettings.ValidationType = ValidationType.Schema;

            rSettings.ValidationEventHandler += new ValidationEventHandler(c.ValidationReporter);
            using var src = File.OpenRead(theFile.FullName);
            XmlReader content = XmlReader.Create(src, rSettings);
            var cntRead = 0;
            while (content.Read())
            {
                cntRead++;
                // read all file to trigger validation events.
            }
            c.Writer?.LogDebug("Read {fullname}, {cntRead} elements.", theFile.FullName, cntRead);
            return c.Status;
        }

        private static XmlReaderSettings GetSchemaSettings(CheckInfo c, ILogger? logger)
        {
            var rSettings = new XmlReaderSettings();
            foreach (var schema in GetSchemas(c.Options, logger))
            {
                var tns = GetSchemaNamespace(schema);
                rSettings.Schemas.Add(tns, schema.FullName);
            }

            return rSettings;
        }

        static readonly Dictionary<string, string> NameSpaces = new();

        private static string GetSchemaNamespace(FileInfo schemaFile)
        {
            if (NameSpaces.ContainsKey(schemaFile.FullName))
                return NameSpaces[schemaFile.FullName];

            string tns = "";
            var re = new Regex(@"targetNamespace=""(?<tns>[^""]*)""");
            var t = File.ReadAllText(schemaFile.FullName);
            var m = re.Match(t);
            if (m.Success)
            {
                tns = m.Groups["tns"].Value;
            }
            NameSpaces.Add(schemaFile.FullName, tns);
            return tns;
        }


        private static Status ProcessSingleFile(FileInfo theFile, CheckInfo c, ILogger? logger)
        {
            Status ret = Status.Ok;
            ret |= CheckSchemaCompliance(c, theFile, logger);
            return ret;
        }


        private static Status ProcessFolder(DirectoryInfo directoryInfo, CheckInfo c, ILogger? logger)
        {
            string idsExtension = c.Options.InputExtension;
#if NETSTANDARD2_0
            var allBcfs = directoryInfo.GetFiles($"*.{idsExtension}", SearchOption.AllDirectories).ToList();
#else
            var eop = new EnumerationOptions() { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };
            var allBcfs = directoryInfo.GetFiles($"*.{idsExtension}", eop).ToList();
#endif
            Status ret = Status.Ok;
            var tally = 0;
            foreach (var bcf in allBcfs.OrderBy(x => x.FullName))
            {
                ret = ProcessSingleFile(bcf, c, logger) | ret;
                tally++;
            }
            var fileCardinality = tally != 1 ? "files" : "file";
            c.Writer?.LogInformation("{tally} {fileCardinality} processed.", tally, fileCardinality);
            return ret;
        }

        private static Status PerformSchemaCheck(ICheckOptions c, ILogger? logger)
        {
            Status ret = Status.Ok;
            var rSettings = new XmlReaderSettings();
            foreach (var schemaFile in GetSchemas(c, logger))
            {
                try
                {
                    var ns = GetSchemaNamespace(schemaFile);
                    rSettings.Schemas.Add(ns, schemaFile.FullName);
                }
                catch (XmlSchemaException ex)
                {
                    logger?.LogError("XSD\t{schemaFile}\tSchema error: {errMessage} at line {line}, position {pos}.", schemaFile, ex.Message, ex.LineNumber, ex.LinePosition);
                    ret |= Status.XsdSchemaError;
                }
                catch (Exception ex)
                {
                    logger?.LogError("XSD\t{schemaFile}\tSchema error: {errMessage}.", schemaFile, ex.Message);
                    ret |= Status.XsdSchemaError;
                }
            }
            return ret;
        }


        private static IEnumerable<FileInfo> GetSchemas(ICheckOptions opt, ILogger? logger)
        {
            var extra = new[] { "xsdschema.xsd", "xml.xsd" };
            var saved = new List<string>();

            // get the resources
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var resourceSchema in extra)
            {
                string resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(resourceSchema));
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    logger?.LogError("Error extracting resource: {schema}", resourceSchema);
                    continue;
                }
                using var reader = new StreamReader(stream);
                string result = reader.ReadToEnd();
                var tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, result);
                saved.Add(tempFile);
            }

            foreach (var item in opt.SchemaFiles.Union(saved))
            {
                var f = new FileInfo(item);
                if (f.Exists)
                    yield return f;
            }
        }

    }
}
