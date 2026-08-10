using Automa.Source.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Parser(string[] Tokens)
    {
        string[] Keywords = ["Write", "Read", "If","Elif","Else","Run"];

        //string[] LogicalOperators = ["==", "!="];

        private T? ParseBlock<T>(List<Variable> _Variables,string StartingLine)
        {
            List<object> Instructions = new();
            List<Variable> Variables = _Variables;
            int index = Tokens.IndexOf(StartingLine), depth=0;
            bool inBlock = false;
            string[] SubTokens = Tokens.Skip(index).ToArray();
            Expression? expr = null;
            Type type = typeof(T);

            TokenType ParsedType = TokenType.Null;

            if(type == typeof(IfBlock))
            {
                ParsedType = TokenType.If;
            }else if(type == typeof(Elif))
            {
                ParsedType = TokenType.Elif;
            }else if(type == typeof(Else))
            {
                ParsedType = TokenType.Else;
            }


            for (int i = 0; i < SubTokens.Length; i++)
            {
                string Line = SubTokens[i];

                if (Line.StartsWith('{') || Line.StartsWith('}'))
                {
                    if (inBlock)
                    {
                        if (Line.StartsWith('{') && depth == 0)
                        {
                            depth++;
                            continue;
                        }

                        if (Line.StartsWith('}') && depth == 0)
                        {
                            break;
                        }
                        else
                        {
                            depth--;
                        }
                    }

                    inBlock = !inBlock;
                    continue;
                }
                else if (Line.StartsWith(Keywords[0], StringComparison.OrdinalIgnoreCase)) // Write
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    Instructions.Add(new WriteInstruction(CleanString(Line.Substring(Keywords[0].Length, Line.Length - Keywords[0].Length))));
                }
                else if (Line.Contains(Keywords[1], StringComparison.OrdinalIgnoreCase)) //Read
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    var data = ExtractRead(Line);

                    Variable? var = FindVariable(data.target, Variables);

                    if (var is null)
                    {
                        var = new(data.target, "");
                        Variables.Add(var);
                    }

                    Instructions.Add(new ReadInstruction(data.target, data.prompt));
                    continue;

                }
                else if (Line.StartsWith(Keywords[2], StringComparison.OrdinalIgnoreCase)) //If
                {
                    if (depth > 0) // Depth Check
                    {
                        continue;
                    }

                    if (inBlock)
                    {
                        Instructions.Add(ParseBlock<IfBlock>(Variables, Line));
                        continue;
                    }

                    expr = ExtractExpression(Line.Substring(Keywords[2].Length, Line.Length - Keywords[2].Length));

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    continue;

                }
                else if (Line.StartsWith(Keywords[3], StringComparison.OrdinalIgnoreCase)) // elif
                { // Elif
                    if (depth > 0) // depth check
                    {
                        continue;
                    }

                    if (inBlock)
                    {
                        Instructions.Add(ParseBlock<Elif>(Variables, Line));
                    }

                    expr = ExtractExpression(Line.Substring(Keywords[3].Length, Line.Length - Keywords[3].Length));

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    continue;
                }else if (Line.StartsWith("Else", StringComparison.OrdinalIgnoreCase))
                {
                    if (inBlock)
                    {
                        Instructions.Add(ParseBlock<Else>(Variables, Line));
                        continue;
                    }
                }
                else if (Line.Contains(Keywords[5], StringComparison.OrdinalIgnoreCase)) // run
                {
                    if (depth > 0)
                    {
                        continue;
                    }


                    var nLine = Line.Replace(Keywords[5], " ");
                    var cmd = ExtractCommand(nLine);


                    Instructions.Add(new RunInstruction(cmd));

                    Variables.Add(new Variable(cmd.target, ""));

                    continue;
                }
                else // Variable Declaration
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    Variable newVar = ExtractVariable(Line);

                    //Console.WriteLine("Debug: Current Variable Name: {0} , Value: {1}", newVar.name, newVar.value);

                    if (Variables.Where(c => c.name == newVar.name).Count() > 0)
                    {
                        var Curr = Variables.FirstOrDefault(c => c.name == newVar.name);
                        int sindex = Variables.IndexOf(Curr);



                        Variable value = FindVariable(newVar.value, Variables);

                        if (value is not null)
                        {
                            Variables[sindex] = value;
                            continue;
                        }


                        //Console.WriteLine("Debug: Current Variable Changed value to: {0} ", newVar.value);


                        Variables[sindex].value = newVar.value;
                        continue;
                    }


                    Variables.Add(newVar);
                    continue;
                }

            }


            if (ParsedType == TokenType.If)
            {
                IfBlock block = new(expr, Instructions, Variables);
                return (T)(object)block;
            }else if(ParsedType == TokenType.Elif)
            {
                Elif block = new(expr, Instructions, Variables);
                return (T)(object)block;
            }else if(ParsedType == TokenType.Else)
            {
                Else block = new(Instructions, Variables);
                return (T)(object)block;
            }

            throw new Exception($"{type.Name} is not a valid block type!");
        }

        private (List<object> Instructions,List<Variable> Variables) Parse()
        {
            List<object> Instructions = new();
            List<object> BlockInstructions = new();
            List<Variable> Variables = new();
            List<Variable> BlockVariables = new();
            Expression? expr = null;
            TokenType Current = TokenType.Null;
            bool inBlock = false;
            int depth = 0;

            foreach(string Line in Tokens)
            {
                Cache.Variables = Variables; // overwrite each iteration

                if (Line.StartsWith(Keywords[0], StringComparison.OrdinalIgnoreCase)) //Write
                {
                    if(depth > 0)
                    {
                        continue;
                    }

                    if (inBlock)
                    {
                        BlockInstructions.Add(new WriteInstruction(CleanString(Line.Substring(Keywords[0].Length, Line.Length - Keywords[0].Length))));
                        continue;
                    }

                    Instructions.Add(new WriteInstruction(CleanString(Line.Substring(Keywords[0].Length, Line.Length - Keywords[0].Length))));
                }
                else if (Line.Contains(Keywords[1], StringComparison.OrdinalIgnoreCase)) //Read
                {
                    if (depth > 0)
                    {
                        continue;
                    }
                    var data = ExtractRead(Line);

                    Variable? var = FindVariable(data.target, Variables);

                    if (var is null && !inBlock)
                    {
                        var = new(data.target, "");
                        Variables.Add(var);
                    }

                    if (inBlock)
                    {
                        BlockInstructions.Add(new ReadInstruction(data.target, data.prompt));
                        if (var is not null)
                        {
                            BlockVariables.Add(var);
                        }
                        continue;
                    }

                    Instructions.Add(new ReadInstruction(data.target, data.prompt));
                    continue;
                }
                else if (Line.StartsWith('{') || Line.StartsWith('}')) // Block Identifiers
                {
                    if (inBlock)
                    {
                        if (Line.StartsWith('{'))
                        {
                            depth++;
                            continue;
                        }

                        if (Line.StartsWith('}') && depth == 0)
                        {
                            //Console.WriteLine("Debug: Block Instructions {0}", BlockInstructions.Count);
                            if (Current == TokenType.If)
                            {
                                Instructions.Add(new IfBlock(expr, new(BlockInstructions), new(BlockVariables)));
                                BlockInstructions.Clear(); // always clear Block Instruction =P, since List is a reference type.
                                BlockVariables.Clear();
                            }
                            else if (Current == TokenType.Elif)
                            {
                                Instructions.Add(new Elif(expr, new(BlockInstructions), new(BlockVariables)));
                                BlockInstructions.Clear();
                                BlockVariables.Clear();
                            }
                            else if(Current == TokenType.Else)
                            {
                                Instructions.Add(new Else(new(BlockInstructions), new(BlockVariables)));
                                BlockInstructions.Clear();
                                BlockVariables.Clear();
                            }

                        }
                        else
                        {
                            depth--;
                            continue;
                        }
                    }

                    inBlock = !inBlock; // move to if later... gonna implement nesting first
                    continue;

                }
                else if (Line.StartsWith(Keywords[2], StringComparison.OrdinalIgnoreCase)) //If
                {
                    if (depth > 0)
                    {
                        continue;
                    }
                    if (!inBlock)
                    {
                        BlockVariables = new(Variables);
                    }

                    if (inBlock)
                    {
                        BlockInstructions.Add(ParseBlock<IfBlock>(BlockVariables, Line));
                        continue;
                    }

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
                    if (depth > 0) // depth check
                    {
                        continue;
                    }

                    if (!inBlock)
                    {
                        BlockVariables = new(Variables);
                    }

                    if (inBlock)
                    {
                        BlockInstructions.Add(ParseBlock<IfBlock>(BlockVariables, Line));
                        continue;
                    }

                    expr = ExtractExpression(Line.Substring(Keywords[3].Length, Line.Length - Keywords[3].Length));
                    Current = TokenType.Elif;

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    continue;
                }
                else if (Line.StartsWith(Keywords[4], StringComparison.OrdinalIgnoreCase)) { // else

                    if (depth > 0) // depth check
                    {
                        continue;
                    }

                    Current = TokenType.Else;
                    continue;

                } else if (Line.Contains(Keywords[5], StringComparison.OrdinalIgnoreCase)) // run
                {
                    if (depth > 0)
                    {
                        continue;
                    }


                    var nLine = Line.Replace(Keywords[5]," ");
                    var cmd = ExtractCommand(nLine);

                    if (inBlock)
                    {
                        BlockInstructions.Add(new RunInstruction(cmd));

                        BlockVariables.Add(new Variable(cmd.target,""));
                    }

                    Instructions.Add(new RunInstruction(cmd));

                    Variables.Add(new Variable(cmd.target, ""));

                    continue;
                }
                else // Variable Declaration
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    Variable newVar = ExtractVariable(Line);

                    //Console.WriteLine("Debug: Current Variable Name: {0} , Value: {1}", newVar.name, newVar.value);

                    if (Variables.Where(c => c.name == newVar.name).Count() > 0)
                    {
                        var Curr = Variables.FirstOrDefault(c => c.name == newVar.name);
                        int index = Variables.IndexOf(Curr);

                        if (!inBlock)
                        {

                            Variable value = FindVariable(newVar.value, Variables);

                            if (value is not null)
                            {
                                Variables[index] = value;
                                continue;
                            }
                        }
                        else if(inBlock)
                        {
                            Variable value = FindVariable(newVar.value, BlockVariables);

                            if (value is not null)
                            {
                                BlockVariables[index] = value;
                                continue;
                            }
                        }
                        //Console.WriteLine("Debug: Current Variable Changed value to: {0} ", newVar.value);

                        if (inBlock) // Guard clause if were in a Block
                        {
                            Curr = BlockVariables.FirstOrDefault(c => c.name == newVar.name);
                            index = BlockVariables.IndexOf(Curr);

                            BlockVariables[index].value = newVar.value;
                            continue;
                        }

                        Variables[index].value = newVar.value;
                        continue;
                    }

                    if (inBlock)
                    {
                        BlockVariables.Add(newVar);
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
