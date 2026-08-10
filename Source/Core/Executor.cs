using Automa.Source.Utility;
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
                    switch (instruction)
                    {
                        case WriteInstruction write:
                            Console.WriteLine(ExpandVariables(write.Content,(Cache.CurrentBlock != null )? Cache.CurrentBlock: Variables));
                            break;
                        case ReadInstruction read:
                            Variable? current = Utils.FindVariable(read.target, Variables);

                            if (current is null)
                            {
                                Console.Write(read.Prompt);
                                current = new(read.target, Console.ReadLine());
                                Variables.Add(current);
                                continue;
                            }

                            int OI = Variables.IndexOf(current);
                            Console.Write(read.Prompt);
                            current.value = Console.ReadLine();
                            Variables[OI] = current;
                            break;
                        case IfBlock block:

                            //Console.WriteLine("Debug: Parsing IFBlock");

                            if (prevSucc)
                            {
                                prevSucc = false;
                            }

                            Cache.CurrentBlock = block.Variables;
                            block.Variables.AddRange(Variables.Where(c => !block.Variables.Contains(c)));

                            if (block.expression is EqualTo eq)
                            {
                                //Console.WriteLine("Debug: Block is a EQTO");
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

                            Cache.CurrentBlock = elif.Variables;

                            elif.Variables.AddRange(Variables.Where(c => !elif.Variables.Contains(c))); // update global var just incase

                            if (prevSucc)
                            {
                                continue;
                            }

                            if (elif.expression is EqualTo EQ)
                            {

                                if (EQ.Evaluate())
                                {
                                    elif.ExecuteBlock();
                                    Variables = elif.Variables.Intersect(Variables).ToList();
                                    prevSucc = !prevSucc;
                                }
                            }else if(elif.expression is NotEqualTo NEQ)
                            {
                                if (NEQ.Evaluate())
                                {
                                    elif.ExecuteBlock();
                                    Variables = elif.Variables.Intersect(Variables).ToList();
                                    prevSucc = !prevSucc;
                                }
                            }

                            break;
                        case Else els:

                            Cache.CurrentBlock = els.Variables;

                            if (prevSucc)
                            {
                                continue;
                            }

                            els.ExecuteBlock();
                            Variables = els.Variables.Intersect(Variables).ToList();
                            break;
                        case RunInstruction run:

                            if(run.Properties.Target != string.Empty)
                            {
                                //Console.WriteLine("Debug: Run Target: {0}", run.Properties.Target);
                                Variable? curr = Variables.FirstOrDefault(c => c.name == run.Properties.Target);
                                

                                if(curr is null)
                                {
                                    string value =  run.Run();
                                    curr = new(run.Properties.Target, value);

                                    Variables.Add(curr);
                                    continue;
                                }

                                int index = Variables.IndexOf(curr);

                                curr.value =  run.Run();

                                Variables[index] = curr;

                            }

                            run.Run();
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
