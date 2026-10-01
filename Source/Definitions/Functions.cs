using Automa.Source.Core;

namespace Automa.Source.Definitions
{

    // Functions
    internal record Return // Function return
    {
        public IValue value { get; set; }
        public VariableType type { get; set; }

        public Return(IValue value, VariableType type)
        {
            this.value = value;
            this.type = type;
        }
    }

    // function definition
    internal record Function(Return returnVal, List<Variable> Args) : Block
    {
        public int ExecuteBlock(List<Parameter>? Param, List<Variable>? Scope,bool isdebug=false)
        {
            if(Param is not null)
            {
            int pos = 0;


            foreach (var par in Param) // parse parameters
            {
                VariableType CT = par.type;

                    if (isdebug)
                    {
                        Console.WriteLine($"Current Parameter: {par.value} with type {par.type}");
                    }

                    switch (par.value)
                    {
                        case AutomaInteger inte:
                            Args[pos] = Args[pos] with { type = CT, value = new AutomaInteger(inte.value) };
                            break;
                        case AutomaBoolean abo:
                            Args[pos] = Args[pos] with { type = CT, value = new AutomaBoolean(abo.value) };
                            break;
                        case AutomaString str:

                            if(par.type is VariableType.Identifier)
                            {
                                Variable? Result = Scope.FirstOrDefault(ex => ex.name == str.value);

                                Args[pos] = Args[pos] with { type = Result.type, value = Result.value };
                                break;
                            }

                            Args[pos] = Args[pos] with { type = CT, value = new AutomaString(str.value) };
                            break;

                    }

                pos++;
            }
            }
 

            Executor execute = new(Body,null,returnVal);

            return execute.Start(false, Args, Scope);
        }


    }

    internal record Parameter(IValue value, VariableType type);
}
