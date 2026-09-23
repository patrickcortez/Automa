using Automa.Source.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Definitions
{
    //--- for arithmetic parser

    internal abstract record class ArithmeticNode { public abstract int Eval(List<Variable> Scope); }

    internal record class NumberNode(int value) : ArithmeticNode
    {
        public override int Eval(List<Variable>? Scope) => value;
    }

    internal record class VariableNode(string name) : ArithmeticNode
    {


        public override int Eval(List<Variable>? Scope)
        {
            if (Scope is null)
            {
                throw new Exception("Current Scope not Set!");
            }

            Variable? result = Scope.FirstOrDefault(ex => ex.name == name);

            if (result == null)
            {
                throw new ArgumentNullException($"Variable {name} doesn't exist!");
            }

            if (result.type != VariableType.Int)
            {
                throw new Exception($"Variable {name} is not an integer!");
            }

            return int.Parse(result.value);
        }
    }

    internal record class FunctionNode(string name, List<Parameter> Params) : ArithmeticNode
    {
        public override int Eval(List<Variable> Scope)
        {
            Return? result = FunctionTable.RunFunc(name, Params, Scope);

            if (result is null)
            {
                return -1;
            }

            if (result.type is not VariableType.Int)
            {
                return -1;
            }

            return int.Parse(result.value);


        }
    }

    internal record class BinaryOpNode(ArithmeticNode left, char Op, ArithmeticNode right) : ArithmeticNode
    {

        public override int Eval(List<Variable> Scope) => Op switch
        {
            '+' => left.Eval(Scope) + right.Eval(Scope),
            '-' => left.Eval(Scope) - right.Eval(Scope),
            '*' => left.Eval(Scope) * right.Eval(Scope),
            '/' => left.Eval(Scope) / right.Eval(Scope),
            _ => throw new InvalidOperationException($"Unknown Operation {Op}")
        };
    }


}
