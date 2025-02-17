using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Clasp.Data.AbstractSyntax;
using Clasp.Exceptions;
using Clasp.Interfaces;

namespace Clasp.ExtensionMethods
{
    public static class ExceptionExtensions
    {
        private static readonly string _stkRegex = string.Concat(
            @"(?:(?<namespace>[_a-zA-Z][_a-zA-Z0-9]*)\.)+",
            @"(?<fun>\.?[_a-zA-Z][_a-zA-Z0-9`]*)",
            @"\((?:",
                @"(?<type>[_a-zA-Z][_a-zA-Z0-9`\<\>]*(?:\[\]|\&)*)",
                @"\s",
                @"(?<param>[_a-zA-Z][_a-zA-Z0-9]*)",
                @"(?:\, )?",
            @")*\)",
            @"(?: in (?<path>.*?)\:line (?<line>\d+))?"
        );

        public static string GetSimpleStackTrace(this Exception ex)
        {
            if (string.IsNullOrEmpty(ex.StackTrace))
            {
                return string.Empty;
            }

            List<string> outputLines = new List<string>();

            MatchCollection matches = Regex.Matches(ex.StackTrace, _stkRegex);

            foreach (Match match in matches)
            {
                string[] nameSpace = match.Groups[1].Captures.Select(x => x.Value).ToArray();
                string methodName = match.Groups[2].Value;

                string[] argTypes = match.Groups[3].Captures.Select(x => x.Value).ToArray();
                string[] argNames = match.Groups[4].Captures.Select(x => x.Value).ToArray();

                string filePath = match.Groups[5].Value;
                int lineNumber = string.IsNullOrEmpty(filePath) ? -1 : int.Parse(match.Groups[6].Value);

                string atMethod = string.Format("{0}({1})", methodName, argTypes.Length > 0 ? "..." : string.Empty);
                string inFile = string.IsNullOrWhiteSpace(filePath)
                    ? string.Empty
                    : string.Format(" in {0}:{1}", System.IO.Path.GetFileName(filePath), lineNumber);

                outputLines.Add(string.Format("   at {0}{1}", atMethod, inFile));
            }

            return string.Join(System.Environment.NewLine, outputLines);
        }

        public static void PrintExceptionInfo(this Exception ex, StreamWriter sw)
        {
            if (ex.InnerException is Exception inner)
            {
                inner.PrintExceptionInfo(sw);
                sw.Write("└─> ");
            }

            sw.Write(ex switch
            {
                PiperException => "Piping error: ",
                LexerException => "Lexing error: ",
                ReaderException => "Reading error: ",
                ExpanderException => "Expansion error: ",
                ParserException => "Parsing error: ",
                InterpreterException => "Interpreter Runtime error: ",
                ProcessingException => "Processing error: ",
                ClaspException => "CLASP error: ",
                _ => string.Format("System-Level error ({0}): ", ex.GetType().Name)
            });

            sw.WriteLine(ex.Message);

            if (ex is InterpreterException ie && ie.ContinuationTrace.Count > 0)
            {
                sw.WriteLine();
                sw.WriteLine("Stack trace:");
                foreach (VmInstruction frame in ie.ContinuationTrace)
                {
                    sw.Write("   ");
                    sw.WriteLine(frame.ToString());
                }
                sw.WriteLine();
            }

            if (ex is ISourceTraceable ist)
            {
                sw.WriteLine();
                sw.WriteLine("At input source:");
                sw.WriteLine(Printer.PrintLineErrorHelper(ist));
            }

            sw.WriteLine("From CLASP source:");
            sw.WriteLine(ex.GetSimpleStackTrace());

            sw.WriteLine();
        }
    }
}
