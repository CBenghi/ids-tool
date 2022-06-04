using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

namespace IdsLib
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

		public static Status Run(CheckOptions opts, TextWriter writer = null)
		{
			if (writer == null)
				writer = Console.Out;
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
				List<string> checks = new List<string>();
				if (opts.InputSource != null)
					checks.Add("XML content");
				if (opts.CheckSchemaDefinition)
					checks.Add("Xsd schemas correctness");
				writer.WriteLine($"Checking: {string.Join(", ", checks.ToArray())}." );
			}

			// start checking
			if (opts.CheckSchemaDefinition)
			{
				retvalue |= opts.PerformSchemaCheck(opts, writer);
			}

			if (Directory.Exists(opts.InputSource))
			{
				writer.WriteLine("");
				var t = new DirectoryInfo(opts.InputSource);
				opts.ResolvedSource = t;
				var ret = ProcessExamplesFolder(t, new CheckInfo(opts,writer));
				writer.WriteLine($"\r\nCompleted with status: {ret}.");
				return ret;
			}
			if (File.Exists(opts.InputSource))
			{
				writer.WriteLine("");
				var t = new FileInfo(opts.InputSource);
				opts.ResolvedSource = t;
				var ret = ProcessSingleFile(t, new CheckInfo(opts,writer));
				writer.WriteLine($"\r\nCompleted with status: {ret}.");
				return ret;
			}
			writer.WriteError($"Error: Invalid input source '{opts.InputSource}'");
			return Status.NotFoundError;
		}

		

		private Status PerformSchemaCheck(CheckOptions c, TextWriter writer)
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
            var extra = new[] { "xsdschema.xsd", "xml.xsd" };
			var saved = new List<string>();

			// get the resources
			var assembly = Assembly.GetExecutingAssembly();
			foreach (var extraXsd in extra)
            {
				string resourceName = assembly.GetManifestResourceNames()
				.Single(str => str.EndsWith(extraXsd));
				using (Stream stream = assembly.GetManifestResourceStream(resourceName))
				using (StreamReader reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();
					var tempFile = Path.GetTempFileName();
					File.WriteAllText(tempFile, result);
					saved.Add(tempFile);
				}
			}
			
			foreach (var item in this.CheckSchema.Union(saved))
			{
				FileInfo f = new FileInfo(item);
				if (f.Exists)
					yield return f;
			}
			

		}

		private static Status ProcessExamplesFolder(DirectoryInfo directoryInfo, CheckInfo c)
		{
#if NETSTANDARD2_0
			var allBcfs = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories).ToList();
#else
			var eop = new EnumerationOptions() { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };
			var allBcfs = directoryInfo.GetFiles("*.xml", eop).ToList();
#endif
			Status ret = Status.Ok;
			foreach (var bcf in allBcfs.OrderBy(x => x.FullName))
			{
				ret |= ProcessSingleFile(bcf, c);
			}
			return ret;
		}

		private class CheckInfo
		{
			// todo: rather ugly to have this here... I'm designing classes as I go along
			public CheckOptions Options { get; }

			public CheckInfo(CheckOptions opts, TextWriter writer)
			{
				Options = opts;
				Writer = writer;
				if (Writer is StringWriter)
				{
					Verbose = true;
				}
			}

			public Dictionary<string, string> guids = new Dictionary<string, string>();

			// if you think `options` is ugly, this is jsut awful ;-)
			public string validatingFile { get; set; }

			public Status Status { get; internal set; }

			internal TextWriter Writer;

			internal readonly bool Verbose;

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
					Writer.WriteError($"XML WARNING",$"{validatingFile}\t{location}{e.Message}{newguid}");
					Status |= Status.ContentError;
				}
				else if (e.Severity == XmlSeverityType.Error)
				{
					Writer.WriteError($"XML ERROR", $"{validatingFile}\t{location}{e.Message}{newguid}");
					Status |= Status.ContentError;
				}
			}
		}

		private static Status ProcessSingleFile(FileInfo zippedFileInfo, CheckInfo c)
		{
			Status ret = Status.Ok;
			ret |= CheckSchemaCompliance(c, zippedFileInfo);
			return ret;
		}

		
		private static Status CheckSchemaCompliance(CheckInfo c, FileInfo theFile)
		{
			c.validatingFile = theFile.FullName;
			XmlReaderSettings rSettings = GetSchemaSettings(c);
			rSettings.ValidationType = ValidationType.Schema;
			
			rSettings.ValidationEventHandler += new ValidationEventHandler(c.validationReporter);
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
			XmlReaderSettings rSettings = new XmlReaderSettings();
			foreach (var schema in c.Options.GetSchemas())
			{
				var tns = GetSchemaNamespace(schema);
				rSettings.Schemas.Add(tns, schema.FullName);
			}

			return rSettings;
		}

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
	}
}
