using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IdsLib.IdsSchema
{
    internal class BaseContext 
    {
        protected internal readonly string type;

        protected internal BaseContext? Parent { get; set; }

        internal int StartLineNumber { get; set; } = 0;
        internal int StartLinePosition { get; set; } = 0;

        public BaseContext(XmlReader reader)
        {
            type = reader.LocalName;
            var li = (IXmlLineInfo)reader;
            StartLineNumber = li.LineNumber;
            StartLinePosition = li.LinePosition;
        }

        internal protected virtual Check.Status Audit(ILogger? logger)
        {
            return IdsLib.Check.Status.Ok;
        }
    }
}
