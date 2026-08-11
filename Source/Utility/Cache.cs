using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Utility
{
    internal static class Cache // Unused, might repurpose for later.
    {
        public static bool Debug { get; set; } = false;

        public static List<Variable> Variables  { get; set; }

        public static List<Variable> CurrentBlock { get; set; }

        public static Variable UpdateVariable(Variable current,List<Variable> Variables)
        {
            Variable? updated = Utils.FindVariable(current.name, Variables);

            if(current.value != updated.value)
            {
                current = updated;
            }

            return current;
        }

        
    }
}
