using CommandLine;
using IdsLib;
using IdsTool;
using System;
using static IdsLib.Check;

namespace idsTool
{
    public partial class Program
	{
		static public int Main(string[] args)
		{
			var t = Parser.Default.ParseArguments<CheckOptions, ErrorCodeOptions>(args)
			  .MapResult(
				(CheckOptions opts) => Check.Run(opts, Console.Out),
				(ErrorCodeOptions opts) => ErrorCodeOptions.Run(opts),
				errs => Status.CommandLineError);
			return (int)t;
		}


	}
}
