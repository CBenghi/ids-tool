﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using IdsLib.IdsSchema;
using System.Diagnostics;

namespace IdsLib
{
    public static partial class Check
    {
        [Flags]
        public enum Status
        {
            Ok = 0,
            NotImplementedError = 1 << 0,
            InvalidOptionsError = 1 << 1,
            NotFoundError = 1 << 2,
            IdsStructureError = 1 << 3,
            IdsContentError = 1 << 4,
            XsdSchemaError = 1 << 5,
        }

        public static Status Run(ICheckOptions opts, ILogger? logger = null)
        {
            Status retvalue = Status.Ok;
            if (string.IsNullOrEmpty(opts.InputSource) && !opts.SchemaFiles.Any())
            {
                // no IDS and no schema => nothing to do
                logger?.LogWarning("Nothing to check.");
                retvalue = Status.InvalidOptionsError;
            }
            else if (string.IsNullOrEmpty(opts.InputSource)) 
            {
                // No ids, but we have a schemafile => check the schema itself
                opts.CheckSchemaDefinition = true;
            }
            else
            {
                // just inform on the config
                var checkList = new List<string>();
                if (!string.IsNullOrEmpty(opts.InputSource))
                    checkList.Add("Ids structure");
                if (opts.CheckSchemaDefinition)
                    checkList.Add("Xsd schemas correctness");
                if (!checkList.Any())
                {
                    logger?.LogError("Invalid options.");
                    return Status.InvalidOptionsError;
                }
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
            logger?.LogError("Invalid input source '{missingSource}'", opts.InputSource);
            return Status.NotFoundError;
        }

        private static Status CompleteWith(Status ret, ILogger? writer)
        {
            writer?.LogInformation("Completed with status: {status}.", ret);
            return ret;
        }

        private async static Task<Status> CheckIdsComplianceAsync(CheckInfo c, FileInfo theFile, ILogger? logger)
        {
            c.ValidatingFile = theFile.FullName;

            XmlReaderSettings rSettings;
            if (c.Options.SchemaFiles.Any())
            {
                // we load the schema settings from the configuration options
                rSettings = GetSchemaSettings(c.Options.SchemaFiles, logger);
            }
            else
            {
                // we load the schema settings from the file
                var info = IdsXmlHelpers.GetIdsInformationAsync(theFile).Result;
                if (info.Version == IdsVersion.Invalid)
                {
                    logger?.LogError("IDS schema version not found, or not recognised ({vrs}).", info.SchemaLocation);
                    return Status.IdsStructureError;
                }
                var sett = GetSchemaSettings(info.Version, logger);
                if (sett is null)
                {
                    logger?.LogError("Embedded schema not found for IDS version {vrs}.", info.Version);
                    return Status.NotImplementedError;
                }
                rSettings = sett;
            }
            rSettings.ValidationType = ValidationType.Schema;
            rSettings.Async = true;
            rSettings.ValidationEventHandler += new ValidationEventHandler(c.ValidationReporter);
            rSettings.IgnoreComments = true;
            rSettings.IgnoreWhitespace = true;

            using var src = File.OpenRead(theFile.FullName);
            var reader = XmlReader.Create(src, rSettings);
            var cntRead = 0;
            var elementsStack = new Stack<BaseContext>(); // we prepare the stack to evaluate the IDS content
            BaseContext? current = null;
            Status contentStatus = Status.Ok;
            while (await reader.ReadAsync()) // the loop reads the entire file to trigger validation events.
            {
                cntRead++;
                if (!c.Options.OmitIdsContentCheck) // content checks can be omitted, but the while loop is still executed
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            Debug.WriteLine($"Start Element {reader.LocalName}");
                            var newContext = IdsXmlHelpers.GetContextFromElement(reader, logger); // this is always not null
                            newContext.Parent = current;
                            // we only push on the stack if it's not empty, e.g.: <some />
                            if (!reader.IsEmptyElement)
                                elementsStack.Push(newContext);
                            else
                                contentStatus |= newContext.Audit(logger); // invoking audit empty element
                            current = newContext; 
                            break;
                        case XmlNodeType.Attribute:
                            Debug.WriteLine($"Attribute Node: {reader.GetValueAsync().Result}");
                            break;
                        case XmlNodeType.Text:
                            Debug.WriteLine($"  Text Node: {reader.GetValueAsync().Result}");
                            break;
                        case XmlNodeType.EndElement:
                            Debug.WriteLine($"End Element {reader.LocalName}");
                            var closing = elementsStack.Pop();
                            Debug.WriteLine($"  auditing {closing.type} on end element");
                            contentStatus |= closing.Audit(logger); // invoking audit on end of element
                            break;
                        default:
                            Debug.WriteLine("Other node {0} with value '{1}'.", reader.NodeType, reader.Value);
                            break;
                    }
                }
            }
            c.Logger?.LogDebug("Read {fullname}, {cntRead} elements.", theFile.FullName, cntRead);
            return c.Status | contentStatus;
        }

        

