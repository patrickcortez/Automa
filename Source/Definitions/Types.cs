using Automa.Source.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Definitions
{

    internal interface IValue
    {
        object Eval(List<Variable>? Scope = null);
    }

    internal abstract record Value<T> : IValue
    {
        public object Eval(List<Variable>? Scope = null) => UEval(Scope)!;
        protected abstract T UEval(List<Variable>? Scope);
    }

    internal record AutomaInteger(int value) : Value<int>
    {
        protected override int UEval(List<Variable>? Scope) => value;
    }

    internal record AutomaString(string value) : Value<string>
    {
        protected override string UEval(List<Variable>? Scope) => value;
    }

    internal record AutomaBoolean(bool value) : Value<bool>
    {
        protected override bool UEval(List<Variable>? Scope) => value;
    }

    internal record AutomaFunction(string name,List<Parameter> args) : Value<Return>
    {
        protected override Return UEval(List<Variable>? Scope) => FunctionTable.RunFunc(name, args, Scope);
    }

    // for error handling
    internal record AutomaNull() : Value<object>
    {
        protected override object? UEval(List<Variable>? Scope) => null;
    }

}
