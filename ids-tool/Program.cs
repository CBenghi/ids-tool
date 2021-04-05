using CommandLine;
using System;

namespace idsTool
{
	public partial class Program
	{
		static public int Main(string[] args)
		{
			var t = Parser.Default.ParseArguments<CheckOptions, ErrorCodeOptions>(args)
			  .MapResult(
				(CheckOptions opts) => CheckOptions.Run(opts),
				(ErrorCodeOptions opts) => ErrorCodeOptions.Run(opts),
				errs => Status.CommandLineError);
			return (int)t;
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
