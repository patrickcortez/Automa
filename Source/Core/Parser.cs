using Automa.Source.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Parser(string[] Tokens)
    {
        string[] Keywords = ["Write", "Read", "If"];

        //string[] LogicalOperators = ["==", "!="];

        private (List<object> Instructions,List<Variable> Variables) Parse()
        {
            List<object> Instructions = new();
            List<object> BlockInstructions = new();
            List<Variable> Variables = new();
            Expression? expr = null;
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

                    if (inBlock)
                    {
                        BlockInstructions.Add(new ReadInstruction(data.target, data.prompt));
                        continue;
                    }

                    Instructions.Add(new ReadInstruction(data.target, data.prompt));
                }
                else if (Line.StartsWith('{') || Line.StartsWith('}'))
                {
                    if (inBlock)
                    {
                        if (Line.StartsWith('}'))
                        {
                            Console.WriteLine("Debug: Block Instructions {0}", BlockInstructions.Count);
                            Instructions.Add(new IfBlock(expr, new(BlockInstructions), Variables));
                            BlockInstructions.Clear();
                           
                        }
                    }

                    inBlock = !inBlock;
                    continue;

                }
                else if (Line.StartsWith(Keywords[2], StringComparison.OrdinalIgnoreCase))
                {

                    expr = ExtractExpression(Line.Substring(Keywords[2].Length, Line.Length - Keywords[2].Length));

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    continue;

                }else // Variable Declaration
                {
                    Variable newVar = ExtractVariable(Line);

                    Console.WriteLine("Current Variable Name: {0} , Value: {1}", newVar.name, newVar.value);

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
