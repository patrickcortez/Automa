using Automa.Source.Core.IO;
using Automa.Source.Definitions;
using System.Text.RegularExpressions;

namespace Automa.Source.Utility
{
    internal static class Utils
    {


        public static void Print(string Title, object Content, PrintConfiguration? config = null)
        {
            string newContent = string.Empty;

            if (Content is string)
            {
                newContent = (string)Content;
            }

            if (config is not null && config.option == PrintOptions.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (config.newline)
                {
                    Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }

                Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                Console.ForegroundColor = ConsoleColor.White;
                return;
            } else if (config is not null && config.option == PrintOptions.Warning)
            {

                Console.ForegroundColor = ConsoleColor.Yellow;
                if (config.newline)
                {
                    Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }

                Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                Console.ForegroundColor = ConsoleColor.White;
                return;

            } else if (config is null || config.option == PrintOptions.Normal)
            {
                Console.ForegroundColor = ConsoleColor.White;
                if (config is null || config.newline)
                {
                    Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }

                Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }
        }

        public static void Print(string Content, PrintConfiguration? config = null)
        {


            if (config is not null && config.option == PrintOptions.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (config.newline)
                {
                    Console.Error.WriteLine(Content);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }

                Console.Error.WriteLine(Content);
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }
            else if (config is not null && config.option == PrintOptions.Warning)
            {

                Console.ForegroundColor = ConsoleColor.Yellow;
                if (config.newline)
                {
                    Console.Error.WriteLine(Content);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }

                Console.Error.WriteLine(Content);
                Console.ForegroundColor = ConsoleColor.White;
                return;

            }
            else if (config is null || config.option == PrintOptions.Normal)
            {
                Console.ForegroundColor = ConsoleColor.White;
                if (config is null || config.newline)
                {
                    Console.Error.WriteLine(Content);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }

                Console.Error.WriteLine(Content);
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }
        }


        public static Variable? FindVariable(string name,List<Variable> Variables)
        {
            foreach(var variable in Variables)
            {
                if(variable.name == name)
                {
                    return variable;
                }
            }

            return null;
        }

        public static string ExpandVariables(string Line,List<Variable> Variables) // expand any variable and escape codes in a string using Regex
        { // formerly a simple Replace()

            //  Console.WriteLine("Debug: Current line being Expanded: {0}", Line);


            string expanded = Regex.Replace(Line, @"\$([a-zA-Z_][a-zA-Z0-9_]*)", match => // expand all variable calls statring with: $, including numbers
            {
                string varname = match.Groups[1].Value;

                Variable? variable = Variables.Find(ex => ex.name == varname);

                if (variable != null)
                {
                    return variable.value.Eval().ToString()!;
                }

                return match.Value;
            });

            string formatted = Regex.Unescape(expanded); // format all the escape codes

            return formatted;

        }

        public static string? Input(string Prompt) // grab user input during execution
        {
            Console.WriteLine(Prompt);
            return InputHandler.ReadLine();
        }

    }
}
