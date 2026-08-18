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
                    Console.ForegroundColor = default;
                    return;
                }

                Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                Console.ForegroundColor = default;
                return;
            } else if (config is not null && config.option == PrintOptions.Warning)
            {

                Console.ForegroundColor = ConsoleColor.Yellow;
                if (config.newline)
                {
                    Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                    Console.ForegroundColor = default;
                    return;
                }

                Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                Console.ForegroundColor = default;
                return;

            } else if (config is null || config.option == PrintOptions.Normal)
            {
                Console.ForegroundColor = ConsoleColor.White;
                if (config is null || config.newline)
                {
                    Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                    Console.ForegroundColor = default;
                    return;
                }

                Console.Error.WriteLine("{0}\n---\n{1}", Title, (newContent != String.Empty) ? newContent : Content);
                Console.ForegroundColor = default;
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
                    Console.ForegroundColor = default;
                    return;
                }

                Console.Error.WriteLine(Content);
                Console.ForegroundColor = default;
                return;
            }
            else if (config is not null && config.option == PrintOptions.Warning)
            {

                Console.ForegroundColor = ConsoleColor.Yellow;
                if (config.newline)
                {
                    Console.Error.WriteLine(Content);
                    Console.ForegroundColor = default;
                    return;
                }

                Console.Error.WriteLine(Content);
                Console.ForegroundColor = default;
                return;

            }
            else if (config is null || config.option == PrintOptions.Normal)
            {
                Console.ForegroundColor = ConsoleColor.White;
                if (config is null || config.newline)
                {
                    Console.Error.WriteLine(Content);
                    Console.ForegroundColor = default;
                    return;
                }

                Console.Error.WriteLine(Content);
                Console.ForegroundColor = default;
                return;
            }
        }


        public static string CleanString(string Line)
        {
            StringBuilder Cleansed = new();
            bool inQoutes = false;

            int StartIndex = Line.IndexOf('(');
           // Console.WriteLine("Start of Cleaning at index {0} and the length of line is {1}",StartIndex,Line.Length);

            while (StartIndex < Line.Length)
            {
                char Current = Line[StartIndex]; // wtf
                

                if (Current == '(' || Current == ')' && !inQoutes)
                {
                    if (StartIndex + 1 < Line.Length) // bounds check
                    {

                        char next = Line[StartIndex + 1];

                        if (next == ')')
                        {
                            if (char.IsWhiteSpace(Current) && !inQoutes)
                            {
                                continue;
                            }

                            if (Current == '"')
                            {
                                inQoutes = !inQoutes;
                                continue;
                            }

                            Cleansed.Append(Current);
                            break;
                        }
                    }
                    StartIndex++;
                    continue;
                }

                if (char.IsWhiteSpace(Current) && !inQoutes)
                {
                    StartIndex++;
                    continue;
                }

                if (Current == '"')
                {
                    inQoutes = !inQoutes;
                    StartIndex++;
                    continue;
                }

                Cleansed.Append(Current);
                StartIndex++;


            }

            return Cleansed.ToString();

        }

        public static bool isReadAssign(string Statement)
        {
            if(Statement.StartsWith("Read(",StringComparison.OrdinalIgnoreCase) && Statement.EndsWith(')'))
            {
                return true;
            }

            return false;
        }

        public static Variable ExtractVariable(string Line,out AssignmentType type)
        {
            (string name, string value) varContainer = new("", "");
            StringBuilder token = new();
            bool inQoutes = false, parsedEqualSign = false;
            type = AssignmentType.Variable;

            foreach (char c in Line)
            {
                if (char.IsWhiteSpace(c) && !inQoutes)
                {
                    continue;
                }

                if (c == '=' && !inQoutes)
                {

                    if (parsedEqualSign)
                    {
                        throw new Exception("Cannot Declare more than one equal sign!");
                    }

                    varContainer.name = token.ToString();
                    token.Clear();
                    parsedEqualSign = true;
                    continue;
                }

                if (c == '"')
                {
                    inQoutes = !inQoutes;
                    continue;
                }



                if (token.ToString().Contains("Read", StringComparison.OrdinalIgnoreCase) && !inQoutes)
                {
                    type = AssignmentType.Read;
                    token.Clear();
                    continue;
                }

                if(token.ToString().Contains("Run",StringComparison.OrdinalIgnoreCase) && !inQoutes)
                {
                    type = AssignmentType.Run;
                    token.Clear();
                }

                if (c == '(' || c == ')' && !inQoutes)
                {
                    continue;
                }

                token.Append(c);
            }

            if (token.Length > 0)
            {
                varContainer.value = token.ToString();
                token.Clear();
            }

            return new(varContainer.name, varContainer.value);
        }

        public static bool HasAssignment(string Line) // Assignment signifier
        {
            try
            {
                int Assignment = 0;

                for (int i = 0; i < Line.Length; i++)
                {
                    char current = Line[i];

                    if (current == '=')
                    {
                        Assignment++;
                    }
                }

                if (Assignment == 1)
                {
                    return true;
                }

                if (Assignment > 1)
                {
                    throw new Exception("Cannot have more than  1 assignment operator");
                }

                return false;

            }catch(Exception ex)
            {
                Utils.Print("Assignment Error", ex, new(PrintOptions.Error, true));
                return false;
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



        public static string ExpandVariables(string Line,List<Variable> Variables)
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

        public static string? Input(string Prompt)
        {
            Console.WriteLine(Prompt);
            return Console.ReadLine();
        }

        public static Expression? ExtractExpression(string Line)
        {
            int parentCount = 0;
            bool inQoutes = false;
            StringBuilder Sb = new();
            (string Left, string op, string right) Properties = ("","","");
            Expression current = null;

            foreach (char c in Line)
            {
                if (c == ' ' && ! inQoutes)
                {

                    if (Sb.Length > 0)
                    {
                        if(Properties.Left.Length == 0) {
                            Properties.Left = Sb.ToString();
                            Sb.Clear();
                            continue;
                        }else if(Properties.op.Length == 0)
                        {
                            Properties.op = Sb.ToString();
                            Sb.Clear();
                            continue;
                        }
                        else if (Properties.right.Length == 0)
                        {
                            Properties.right = Sb.ToString();
                            Sb.Clear();
                            continue;
                        }
                    }

                    continue;
                }

                if (c == '"')
                {
                    inQoutes = !inQoutes;
                    continue;
                }

                if ((c == '(' || c == ')') && !inQoutes)
                {
                    parentCount++;
                    continue;
                }

                if(parentCount > 2)
                {
                    throw new Exception("Cannot have more than two parenthesis! Currently Unsupported!");
                }

                if(parentCount < 1)
                {
                    throw new Exception("Parenthesis missing from Statement");
                }

                Sb.Append(c);
            }

            if(Sb.Length > 0)
            {
                Properties.right = Sb.ToString();
                Sb.Clear();
            }

            string expr = Sb.ToString();

            if (Properties.op == "==")
            {
                Expression Left,Right;

                //Console.WriteLine("Debug: L {0} , R {1}", Lvar.value, Rvar.value);
                // Treat both as literals first
                Left = new LiteralExpression(Properties.Left);
                Right = new LiteralExpression(Properties.right);


                current = new EqualTo(Left,Right);

            }else if (Properties.op == "!="){

                Expression Left, Right;
                // Treat both as literals first
                Left = new LiteralExpression(Properties.Left);
                Right = new LiteralExpression(Properties.right);


                current = new NotEqualTo(Left, Right);

            }

            return current;
        }

        public static (string target,string cmd) ExtractCommand(string Line)
        {
            StringBuilder token = new();
            (string target, string cmd) res = new(string.Empty, string.Empty);
            bool inQoutes = false;
            foreach(char c in Line)
            {
                if((c == '(' || c == ')') && !inQoutes)
                {
                    continue;
                }

                if(c == '=' && !inQoutes)
                {
                    res.target = token.ToString();
                    token.Clear();
                    continue;
                }

                if(c == ' ' && !inQoutes)
                {
                    continue;
                }
                
                if(c == '"')
                {
                    inQoutes = !inQoutes;
                    continue;
                }

                token.Append(c);
            }

            if(token.Length > 0)
            {
                res.cmd = token.ToString();
                token.Clear();
            }

            return res;
        }
    }
}
