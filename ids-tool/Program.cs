using CommandLine;
using IdsLib;
using IdsTool;
using Microsoft.Extensions.Logging;
using System;

namespace idsTool
{
    public partial class Program
    {
        static public int Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("LoggingConsoleApp.Program", LogLevel.Information)
                .AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                });
            });
            var writer = Console.Out;
            writer.WriteLine("=== ids-tool - checking IDS files.");
            ILogger logger = loggerFactory.CreateLogger<Program>();
            var t = Parser.Default.ParseArguments<CheckOptions, ErrorCodeOptions>(args)
              .MapResult(
                (CheckOptions opts) => Check.Run(opts, logger),
                (ErrorCodeOptions opts) => ErrorCodeOptions.Run(opts),
                errs => Check.Status.CommandLineError);
            return (int)t;
        }


    }
}
