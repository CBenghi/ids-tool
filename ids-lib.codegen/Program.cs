using IdsLib.Generator;

namespace ids_lib_codegen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var destPath = new DirectoryInfo(@"..\..\..\..\");
            Console.WriteLine("Running code generation for ids-lib.");
            var tmp = IfcSchemaGenerator.Execute();
            var dest = Path.Combine(destPath.FullName, @"ids-lib\IfcSchema\SchemaInfo.g.cs");
            File.WriteAllText(dest, tmp);
        }
    }
}