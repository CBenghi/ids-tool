using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace IdsLib.Generator
{
    [Generator]
    public class SomeGen : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //// The following is an example of code to be added regeardless
            //context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            //    "SomeCode.g.cs",
            //    SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

            // CompilationProvider fires with all code changes, but we should be using caching (of current hour string) to make it run every hour
            //
            var oncePerHourEvent = context.CompilationProvider.Select((context,cancToken)=>DateTime.Now.Hour.ToString());          
            context.RegisterSourceOutput(oncePerHourEvent, OncePerHourAction);
        }

        Action<SourceProductionContext, string> OncePerHourAction = static (sourceProductionContext, filePaths) =>
        {
            sourceProductionContext.AddSource("additionalFiles.cs", @"
namespace Generated
{
    public class AdditionalTextList
    {
        public static void PrintTexts()
        {
            // " + DateTime.Now.ToLongTimeString() + @" "");
        }
    }
}");
        };

    }
}
