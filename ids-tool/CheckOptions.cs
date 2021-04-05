using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using static idsTool.Program;

namespace idsTool
{

	[Verb("check", HelpText = "check files for issues.")]
	public class CheckOptions
	{
		[Option('x', "xsd", Required = true, HelpText = "XSD sources.", Default = false)]
		public IEnumerable<string> CheckSchema { get; set; }

		[Option('s', "schema", Default = false, Required = false, HelpText = "Check validity of the xsd schemas.")]
		public bool CheckSchemaDefinition { get; set; }

		[Value(0, 
			MetaName = "source",
			HelpText = "Input source to be processed; it can be a file or a folder.",
			Required = false)]
		public string InputSource { get; set; }

		public FileSystemInfo ResolvedSource { get; set; }

		public static Status Run(CheckOptions opts)
		{
			Console.WriteLine("=== ids-tool - checking IDS files.");

			Status retvalue = Status.Ok;

			// if no check is required than check default
			if (
				opts.InputSource == null
				)
			{
				opts.CheckSchemaDefinition = true;
				Console.WriteLine("Performing default checks.");
			}
			else
			{
				List<string> checks = new List<string>();
				if (opts.InputSource != null)
					checks.Add("XML content");
				if (opts.CheckSchemaDefinition)
					checks.Add("Xsd schemas correctness");
				Console.WriteLine($"Checking: {string.Join(", ", checks.ToArray())}." );
			}

			// start checking
			if (opts.CheckSchemaDefinition)
			{
				retvalue |= opts.PerformSchemaCheck(opts);
			}

			if (Directory.Exists(opts.InputSource))
			{
				Console.WriteLine("");
				var t = new DirectoryInfo(opts.InputSource);
				opts.ResolvedSource = t;
				var ret = ProcessExamplesFolder(t, new CheckInfo(opts));
				Console.WriteLine($"\r\nCompleted with status: {ret}.");
				return ret;
			}
			if (File.Exists(opts.InputSource))
			{
				Console.WriteLine("");
				var t = new FileInfo(opts.InputSource);
				opts.ResolvedSource = t;
				var ret = ProcessSingleFile(t, new CheckInfo(opts));
				Console.WriteLine($"\r\nCompleted with status: {ret}.");
				return ret;
			}
			Console.WriteLine($"Error: Invalid input source '{opts.InputSource}'");
			return Status.NotFoundError;
		}

		

		private Status PerformSchemaCheck(CheckOptions c)
		{
			Status ret = Status.Ok;
			XmlReaderSettings rSettings = new XmlReaderSettings();
			foreach (var schemaFile in c.GetSchemas())
			{
				try
				{
					var ns = GetSchemaNamespace(schemaFile);
					rSettings.Schemas.Add(ns, schemaFile.FullName);
				}
				catch (XmlSchemaException ex)
				{
					Console.WriteLine($"XSD\t{schemaFile}\tSchema error: {ex.Message} at line {ex.LineNumber}, position {ex.LinePosition}.");
					ret |= Status.XsdSchemaError;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"XSD\t{schemaFile}\tSchema error: {ex.Message}.");
					ret |= Status.XsdSchemaError;
				}
			}
			return ret;
		}

		static Dictionary<string, string> NameSpaces = new Dictionary<string, string>();

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

		private IEnumerable<FileInfo> GetSchemas()
		{
			foreach (var item in this.CheckSchema)
			{
				FileInfo f = new FileInfo(item);
				if (f.Exists)
					yield return f;
			}
		}

		private static Status ProcessExamplesFolder(DirectoryInfo directoryInfo, CheckInfo c)
		{
			var eop = new EnumerationOptions() { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };
			var allBcfs = directoryInfo.GetFiles("*.xml", eop).ToList();
			foreach (var bcf in allBcfs.OrderBy(x => x.FullName))
			{
				ProcessSingleFile(bcf, c);
			}
			return c.Status;
		}

		

		private static DirectoryInfo GetRepoSchemasFolder(DirectoryInfo directoryInfo)
		{
			// the assumption is that the directory is either the entire repo, 
			// or one of the test cases
			//
			var enumOptions = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true };
			var tmp = directoryInfo.GetDirectories("schemas", enumOptions).FirstOrDefault();
			if (tmp != null)
			{
				return tmp;
			}
			var searchPath = "Test Cases";
			var pos = directoryInfo.FullName.IndexOf(searchPath, StringComparison.InvariantCultureIgnoreCase);
			if (pos != -1)
			{
				var testcasefolder = directoryInfo.FullName.Substring(0, pos + searchPath.Length);
				var tdDirInfo = new DirectoryInfo(testcasefolder);
				// the parent of the test cases is the main repo folder
				if (!tdDirInfo.Exists) // this shoud always be the case
					return null;
				return tdDirInfo.Parent.GetDirectories("schemas", enumOptions).FirstOrDefault();
			}
			return null;
		}

		private class CheckInfo
		{
			// todo: rather ugly to have this here... I'm designing classes as I go along
			public CheckOptions Options { get; }

			public CheckInfo(CheckOptions opts)
			{
				Options = opts;
			}

			public Dictionary<string, string> guids = new Dictionary<string, string>();

			// if you think `options` is ugly, this is jsut awful ;-)
			public string validatingFile { get; set; }

			public Status Status { get; internal set; }
			
			public void validationReporter(object sender, ValidationEventArgs e)
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
					Console.WriteLine($"XML WARNING\t{validatingFile}\t{location}{e.Message}{newguid}");
					Status |= Status.ContentError;
				}
				else if (e.Severity == XmlSeverityType.Error)
				{
					Console.WriteLine($"XML ERROR\t{validatingFile}\t{location}{e.Message}{newguid}");
					Status |= Status.ContentError;
				}
			}

			public string CleanName(FileSystemInfo f)
			{
				return Path.GetRelativePath(Options.ResolvedSource.FullName, f.FullName);
			}
		}

		private static Status ProcessSingleFile(FileInfo zippedFileInfo, CheckInfo c)
		{
			Status ret = Status.Ok;
			ret |= CheckSchemaCompliance(c, zippedFileInfo);
			return Status.Ok;
		}

		
		private static Status CheckSchemaCompliance(CheckInfo c, FileInfo unzippedDir)
		{
			c.validatingFile = unzippedDir.FullName;
			XmlReaderSettings rSettings = GetSchemaSettings(c);
			rSettings.ValidationType = ValidationType.Schema;
			rSettings.ValidationEventHandler += new ValidationEventHandler(c.validationReporter);
			XmlReader content = XmlReader.Create(File.OpenRead(unzippedDir.FullName), rSettings);
			while (content.Read())
			{
				// read all files to trigger validation events.
			}
			return c.Status;
		}

		private static XmlReaderSettings GetSchemaSettings(CheckInfo c)
		{
			XmlReaderSettings rSettings = new XmlReaderSettings();
			foreach (var schema in c.Options.GetSchemas())
			{
				var tns = GetSchemaNamespace(schema);
				rSettings.Schemas.Add(tns, schema.FullName);
			}

			return rSettings;
		}
	}
}
