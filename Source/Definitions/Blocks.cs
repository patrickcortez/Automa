using Automa.Source.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Definitions
{

    // Block Definition
    internal abstract record Block : Instruction
    {
        public Instruction? Body { get; set; } = null;
    }

    // Block types:

    //  Loops
    internal record WhileBlock(LogOp node) : Block // While( expr ) { }
    {
        public Instruction? next { get; set; } = null;

        public bool Eval(List<Variable> Scope) => node switch
        {
            LogicalUnit single => single.Eval(Scope),
            Or or => or.Eval(Scope),
            And and => and.Eval(Scope),
            _ => false
        };


        public int ExecuteBlock(List<Variable> Scope) // While executor
        {
            int exitc = 0;

            while (Eval(Scope))
            {
                int before = Scope.Count; // save Scope count before starting, so its one pass
                Executor execute = new(Body, Scope);

                exitc = execute.Start();

                Scope.RemoveRange(before, Scope.Count - before); // filter out new
            }

            return exitc;
        }

    }

    // Control Flow

    internal record IfBlock(LogOp expression) : Block // if(condition)
    {

        public bool Eval(List<Variable> Scope) => expression switch
        {
            LogicalUnit single => single.Eval(Scope),
            Or or => or.Eval(Scope),
            And and => and.Eval(Scope),
            _ => false
        };

        public int ExecuteBlock(List<Variable> Scope)
        {
            Executor executor = new(Body, Scope); // replace new with Dody later...

            return executor.Start();
        }
    }

    internal record Elif(LogOp expression) : Block // elif(<condition>)
    {

        public bool Eval(List<Variable> Scope) => expression switch
        {
            LogicalUnit single => single.Eval(Scope),
            Or or => or.Eval(Scope),
            And and => and.Eval(Scope),
            _ => false
        };

        public int ExecuteBlock(List<Variable> Scope)
        {
            Executor executor = new(Body, Scope); // replace new with Dody later...

            return executor.Start();
        }
    }

    internal record Else : Block
    {
        public int ExecuteBlock(List<Variable> Scope)
        {
            Executor executor = new(Body, Scope);
            return executor.Start();
        }
    }
}
