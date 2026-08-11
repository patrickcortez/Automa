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

        private T? ParseBlock<T>(string StartingLine)
        {
            List<object> Instructions = new();
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
                else if (Line.StartsWith(Keywords[1], StringComparison.OrdinalIgnoreCase)) //Read
                {
                    throw new Exception("Cannot use Read outside of Assignment");
                }
                else if (Line.StartsWith(Keywords[2], StringComparison.OrdinalIgnoreCase)) //If
                {
                    if (depth > 0) // Depth Check
                    {
                        continue;
                    }

                    if (inBlock)
                    {
                        Instructions.Add(ParseBlock<IfBlock>(Line));
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
                        Instructions.Add(ParseBlock<Elif>(Line));
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
                        Instructions.Add(ParseBlock<Else>(Line));
                        continue;
                    }
                }
                else if (Line.StartsWith(Keywords[5], StringComparison.OrdinalIgnoreCase)) // run
                {

                    throw new Exception("Cannot use Run Outside of Assignment");
                }
                else if (HasAssignment(Line)) // Variable Declaration
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    
                    Variable newVar = ExtractVariable(Line, out AssignmentType Atype);


                    if (Atype == AssignmentType.Read)
                    {


                        Instructions.Add(new AssignInstruction(new ReadAssign(newVar.name, newVar.value)));
                        continue;
                    }
                    else if (Atype == AssignmentType.Run)
                    {


                        Instructions.Add(new AssignInstruction(new RunAssignment((newVar.name, newVar.value))));
                        continue;
                    }

                    //Console.WriteLine("Debug: Current Variable Name: {0} , Value: {1}", newVar.name, newVar.value);

                    Instructions.Add(new AssignInstruction(new VariableAssign(newVar)));
                    continue;


                }

            }


            if (ParsedType == TokenType.If)
            {
                IfBlock block = new(expr, Instructions, new());
                return (T)(object)block;
            }else if(ParsedType == TokenType.Elif)
            {
                Elif block = new(expr, Instructions, new());
                return (T)(object)block;
            }else if(ParsedType == TokenType.Else)
            {
                Else block = new(Instructions, new());
                return (T)(object)block;
            }

            throw new Exception($"{type.Name} is not a valid block type!");
        }

        private (List<object> Instructions, List<Variable> Variables) Parse()
        {
            List<object> Instructions = new();
            List<object> BlockInstructions = new();
            List<Variable> Variables = new();
            List<Variable> BlockVariables = new();
            Expression? expr = null;
            TokenType Current = TokenType.Null;
            bool inBlock = false;
            int depth = 0;

            foreach (string Line in Tokens)
            {
                Cache.Variables = Variables; // overwrite each iteration

                if (Line.StartsWith(Keywords[0], StringComparison.OrdinalIgnoreCase)) //Write
                {
                    if (depth > 0)
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
                else if (Line.StartsWith(Keywords[1], StringComparison.OrdinalIgnoreCase)) //Read
                {
                    throw new Exception("Read cannot be used outside of assignment operations");
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
                                Instructions.Add(new IfBlock(expr, new(BlockInstructions), new()));
                                BlockInstructions.Clear(); // always clear Block Instruction =P, since List is a reference type.
                            }
                            else if (Current == TokenType.Elif)
                            {
                                Instructions.Add(new Elif(expr, new(BlockInstructions), new()));
                                BlockInstructions.Clear();
                            }
                            else if (Current == TokenType.Else)
                            {
                                Instructions.Add(new Else(new(BlockInstructions), new()));
                                BlockInstructions.Clear();

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


                    if (inBlock)
                    {
                        BlockInstructions.Add(ParseBlock<IfBlock>(Line));
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

                    if (inBlock)
                    {
                        BlockInstructions.Add(ParseBlock<IfBlock>(Line));
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
                else if (Line.StartsWith(Keywords[4], StringComparison.OrdinalIgnoreCase))
                { // else

                    if (depth > 0) // depth check
                    {
                        continue;
                    }

                    Current = TokenType.Else;
                    continue;

                }
                else if (Line.StartsWith(Keywords[5], StringComparison.OrdinalIgnoreCase)) // run
                {

                    throw new Exception("Cannot use Run Outside of Assignment");
                }
                else if (HasAssignment(Line)) // Variable Declaration
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    Variable newVar = ExtractVariable(Line, out AssignmentType type);


                    if (type == AssignmentType.Read)
                    {

                        if (inBlock)
                        {
                            BlockInstructions.Add(new AssignInstruction(new ReadAssign(newVar.name, newVar.value)));
                            continue;
                        }

                        Instructions.Add(new AssignInstruction(new ReadAssign(newVar.name, newVar.value)));
                        continue;
                    }else if(type == AssignmentType.Run)
                    {
                        if (inBlock)
                        {
                            BlockInstructions.Add(new AssignInstruction(new RunAssignment((newVar.name, newVar.value))));
                            continue;
                        }

                        Instructions.Add(new AssignInstruction(new RunAssignment((newVar.name, newVar.value))));
                        continue;
                    }

                    //Console.WriteLine("Debug: Current Variable Name: {0} , Value: {1}", newVar.name, newVar.value);

                    if (inBlock)
                    {
                        BlockInstructions.Add(new AssignInstruction(new VariableAssign(newVar)));
                        continue;
                    }


                    Instructions.Add(new AssignInstruction(new VariableAssign(newVar)));
                    continue;


                }



            }

            return (Instructions, Variables);
        }

        public int Start()
        {
            try
            {
                var Data = Parse();
                Executor exec = new(Data.Instructions, new()); // only pass instructions not the entire variables
                

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
