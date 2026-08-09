using Automa.Source.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Parser(string[] Tokens)
    {
        string[] Keywords = ["Write", "Read", "If","Elif","Else","Run"];

        //string[] LogicalOperators = ["==", "!="];

        private (List<object> Instructions,List<Variable> Variables) Parse()
        {
            List<object> Instructions = new();
            List<object> BlockInstructions = new();
            List<Variable> Variables = new();
            Expression? expr = null;
            TokenType Current = TokenType.Null;
            bool inBlock = false;

            foreach(string Line in Tokens)
            {
                Cache.Variables = Variables; // overwrite each iteration



                if (Line.StartsWith(Keywords[0], StringComparison.OrdinalIgnoreCase)) //Write
                {

                    if (inBlock)
                    {
                        BlockInstructions.Add(new WriteInstruction(CleanString(Line.Substring(Keywords[0].Length, Line.Length - Keywords[0].Length))));
                        continue;
                    }

                    Instructions.Add(new WriteInstruction(CleanString(Line.Substring(Keywords[0].Length, Line.Length - Keywords[0].Length))));
                }
                else if (Line.Contains(Keywords[1], StringComparison.OrdinalIgnoreCase)) //Read
                {

                    var data = ExtractRead(Line);

                    Variable? var = FindVariable(data.target, Variables);

                    if (var is null)
                    {
                        var = new(data.target, "");
                        Variables.Add(var);
                    }

                    if (inBlock)
                    {
                        BlockInstructions.Add(new ReadInstruction(data.target, data.prompt));
                        continue;
                    }

                    Instructions.Add(new ReadInstruction(data.target, data.prompt));
                    continue;
                }
                else if (Line.StartsWith('{') || Line.StartsWith('}')) // Block Identifiers
                {
                    if (inBlock)
                    {
                        if (Line.StartsWith('}'))
                        {
                            //Console.WriteLine("Debug: Block Instructions {0}", BlockInstructions.Count);
                            if (Current == TokenType.If)
                            {
                                Instructions.Add(new IfBlock(expr, new(BlockInstructions), Variables));
                                BlockInstructions.Clear(); // always clear Block Instruction =P, since List is a reference type.
                            }
                            else if (Current == TokenType.Elif)
                            {
                                Instructions.Add(new Elif(expr, new(BlockInstructions), Variables));
                                BlockInstructions.Clear();
                            }else if(Current == TokenType.Else)
                            {
                                Instructions.Add(new Else(new(BlockInstructions), Variables));
                                BlockInstructions.Clear();
                            }

                        }
                    }

                    inBlock = !inBlock; // move to if later... gonna implement nesting first
                    continue;

                }
                else if (Line.StartsWith(Keywords[2], StringComparison.OrdinalIgnoreCase)) //If
                {

                    expr = ExtractExpression(Line.Substring(Keywords[2].Length, Line.Length - Keywords[2].Length));
                    Current = TokenType.If;

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    

                    continue;

                }
                else if (Line.StartsWith(Keywords[3], StringComparison.OrdinalIgnoreCase)) // elif
                { // Elif
                    expr = ExtractExpression(Line.Substring(Keywords[3].Length, Line.Length - Keywords[3].Length));
                    Current = TokenType.Elif;

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    continue;
                }
                else if (Line.StartsWith(Keywords[4], StringComparison.OrdinalIgnoreCase)) { // else

                    Current = TokenType.Else;
                    continue;

                } else if (Line.Contains(Keywords[5], StringComparison.OrdinalIgnoreCase)) // run
                {
                    var nLine = Line.Replace(Keywords[5]," ");
                    var cmd = ExtractCommand(nLine);
                    Instructions.Add(new RunInstruction(cmd));

                    Variables.Add(new Variable(cmd.target, ""));

                    continue;
                }
                else // Variable Declaration
                {
                    Variable newVar = ExtractVariable(Line);

                    //Console.WriteLine("Debug: Current Variable Name: {0} , Value: {1}", newVar.name, newVar.value);

                    if (Variables.Where(c => c.name == newVar.name).Count() > 0)
                    {
                        var Curr = Variables.FirstOrDefault(c => c.name == newVar.name);
                        int index = Variables.IndexOf(Curr);

                        Variable value = FindVariable(newVar.value, Variables);

                        if(value is not null)
                        {
                            Variables[index] = value;
                            continue;
                        }

                        //Console.WriteLine("Debug: Current Variable Changed value to: {0} ", newVar.value);

                        Variables[index].value = newVar.value;
                        continue;
                    }

                    Variables.Add(newVar);
                    continue;
                }
            }

            return (Instructions,Variables);

        }

        public int Start()
        {
            try
            {
                var Data = Parse();
                Executor exec = new(Data.Instructions, Data.Variables);
                

                return exec.Start();
            }
            catch(Exception ex)
            {
                Print("Error while Parsing File", ex, new(PrintOptions.Error, true));
                return 1;
            }
            
        }
    }
}
