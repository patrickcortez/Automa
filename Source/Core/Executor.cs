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
                foreach (var instruction in Instructions)
                {
                    switch (instruction)
                    {
                        case WriteInstruction write:
                            Console.WriteLine(write.Content);
                            break;
                        case ReadInstruction read:
                            Variable? current = Utils.FindVariable(read.target, Variables);

                            if (current is null)
                            {
                                throw new Exception($"Variable {read.target} not Found");
                            }

                            int OI = Variables.IndexOf(current);

                            current.value = Console.ReadLine();
                            Variables[OI] = current;
                            break;
                        case IfBlock block:

                            Console.WriteLine("Debug: Parsing IFBlock");

                            if(block.expression is EqualTo eq)
                            {
                                Console.WriteLine("Debug: Block is a EQTO");
                                if (eq.Evaluate())
                                {
                                    Console.WriteLine("Debug: Block is Executing");
                                    block.ExecuteIF();
                                }
                            }else if(block.expression is NotEqualTo neq)
                            {
                                Console.WriteLine("Debug: Block is a NEQTO");
                                if (neq.Evaluate())
                                {
                                    Console.WriteLine("Debug: Block is Executing");
                                    block.ExecuteIF();
                                }
                            }

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
