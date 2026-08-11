using Automa.Source.Utility;
using Microsoft.VisualBasic;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Executor(List<object> Instructions, List<Variable> Variables)
    {
        public int Start()
        {
            try
            {
                bool prevSucc = false;
                
                foreach (var instruction in Instructions)
                {
                   // Cache.Variables = Variables;
                    switch (instruction)
                    {
                        case WriteInstruction write:
                            Console.WriteLine(ExpandVariables(write.Content,Variables));
                            break;
                        case AssignInstruction assignment:

                            if(assignment.type is VariableAssign var)
                            {
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
                            }else if(assignment.type is ReadAssign read)
                            {
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
                            else if (assignment.type is RunAssignment run)
                            {
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

                            //Console.WriteLine("Debug: Parsing IFBlock");

                            if (prevSucc)
                            {
                                prevSucc = false;
                            }

                            //Cache.CurrentBlock = block.Variables;
                            block.Variable = Variables;

                            if (block.expression is EqualTo eq)
                            {
                                //Console.WriteLine("Debug: Block is a EQTO");
                                eq.UpdateVariables(block.Variable);
                                if (eq.Evaluate())
                                {

                                    //Console.WriteLine("Debug: Block is Executing");
                                    
                                    block.ExecuteBlock();
                                    prevSucc = !prevSucc;
                                    Variables = block.Variables.Intersect(Variables).ToList();
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
                                    Variables = block.Variables.Intersect(Variables).ToList();
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

                            if (elif.expression is EqualTo EQ)
                            {
                                EQ.UpdateVariables(elif.Variable);
                                if (EQ.Evaluate())
                                {
                                    elif.ExecuteBlock();
                                    Variables = elif.Variables.Intersect(Variables).ToList();
                                    prevSucc = !prevSucc;
                                }
                            }else if(elif.expression is NotEqualTo NEQ)
                            {
                                NEQ.UpdateVariables(elif.Variable);
                                if (NEQ.Evaluate())
                                {
                                    elif.ExecuteBlock();
                                    Variables = elif.Variables.Intersect(Variables).ToList();
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
                            els.ExecuteBlock();
                            Variables = els.Variables.Intersect(Variables).ToList();
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
