using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib
{
	public static class TextWriterExtensions
	{
        private static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out)
                return !Console.IsOutputRedirected;

            if (writer == Console.Error)
                return !Console.IsErrorRedirected && !Console.IsOutputRedirected; // Color codes are always output to Console.Out
            return false;
        }

        private static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsole())
                Console.ForegroundColor = color;
        }

        private static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsole())
                Console.ResetColor();
        }

        public static void WriteError(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkRed);
            writer.WriteLine(text);
            writer.ResetColor();
        }

        public static void WriteError(this TextWriter writer, string errorclass, string text)
        {
            writer.SetForeground(ConsoleColor.DarkRed);
            writer.Write(errorclass);
            writer.ResetColor();
            writer.Write($"\t{text}\r\n");
        }
    }
}
