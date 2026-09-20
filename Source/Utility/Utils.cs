using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

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



        public static string ExpandVariables(string Line,List<Variable> Variables) // expand any variable in a string
        {

          //  Console.WriteLine("Debug: Current line being Expanded: {0}", Line);

            foreach(Variable var in Variables)
            {
                string Current = "$" + var.name;

                if (Line.Contains(Current))
                {
                    Line = Line.Replace(Current, var.value);
                }
            }

            return Line;
        }

        public static string? Input(string Prompt) // grab user input during execution
        {
            Console.WriteLine(Prompt);
            return Console.ReadLine();
        }

    }
}