        private static XmlReaderSettings? GetSchemaSettings(IdsVersion vrs, ILogger? logger)
        {
            var rSettings = new XmlReaderSettings();
            var files = GetSchemaFiles(vrs, logger);
            if (!files.Any())
                return null;
            foreach (var schema in files) // from GetSchemaSettings
            {
                var tns = GetSchemaNamespace(schema);
                rSettings.Schemas.Add(tns, schema.FullName);
            }
            return rSettings;
        }

        

        private static XmlReaderSettings GetSchemaSettings(IEnumerable<string> diskSchemas, ILogger? logger)
        {
            var rSettings = new XmlReaderSettings();
            foreach (var schema in GetSchemaFiles(diskSchemas, logger)) // from GetSchemaSettings
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
            ret |= CheckIdsComplianceAsync(c, theFile, logger).Result;
            return ret;
        }

        private static Status ProcessFolder(DirectoryInfo directoryInfo, CheckInfo c, ILogger? logger)
        {
            string idsExtension = c.Options.InputExtension;
#if NETSTANDARD2_0
            var allIdss = directoryInfo.GetFiles($"*.{idsExtension}", SearchOption.AllDirectories).ToList();
#else
            var eop = new EnumerationOptions() { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };
            var allIdss = directoryInfo.GetFiles($"*.{idsExtension}", eop).ToList();
#endif
            Status ret = Status.Ok;
            var tally = 0;
            foreach (var ids in allIdss.OrderBy(x => x.FullName))
            {
                logger?.LogInformation("Checking file: `{filename}`.", ids.FullName);
                var sgl = ProcessSingleFile(ids, c, logger);
                ret |= sgl;
                tally++;
            }
            var fileCardinality = tally != 1 ? "files" : "file";
            c.Logger?.LogInformation("{tally} {fileCardinality} processed.", tally, fileCardinality);
            return ret;
        }

        private static Status PerformSchemaCheck(ICheckOptions checkOptions, ILogger? logger)
        {
            Status ret = Status.Ok;
            var rSettings = new XmlReaderSettings();
            foreach (var schemaFile in GetSchemaFiles(checkOptions.SchemaFiles, logger)) // within PerformSchemaCheck
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

        private static IEnumerable<FileInfo> GetSchemaFiles(IdsVersion vrs, ILogger? logger)
        {
            List<string> resourceList;
            switch (vrs)
            {
                case IdsVersion.Ids0_9:
                    resourceList = new List<string> { "xsdschema.xsd", "xml.xsd", "ids.xsd" };
                    break;
                default:
                    logger?.LogError("Embedded schema for version {vrs} not implemented.", vrs);
                    yield break;
            }
            List<string> saved = ExtractResources(resourceList, logger);
            foreach (var item in saved)
            {
                var f = new FileInfo(item);
                if (f.Exists)
                    yield return f;
            }
        }

        private static IEnumerable<FileInfo> GetSchemaFiles(IEnumerable<string> diskFiles, ILogger? logger)
        {
            var resourceList = new List<string> { "xsdschema.xsd", "xml.xsd" };
            List<string> saved = ExtractResources(resourceList, logger);
            foreach (var item in diskFiles.Union(saved))
            {
                var f = new FileInfo(item);
                if (f.Exists)
                    yield return f;
            }
        }

        private static List<string> ExtractResources(IEnumerable<string> resources, ILogger? logger)
        {
            // get the resources
            var saved = new List<string>();
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var resourceSchema in resources)
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

            return saved;
        }
    }
}
