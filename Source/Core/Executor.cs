using Automa.Source.Core.IO;
using Automa.Source.Definitions;
using static Automa.Source.Utility.Utils;


// Implement function calls and returns

namespace Automa.Source.Core
{

    internal class Executor(Instruction Current, List<Variable>? Variables = null, Return? ret = null) // Var and ret is opt, no need for ref since ret is a ref type.
    {

        public int Start(bool isdebug = false,List<Variable>? Arguments=null,List<Variable>? Outer=null)
        {
            try
            {
                if(Variables is null) // instantiate once null
                {
                    Variables = new();
                }

                if(Arguments is not null)
                {
                    Variables.AddRange(Arguments);
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

                if (isdebug)
                {
                    Console.WriteLine("[DEBUG] Executing AST...");
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

                            OutputHandler.Out(ExpandVariables(write.Content,Variables));  // expand variables and ansi before outputting
                            break;
                        case AssignInstruction assignment:

                            if (isdebug)
                            {
                                Console.Write("[Debug] Executing Assignment with ");
                            }
                            
                            if (assignment.type is VariableAssign var)
                            {
                                if (isdebug)
                                {
                                    Console.WriteLine("Variable assignment type, Variable: {0}",var);
                                }

                                // will be moved to VariableAssign's Evaluate()

                                Variable newVariable = var.variable;
                                Variable? findVariable = FindVariable(newVariable.name,Variables);
                                Variable? FindValue = FindVariable((newVariable.value.Eval() is AutomaString str)? str.value : "", Variables);

                                if(findVariable is not null)
                                {
                                    int vIndex = Variables.IndexOf(findVariable);

                                    if(FindValue is not null)
                                    {
                                        Variables[vIndex] = Variables[vIndex] with { value = FindValue.value };
                                        break;
                                    }

                                    Variables[vIndex]= Variables[vIndex] with { value = newVariable.value };
                                    break;
                                }

                                IValue cleaned = newVariable.value;

                                if(newVariable.type is VariableType.Identifier)
                                {
                                    if(newVariable.value is AutomaString nstr)
                                    {
                                       Variable res= Variables.FirstOrDefault(ex => ex.name == nstr.value) ?? throw new Exception($"Cannot assign non-existent variable {nstr.value}");

                                        cleaned = res.value switch
                                        {
                                            AutomaString nstr2 => new AutomaString(nstr2.value),
                                            AutomaInteger nint2 => new AutomaInteger(nint2.value),
                                            AutomaBoolean nbool => new AutomaBoolean(nbool.value),
                                            AutomaFunction func => new AutomaFunction(func.name, func.args),
                                            _ => new AutomaNull()
                                        };
                                    }
                                }

                                Variable declared = new(newVariable.name, cleaned, newVariable.type);
                                
                                if(FindValue is not null)
                                {
                                    int vIndex = Variables.IndexOf(FindValue);

                                    declared = declared with { value = Variables[vIndex].value };
                                }

                                Variables.Add(declared);
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

                                    Variables[vIndex] = Variables[vIndex] with { value = new AutomaString( Input(read.Prompt) ?? "")};
                                    break;
                                }

                                Variable newVariable = new(read.target, new AutomaString(Input(read.Prompt) ?? ""));
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
                                    Variables[vIndex] = Variables[vIndex] with { value = new AutomaInteger(run.Run()) };
                                    break;
                                }

                                Variables.Add(new(run.Properties.Target, new AutomaInteger(run.Run())));
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
                            }else if(assignment.type is UnaryAssign UA)
                            {
                                if (isdebug)
                                {
                                    Console.WriteLine("[DEBUG] Unary Assign");
                                }

                                UA.UpdateScope(Variables);
                                break;
                            }else if(assignment.type is FunctionCall FA)
                            {
                                if (isdebug)
                                {
                                    Console.WriteLine(" [DEBUG] Function Call");
                                    Console.WriteLine(" [DEBUG] With variables:");
                                    Variables.ForEach(item =>  Console.WriteLine(item));
                                }

                                FA.Invoke(Variables,isdebug);
                            }

                            break;
                        case IfBlock If:
                            prevSucc = false;

                            Hold = Variables.ToList(); // Hold original for comparison

                            if (If.Eval(Variables))
                            {
                                If.ExecuteBlock(Variables);
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

                        case WhileBlock While:

                            Hold = Variables.ToList();

                            if (While.Eval(Variables))
                            {
                                While.ExecuteBlock(Variables);
                            }

                            Compare(Variables);
                            break;
                        case ReturnInstruction returning:

                            if (isdebug)
                            {
                                Console.WriteLine("[DEBUG] Running return instruction");
                            }

                            switch (returning.value.value)
                            {
                                case AutomaString str:

                                    if(returning.value.type is VariableType.Identifier)
                                    {
                                        Variable? target = Variables.FirstOrDefault(ex => ex.name == str.value);

                                        if(target is null)
                                        {
                                            throw new Exception($"Variable {str.value} doesn't exist");
                                        }

                                        int index =  Variables.IndexOf(target);

                                        if(target.value is AutomaString stri)
                                        {
                                            ret.value = new AutomaString(stri.value);
                                        }
                                        else if(target.value is AutomaInteger inte)
                                        {
                                            ret.value = new AutomaInteger(inte.value);
                                        }
                                        else if(target.value is AutomaBoolean abo)
                                        {
                                            ret.value = new AutomaBoolean(abo.value);
                                        }else if(target.value is AutomaFunction func)
                                        {
                                            Return tmp = (Return)func.Eval(Variables);

                                            ret.value = tmp.value;
                                        }

                                        
                                        break;
                                    }

                                    ret.value =  new AutomaString(str.value);
                                break;
                                case AutomaInteger integ:
                                    ret = ret with { value = new AutomaInteger(integ.value) };
                                break;

                                case AutomaBoolean botoma:
                                    ret = ret with { value = new AutomaBoolean(botoma.value) };
                                break;

                                //case AutomaFunction func:

                                //    ret. =  func.Eval<object>(Variables) };

                                //    break;
                            }

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
