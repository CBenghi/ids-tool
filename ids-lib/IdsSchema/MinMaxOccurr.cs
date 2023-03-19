using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IdsLib.IdsSchema
{
    internal class MinMaxOccurr
    {
        private readonly string minString;
        private readonly string maxString;

        public override string ToString()
        {
            return $"[{minString}..{maxString}]";
        }

        public MinMaxOccurr(XmlReader reader)
        {
            // both default to "1" when not specified.
            minString = reader.GetAttribute("minOccurs") ?? "1"; 
            maxString = reader.GetAttribute("maxOccurs") ?? "1"; 
        }

        internal Check.Status Audit()
        {
            try
            {
                var max = maxString switch
                {
                    "unbounded" => int.MaxValue,
                    _ => int.Parse(maxString)
                };
                if (!int.TryParse(minString, out var min))
                    return Check.Status.IdsContentError;
                return (min <= max)
                    ? Check.Status.Ok
                    : Check.Status.IdsContentError;
            }
            catch (Exception)
            {
                return Check.Status.IdsContentError;
            }
        }
    }
}
