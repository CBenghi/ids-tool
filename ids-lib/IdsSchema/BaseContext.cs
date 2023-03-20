﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IdsLib.IdsSchema
{
    [DebuggerDisplay("{type} (Line {StartLineNumber}, Pos {StartLinePosition})")]
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

        /// <summary>
        /// The Audit method of the base context always succeeds
        /// </summary>
        /// <param name="logger">unused in the base class</param>
        /// <returns><see cref="IdsLib.Audit.Status.Ok"/> in all circumstances, only overridden implementation determine failure behaviours</returns>
        internal protected virtual Audit.Status Audit(ILogger? logger)
        {
            return IdsLib.Audit.Status.Ok;
        }

        internal protected virtual void SetContent(string contentString)
        {
            // nothing to do for the base entity
        }

        protected static bool TryGetNodes(BaseContext start, ref List<BaseContext> nodes, params string[] typeNames)
        {
            if (start.Parent is null)
                return false;
            if (start.Parent.type == typeNames[0])
            {
                // found
                nodes.Add(start.Parent);
                if (typeNames.Length > 1) // more to find
                    return TryGetNodes(start.Parent, ref nodes, typeNames.Skip(1).ToArray());
                return true; // all found
            }
            // not found, search on the parent, instead
            return TryGetNodes(start.Parent, ref nodes, typeNames);
        }

        protected static bool TryGetNodes(BaseContext start, string[] typeNames, out List<BaseContext> nodes)
        {
            var span = new ReadOnlySpan<string>(typeNames);
            nodes = new List<BaseContext>();
            return TryGetNodes(start, ref nodes, span);
        }

        protected static bool TryGetNodes(BaseContext start, ref List<BaseContext> nodes, ReadOnlySpan<string> typeNames)
        {
            if (start.Parent is null)
                return false;
            if (start.Parent.type == typeNames[0])
            {
                // found
                nodes.Add(start.Parent);
                if (typeNames.Length > 1) // more to search
                    return TryGetNodes(start.Parent, ref nodes, typeNames.Slice(1));
                return true; // all found
            }
            // not found, search on the parent, instead
            return TryGetNodes(start.Parent, ref nodes, typeNames);
        }
    }
}
