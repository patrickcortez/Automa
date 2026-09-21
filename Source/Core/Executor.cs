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
                List<Variable>? Hold = null;

                void Compare(List<Variable> NewList)
                {
                    int original = Hold.Count;
                    int mutated = NewList.Count;

                    int diff = mutated - original;

                    NewList.RemoveRange(original, diff);
                    Hold.Clear();
                }


                
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
                                    Console.WriteLine("Variable assignment type");
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
                                    Console.WriteLine("Read assignment type");
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
                            else if(assignment.type is ArithmeticAssign arith) // handle arithmetic
                            {

                                if (isdebug)
                                {
                                    Console.WriteLine("Arithmetic assignment type");
                                }


                                arith.UpdateScope(Variables); // Update scope.
                                break;
                            }

                            break;
                        case IfBlock block:
                            prevSucc = false;

                            Hold = Variables.ToList(); // Hold original for comparison

                            if (block.Eval(Variables))
                            {
                                block.ExecuteBlock(Variables);
                                prevSucc = true;
                                Compare(Variables); // compare to original
                            }

                            break;
                        case Elif elif:

                            if (prevSucc)
                            {
                                break;
                            }

                            Hold = Variables.ToList();

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing EliFBlock instructions");
                            }

                                if (elif.Eval(Variables))
                                {
                                    elif.ExecuteBlock(Variables);
                                    Compare(Variables);

                                    prevSucc = true;
                                }

                            break;
                        case Else els:

                            //Cache.CurrentBlock = els.Variables;

                            if (prevSucc)
                            {
                                break;
                            }

                            Hold = Variables.ToList();

                            if (isdebug)
                            {
                                Console.WriteLine("[Debug] Executing ElseBlock instructions");
                            }

                            els.ExecuteBlock(Variables);
                            prevSucc = !prevSucc;
                            Compare(Variables);

                            break;

                        default:
                            break;

                    }
                    Instruction prev = Current;
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
