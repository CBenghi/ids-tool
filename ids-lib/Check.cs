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

        public static Status Run(ICheckOptions opts, TextWriter writer = null)
        {
            writer ??= Console.Out;
            writer.WriteLine("=== ids-tool - checking IDS files.");

            Status retvalue = Status.Ok;

            // if no check is required than check default
            if (
                opts.InputSource == null
                )
            {
                opts.CheckSchemaDefinition = true;
                writer.WriteLine("Performing default checks.");
            }
            else
            {
                var checks = new List<string>();
                if (opts.InputSource != null)
                    checks.Add("XML content");
                if (opts.CheckSchemaDefinition)
                    checks.Add("Xsd schemas correctness");
                writer.WriteLine($"Checking: {string.Join(", ", checks.ToArray())}.");
            }

            // start checking
            if (opts.CheckSchemaDefinition)
            {
                retvalue |= PerformSchemaCheck(opts, writer);
                if (retvalue != Status.Ok)
                    return retvalue;
            }

            if (Directory.Exists(opts.InputSource))
            {
                writer.WriteLine("");
                var t = new DirectoryInfo(opts.InputSource);
                var ret = ProcessFolder(t, new CheckInfo(opts, writer));
                writer.WriteLine($"\r\nCompleted with status: {ret}.");
                return CompleteWith(ret, writer);
            }
            else if (File.Exists(opts.InputSource))
            {
                writer.WriteLine("");
                var t = new FileInfo(opts.InputSource);               
                var ret = ProcessSingleFile(t, new CheckInfo(opts, writer));
                return CompleteWith(ret, writer);
            }
            writer.WriteError($"Error: Invalid input source '{opts.InputSource}'");
            return Status.NotFoundError;
        }


        private static Status CompleteWith(Status ret, TextWriter writer)
        {
            writer.WriteLine($"\r\nCompleted with status: {ret}.");
            return ret;
        }


        private class CheckInfo
        {
            // todo: rather ugly to have this here... I'm designing classes as I go along
            public ICheckOptions Options { get; }

            public CheckInfo(ICheckOptions opts, TextWriter writer)
            {
                Options = opts;
                Writer = writer;
                if (Writer is StringWriter)
                {
                    Verbose = true;
                }
            }

            public Dictionary<string, string> guids = new();

            // if you think `options` is ugly, this is jsut awful ;-)
            public string ValidatingFile { get; set; }

            public Status Status { get; internal set; }

            internal TextWriter Writer;

            internal readonly bool Verbose;

            public void ValidationReporter(object sender, ValidationEventArgs e)
            {
                var location = "";
                var newguid = "";
                if (e.Message.Contains("'Guid' is missing"))
                    newguid = $"You can use: '{Guid.NewGuid()}' instead.";
                if (sender is IXmlLineInfo rdr)
                {
                    location = $"Line: {rdr.LineNumber}, Position: {rdr.LinePosition}, ";
                }
                if (e.Severity == XmlSeverityType.Warning)
                {
                    Writer.WriteError($"XML WARNING", $"{ValidatingFile}\t{location}{e.Message}{newguid}");
                    Status |= Status.ContentError;
                }
                else if (e.Severity == XmlSeverityType.Error)
                {
                    Writer.WriteError($"XML ERROR", $"{ValidatingFile}\t{location}{e.Message}{newguid}");
                    Status |= Status.ContentError;
                }
            }
        }


        private static Status CheckSchemaCompliance(CheckInfo c, FileInfo theFile)
        {
            c.ValidatingFile = theFile.FullName;
            XmlReaderSettings rSettings = GetSchemaSettings(c);
            rSettings.ValidationType = ValidationType.Schema;

            rSettings.ValidationEventHandler += new ValidationEventHandler(c.ValidationReporter);
            using var src = File.OpenRead(theFile.FullName);
            XmlReader content = XmlReader.Create(src, rSettings);
            var cntRead = 0;
            while (content.Read())
            {
                cntRead++;
                // read all files to trigger validation events.
            }
            if (c.Verbose)
                c.Writer.WriteLine($"Read {theFile.FullName}, {cntRead} elements.");
            return c.Status;
        }

        private static XmlReaderSettings GetSchemaSettings(CheckInfo c)
        {
            var rSettings = new XmlReaderSettings();
            foreach (var schema in GetSchemas(c.Options))
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


        private static Status ProcessSingleFile(FileInfo theFile, CheckInfo c)
        {
            Status ret = Status.Ok;
            ret |= CheckSchemaCompliance(c, theFile);
            return ret;
        }


        private static Status ProcessFolder(DirectoryInfo directoryInfo, CheckInfo c)
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
                ret = ProcessSingleFile(bcf, c) | ret;
                tally++;
            }
            var fileCardinality = tally != 1 ? "files" : "file";
            c.Writer.Write($"{tally} {fileCardinality} processed.");
            return ret;
        }

        private static Status PerformSchemaCheck(ICheckOptions c, TextWriter writer)
        {
            Status ret = Status.Ok;
            var rSettings = new XmlReaderSettings();
            foreach (var schemaFile in GetSchemas(c))
            {
                try
                {
                    var ns = GetSchemaNamespace(schemaFile);
                    rSettings.Schemas.Add(ns, schemaFile.FullName);
                }
                catch (XmlSchemaException ex)
                {
                    writer.WriteLine($"XSD\t{schemaFile}\tSchema error: {ex.Message} at line {ex.LineNumber}, position {ex.LinePosition}.");
                    ret |= Status.XsdSchemaError;
                }
                catch (Exception ex)
                {
                    writer.WriteLine($"XSD\t{schemaFile}\tSchema error: {ex.Message}.");
                    ret |= Status.XsdSchemaError;
                }
            }
            return ret;
        }


        private static IEnumerable<FileInfo> GetSchemas(ICheckOptions opt)
        {
            var extra = new[] { "xsdschema.xsd", "xml.xsd" };
            var saved = new List<string>();

            // get the resources
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var extraXsd in extra)
            {
                string resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(extraXsd));
                using var stream = assembly.GetManifestResourceStream(resourceName);
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
