using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Executor(List<object> Instructions, List<Variable>? Variables = null)
    {
        public int Start(bool isdebug = false)
        {
            try
            {
                if(Variables is null) // instantiate once null
                {
                    Variables = new();
                }

                bool prevSucc = false;
                
                foreach (var instruction in Instructions)
                {
                   // Cache.Variables = Variables;
                    switch (instruction)
                    {
                        case WriteInstruction write:

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing Write");
                            }

                            Console.WriteLine(ExpandVariables(write.Content,Variables));
                            break;
                        case AssignType assignment:

                            if (isdebug)
                            {
                                Console.Write("[Debug] Executing Assignment with");
                            }

                            if (assignment is VariableAssign var)
                            {
                                if (isdebug)
                                {
                                    Console.WriteLine("[Debug] with Variable assignment type");
                                }

                                Variable newVariable = var.variable;
                                Variable? findVariable = FindVariable(newVariable.name,Variables);
                                Variable? FindValue = FindVariable(newVariable.value, Variables);

                                if(findVariable is not null)
                                {
                                    int vIndex = Variables.IndexOf(findVariable);

                                    if(FindValue is not null)
                                    {
                                        Variables[vIndex].value = FindValue.value;
                                        continue;
                                    }

                                    Variables[vIndex].value = newVariable.value;
                                    continue;
                                }
                                
                                if(FindValue is not null)
                                {
                                    int vIndex = Variables.IndexOf(FindValue);

                                    newVariable.value = Variables[vIndex].value;
                                }

                                Variables.Add(newVariable);
                                continue;
                            }else if(assignment is ReadAssign read)
                            {
                                if (isdebug)
                                {
                                    Console.WriteLine("[Debug] with Read assignment type");
                                }

                                if(read.target is "null")
                                {
                                    Input(read.Prompt);
                                    continue;
                                }

                                Variable? findVariable = FindVariable(read.target, Variables);

                                if(findVariable is not null)
                                {
                                    int vIndex = Variables.IndexOf(findVariable);

                                    Variables[vIndex].value = Input(read.Prompt) ?? "";
                                    continue;
                                }

                                Variable newVariable = new(read.target, Input(read.Prompt) ?? "");
                                Variables.Add(newVariable);
                                continue;
                            } 
                            else if (assignment is RunAssignment run)
                            {
                                if (isdebug)
                                {
                                    Console.WriteLine("[Debug] with Run assignment type");
                                }

                                if(run.Properties.Target is null)
                                {
                                    run.Run();
                                    continue;
                                }

                                Variable? findVariable = FindVariable(run.Properties.Target, Variables);

                                if(findVariable is not null)
                                {
                                    int vIndex = Variables.IndexOf(findVariable);
                                    Variables[vIndex].value = run.Run();
                                    continue;
                                }

                                Variables.Add(new(run.Properties.Target, run.Run()));
                                continue;
                            }

                            break;
                        case IfBlock block:

                            if (prevSucc)
                            {
                                prevSucc = false;
                            }
                            
                            block.Variable = Variables;

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing IFBlock with {0} instructions", block.Instructions.Count);
                            }

                            if (block.expression is EqualTo eq)
                            {
                                //Console.WriteLine("Debug: Block is a EQTO");
                                eq.UpdateVariables(block.Variable);
                                if (eq.Evaluate())
                                {

                                    //Console.WriteLine("Debug: Block is Executing");
                                    
                                    block.ExecuteBlock();
                                    prevSucc = !prevSucc;
                                    Variables = block.Variable.Intersect(Variables).ToList();
                                }

                            }else if(block.expression is NotEqualTo neq)
                            {
                                //Console.WriteLine("Debug: Block is a NEQTO");
                                neq.UpdateVariables(block.Variable);
                                if (neq.Evaluate())
                                {
                                    //Console.WriteLine("Debug: Block is Executing");
                                    block.ExecuteBlock();
                                    prevSucc = !prevSucc;
                                    Variables = block.Variable.Intersect(Variables).ToList();
                                }
                            }

                            break;
                        case Elif elif:

                            //Cache.CurrentBlock = elif.Variables;

                            elif.Variable = Variables; // update global var just incase


                            if (prevSucc)
                            {
                                continue;
                            }

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing EliFBlock with {0} instructions", elif.Instructions.Count);
                            }

                            if (elif.expression is EqualTo EQ)
                            {
                                EQ.UpdateVariables(elif.Variable);
                                if (EQ.Evaluate())
                                {
                                    elif.ExecuteBlock();
                                    Variables = elif.Variable.Intersect(Variables).ToList();
                                    prevSucc = !prevSucc;
                                }
                            }else if(elif.expression is NotEqualTo NEQ)
                            {
                                NEQ.UpdateVariables(elif.Variable);
                                if (NEQ.Evaluate())
                                {
                                    elif.ExecuteBlock();
                                    Variables = elif.Variable.Intersect(Variables).ToList();
                                    prevSucc = !prevSucc;
                                }
                            }

                            break;
                        case Else els:

                            //Cache.CurrentBlock = els.Variables;

                            if (prevSucc)
                            {
                                continue;
                            }

                            els.Variable = Variables;

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing ElseBlock with {0} instructions", els.Instructions.Count);
                            }
                            els.ExecuteBlock();
                            Variables = els.Variable.Intersect(Variables).ToList();
                            break;

                        default:
                            break;

                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Print("Error while Executing File", ex, new(PrintOptions.Error, true));
                return 1;
            }
        }
    }
}
