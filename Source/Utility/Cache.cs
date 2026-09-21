using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Utility
{
    internal static class FunctionCache // Unused, might repurpose for later.
    {

        private static List<Function> Functions = new();


        public static void Add(Function func)
        {
            Functions.Add(func);
        }

        public static int RunFunc(string name,List<Parameter> param,List<Variable> Scope)
        {
            Function? result = Functions.FirstOrDefault(ex => ex.name == name);

            if(result is null)
            {
                return 1;
            }

            return result.ExecuteBlock(param,Scope);
        }

    }

}
