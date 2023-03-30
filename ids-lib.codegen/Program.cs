namespace IdsLib.codegen;

internal class Program
{
    static void Main()
    {
        var destPath = new DirectoryInfo(@"..\..\..\..\");
        Console.WriteLine("Running code generation for ids-lib.");

        var tmp = IfcSchema_ClassAndAttributeNamesGenerator.Execute();
        var dest = Path.Combine(destPath.FullName, @"ids-lib\IfcSchema\SchemaInfo.ClassAndAttributeNames.g.cs");
        File.WriteAllText(dest, tmp);

        tmp = IfcSchema_ClassGenerator.Execute();
        dest = Path.Combine(destPath.FullName, @"ids-lib\IfcSchema\SchemaInfo.Schemas.g.cs");
        File.WriteAllText(dest, tmp);

        tmp = IfcSchema_AttributesGenerator.Execute();
        dest = Path.Combine(destPath.FullName, @"ids-lib\IfcSchema\SchemaInfo.Attributes.g.cs");
        File.WriteAllText(dest, tmp);
    }

    static internal string[] schemas = new[] { "Ifc2x3", "Ifc4", "Ifc4x3" };
}