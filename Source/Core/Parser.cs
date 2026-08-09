using Automa.Source.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Parser(string[] Tokens)
    {
        string[] Keywords = ["Write", "Read", "If","Elif","Else"];

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
                                BlockInstructions.Clear();
                            }
                            else if (Current == TokenType.Elif)
                            {
                                Instructions.Add(new Elif(expr, new(BlockInstructions), Variables));
                                BlockInstructions.Clear();
                            }else if(Current == TokenType.Else)
                            {
                                Instructions.Add(new Else(BlockInstructions, Variables));
                            }

                        }
                    }

                    inBlock = !inBlock;
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
                else if (Line.StartsWith(Keywords[3], StringComparison.OrdinalIgnoreCase))
                { // Elif
                    expr = ExtractExpression(Line.Substring(Keywords[3].Length, Line.Length - Keywords[3].Length));
                    Current = TokenType.Elif;

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    continue;
                }
                else if (Line.StartsWith(Keywords[4], StringComparison.OrdinalIgnoreCase)) {

                    Current = TokenType.Else;
                    continue;

                }
                else // Variable Declaration
                {
                    Variable newVar = ExtractVariable(Line);

                    //Console.WriteLine("Debug: Current Variable Name: {0} , Value: {1}", newVar.name, newVar.value);

                    if (Variables.Contains(newVar))
                    {
                        int index = Variables.IndexOf(newVar);

                        //Console.WriteLine("Debug: Current Variable Changed value to: {0} ", newVar.value);

                        Variables[index].value = newVar.value;
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
