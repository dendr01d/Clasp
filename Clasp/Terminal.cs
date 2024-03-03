namespace Clasp
{
    public static class Terminal
    {
        public static void RunConsole()
        {
            Stream input = Console.OpenStandardInput();
            Stream output = Console.OpenStandardOutput();
            Run(input, output, output);
        }


        public static void Run(Stream inputStream, Stream outputStream, Stream errorStream)
        {
            StreamReader reader = new(inputStream);
            StreamWriter writer = new(outputStream);
            StreamWriter errors = new(errorStream);

            writer.AutoFlush = true;
            errors.AutoFlush = true;

            writer.WriteLine("CLASP Terminal");
            writer.WriteLine("\"quit\" to exit");

            string? input = string.Empty;

            Environment global = Environment.StdEnv(writer);

            try
            {
                ReadDefinitionsIntoEnvironment("StdDefs.lsp", global);
            }
            catch (AggregateException exs)
            {
                writer.WriteLine("\nError reading standard definitions:");

                foreach (Exception ex in exs.InnerExceptions)
                {
                    writer.WriteLine($"ERROR: {ex.Message}");
                }

                writer.Write("\n\nPress any key to exit...");
                reader.Read();
                return;
            }

            while (input != "quit")
            {
                writer.WriteLine();
                writer.Write("> ");
                input = reader.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    try
                    {
                        Token[] tokens = Lexer.Lex(input).ToArray();
                        writer.WriteLine($"--> {string.Join(", ", tokens.AsEnumerable())}");

                        Expression expr = Parser.Parse(input);
                        writer.WriteLine($"----> {expr}");

                        Expression result = expr.CallEval(global);
                        writer.WriteLine(result);
                    }
                    catch (Exception ex)
                    {
                        errors.WriteLine("ERROR: " + ex.Message);
                        errors.WriteLine(ex.StackTrace);
                    }
                }
            }

            return;
        }

        private static void ReadDefinitionsIntoEnvironment(string fileName, Environment env)
        {
            if (File.Exists(fileName))
            {

                List<Exception> errors = [];

                foreach (string line in File.ReadAllLines(fileName).Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    try
                    {
                        Parser.Parse(line).CallEval(env);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }
                }

                if (errors.Count > 0)
                {
                    throw new AggregateException(errors);
                }
            }
        }
    }
}
