using System;
using System.Collections.Generic;
using System.Text;

namespace IdsLib.Generator
{
    internal class SourceGenerationHelper
    {
        public const string Attribute = @"
namespace Xbim.Generated
{
    [System.AttributeUsage(System.AttributeTargets.Enum)]
    public class DoesItWork : System.Attribute
    {
    }
}";
    }
}
