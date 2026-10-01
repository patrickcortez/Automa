using Automa.Source.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Definitions
{

    // Functions
    internal record Return(IValue value, VariableType type); // Function return

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

                switch (CT)
                {
                    case VariableType.Int:
                        Args[pos] = Args[pos] with { type = CT, value = new AutomaInteger(int.Parse(par.value)) };
                        break;
                    case VariableType.Boolean:
                        Args[pos] = Args[pos] with { type = CT, value = new AutomaBoolean(bool.Parse(par.value)) };
                        break;
                    case VariableType.String:

                        Args[pos] = Args[pos] with { type = CT, value = new AutomaString(par.value) };
                        break;
                    case VariableType.Identifier:
                        Variable? Result = Scope.FirstOrDefault(ex => ex.name == par.value);

                        Args[pos] = Args[pos] with { type = Result.type, value = Result.value };

                        break;

                }

                pos++;
            }
            }
 

            Executor execute = new(Body);

            return execute.Start(false, Args, Scope);
        }


    }

    internal record Parameter(string value, VariableType type);
}
