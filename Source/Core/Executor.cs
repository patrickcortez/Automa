using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Executor(Instruction Current, List<Variable>? Variables = null)
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
                
                while(Current != null)
                {
                   // Cache.Variables = Variables;
                    switch (Current)
                    {
                        case WriteInstruction write:

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing Write");
                            }

                            Console.WriteLine(ExpandVariables(write.Content,Variables));
                            break;
                        case AssignInstruction assignment:

                            if (isdebug)
                            {
                                Console.Write("[Debug] Executing Assignment with");
                            }

                            if (assignment.type is VariableAssign var)
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
                                        break;
                                    }

                                    Variables[vIndex].value = newVariable.value;
                                    break;
                                }
                                
                                if(FindValue is not null)
                                {
                                    int vIndex = Variables.IndexOf(FindValue);

                                    newVariable.value = Variables[vIndex].value;
                                }

                                Variables.Add(newVariable);
                                break;
                            }else if(assignment.type is ReadAssign read)
                            {
                                if (isdebug)
                                {
                                    Console.WriteLine("[Debug] with Read assignment type");
                                }

                                if(read.target is "null")
                                {
                                    Input(read.Prompt);
                                    break;
                                }

                                Variable? findVariable = FindVariable(read.target, Variables);

                                if(findVariable is not null)
                                {
                                    int vIndex = Variables.IndexOf(findVariable);

                                    Variables[vIndex].value = Input(read.Prompt) ?? "";
                                    break;
                                }

                                Variable newVariable = new(read.target, Input(read.Prompt) ?? "");
                                Variables.Add(newVariable);
                                break;
                            } 
                            else if (assignment.type is RunAssignment run)
                            {
                                if (isdebug)
                                {
                                    Console.WriteLine("[Debug] with Run assignment type");
                                }

                                if(run.Properties.Target is null)
                                {
                                    run.Run();
                                    break;
                                }

                                Variable? findVariable = FindVariable(run.Properties.Target, Variables);

                                if(findVariable is not null)
                                {
                                    int vIndex = Variables.IndexOf(findVariable);
                                    Variables[vIndex].value = run.Run();
                                    break;
                                }

                                Variables.Add(new(run.Properties.Target, run.Run()));
                                break;
                            }

                            break;
                        case IfBlock block:
                            prevSucc = false;

                            if (prevSucc)
                            {
                                break;
                            }
                            
                            block.Variable = Variables;

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing IFBlock instructions");
                            }

                            if (block.expression is EqualTo eq)
                            {
                                //Console.WriteLine("Debug: Block is a EQTO");
                                eq.UpdateVariables(block.Variable);
                                if (eq.Evaluate())
                                {

                                    //Console.WriteLine("Debug: Block is Executing");
                                    
                                    block.ExecuteBlock();
                                    prevSucc = true;
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
                                    prevSucc = true;
                                    Variables = block.Variable.Intersect(Variables).ToList();
                                }
                            }

                            break;
                        case Elif elif:

                            if (prevSucc)
                            {
                                break;
                            }

                            elif.Variable = Variables; // update global var just incase

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing EliFBlock instructions");
                            }

                            if (elif.expression is EqualTo EQ)
                            {
                                EQ.UpdateVariables(elif.Variable);
                                if (EQ.Evaluate())
                                {
                                    elif.ExecuteBlock();
                                    Variables = elif.Variable.Intersect(Variables).ToList();
                                    prevSucc = true;
                                }
                            }else if(elif.expression is NotEqualTo NEQ)
                            {
                                NEQ.UpdateVariables(elif.Variable);
                                if (NEQ.Evaluate())
                                {
                                    elif.ExecuteBlock();
                                    Variables = elif.Variable.Intersect(Variables).ToList();
                                    prevSucc = true;
                                }
                            }

                            break;
                        case Else els:

                            //Cache.CurrentBlock = els.Variables;

                            if (prevSucc)
                            {
                                break;
                            }

                            els.Variable = Variables;

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing ElseBlock instructions");
                            }
                            els.ExecuteBlock();
                            prevSucc = !prevSucc;
                            Variables = els.Variable.Intersect(Variables).ToList();
                            break;

                        default:
                            break;

                    }
                    Current = Current.Next;
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
