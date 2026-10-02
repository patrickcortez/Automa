using Automa.Source.Definitions;

namespace Automa.Source.Core
{
    internal static class FunctionTable // Unused, might repurpose for later.
    {

        private static Dictionary<string,Function> Functions = new();


        public static void Add(string name,Function func)
        {
            Functions.Add(name,func);
        }

        public static Function GetFunc(string name) => Functions[name];

        public static Return? RunFunc(string name,List<Parameter> param,List<Variable> Scope,bool isdebug=false)
        {

            if (!Functions.ContainsKey(name))
            {
                return null;
            }

            Function target = Functions[name];


            int exitc= target.ExecuteBlock(param,Scope,isdebug);

            if(exitc is not 0)
            {
                return null;
            }


            Return ret = target.returnVal;

            return ret;
        }

        public static void Purge()
        {
            Functions.Clear();
        }

    }

}
