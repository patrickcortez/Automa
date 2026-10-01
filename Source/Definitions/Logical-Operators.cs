using Automa.Source.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Definitions
{
    // Logical Operators

    internal abstract record LogOp
    {
        public abstract bool Eval(List<Variable> Scope);
    }

    internal record LogicalUnit(Expression<bool> Node) : LogOp
    {
        public override bool Eval(List<Variable> Scope)
        {
            bool Result = Node.Evaluate(Scope);

            return Result;
        }
    }

    internal record And(LogOp Left, LogOp Right) : LogOp
    {

        public override bool Eval(List<Variable> Scope) => Left.Eval(Scope) && Right.Eval(Scope);
    }

    internal record Or(LogOp Left, LogOp Right, LogOp? next = null) : LogOp
    {
        public override bool Eval(List<Variable> Scope) => Left.Eval(Scope) || Right.Eval(Scope);
    }



    // Operands

    internal abstract record Operand
    {
        public abstract IValue Eval(List<Variable> Scope);
    }

    internal record VariableExpression(string VariableName) : Operand
    {
        private Variable? Value { get; set; }

        public Variable? GetVariable(List<Variable> Variables)
        {
            Value = Utils.FindVariable(VariableName, Variables);
            return Value;
        }

        public override IValue Eval(List<Variable> Scope)
        {
            Variable? Result = Scope.FirstOrDefault(ex => ex.name == VariableName);


            if (Result is null)
            {
                throw new Exception($"{VariableName} is not in current scope");
            }

            return Result.value;
        }
    }

    internal record LiteralExpression(IValue value) : Operand
    {
        public override IValue Eval(List<Variable> Scope)
        {
            return value;
        }
    }

    internal record FunctionExpression(IValue value) : Operand
    {
        public override IValue? Eval(List<Variable> Scope)
        {
            Return? ret = null;

            if(value is AutomaFunction func)
            {

                ret = (Return)func.Eval(Scope);

            }

            return ret.value;
        }
    }


    // Expressions

    internal abstract record Expression<T> // Soon to be added: >,<,>= and <=
    {
        public abstract T Evaluate(List<Variable> Variables);
    }


    internal record EqualTo(Operand Left, Operand Right) : Expression<bool>
    {


        public override bool Evaluate(List<Variable> Variables)
        {
            IValue Lval = Left switch
            {
                LiteralExpression lexpr => lexpr.value,
                VariableExpression lexpr => lexpr.Eval(Variables),
                _ => new AutomaNull()
            };

            IValue Rval = Right switch
            {
                LiteralExpression rexpr => rexpr.value,
                VariableExpression rexpr => rexpr.Eval(Variables),
                _ => new AutomaNull() // impossible to reach anyways =P
            };

            return Equals(Lval.Eval(),Rval.Eval());
        }

    }

    internal record NotEqualTo(Operand Left, Operand Right) : Expression<bool>
    {

        public override bool Evaluate(List<Variable> Variables)
        {
            IValue Lval = Left switch
            {
                LiteralExpression lexpr => lexpr.value,
                VariableExpression lexpr => lexpr.Eval(Variables),
                _ => new AutomaNull()
            };

            IValue Rval = Right switch
            {
                LiteralExpression rexpr => rexpr.value,
                VariableExpression rexpr => rexpr.Eval(Variables),
                _ => new AutomaNull()
            };

            return !Equals(Lval.Eval(), Rval.Eval());
        }


    }

    internal record GreaterThan(Operand Left, Operand Right) : Expression<bool>
    {
        public override bool Evaluate(List<Variable> Variables)
        {
            try
            {

                IValue Lval = Left switch
                {
                    LiteralExpression rexpr => rexpr.value,
                    VariableExpression rexpr => rexpr.Eval(Variables),
                    _ => new AutomaInteger(0)
                };

                IValue Rval = Right switch
                {
                    LiteralExpression rexpr => rexpr.value,
                    VariableExpression rexpr => rexpr.Eval(Variables),
                    _ => new AutomaInteger(0)
                };

                if (Lval is AutomaString or AutomaBoolean || Rval is AutomaString or AutomaBoolean)
                {
                    throw new Exception($"{Lval} and {Rval}");
                }

                return (int)Lval.Eval() > (int)Rval.Eval();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Can only compare numbers! not: \n{ex}");
                return false;
            }
        }
    }

    internal record LessThan(Operand Left, Operand Right) : Expression<bool>
    {
        public override bool Evaluate(List<Variable> Variables)
        {
            try
            {

                IValue Lval = Left switch
                {
                    LiteralExpression rexpr => rexpr.value,
                    VariableExpression rexpr => rexpr.Eval(Variables),
                    _ => new AutomaInteger(0)
                };

                IValue Rval = Right switch
                {
                    LiteralExpression rexpr => rexpr.value,
                    VariableExpression rexpr => rexpr.Eval(Variables),
                    _ => new AutomaInteger(0)
                };

                if (Lval is AutomaString or AutomaBoolean || Rval is AutomaString or AutomaBoolean)
                {
                    throw new Exception($"{Lval} and {Rval}");
                }

                return (int)Lval.Eval() < (int)Rval.Eval();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Can only compare numbers! not: \n{ex}");
                return false;
            }
        }
    }

    internal record GTE(Operand Left, Operand Right) : Expression<bool>
    {
        public override bool Evaluate(List<Variable> Variables)
        {
            try
            {

                IValue Lval = Left switch
                {
                    LiteralExpression rexpr => rexpr.value,
                    VariableExpression rexpr => rexpr.Eval(Variables),
                    _ => new AutomaInteger(0)
                };

                IValue Rval = Right switch
                {
                    LiteralExpression rexpr => rexpr.value,
                    VariableExpression rexpr => rexpr.Eval(Variables),
                    _ => new AutomaInteger(0)
                };

                if (Lval is AutomaString or AutomaBoolean || Rval is AutomaString or AutomaBoolean)
                {
                    throw new Exception($"{Lval} and {Rval}");
                }

                return (int)Lval.Eval() >= (int)Rval.Eval();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Can only compare numbers! not: \n{ex}");
                return false;
            }
        }
    }

    internal record LTE(Operand Left, Operand Right) : Expression<bool>
    {
        public override bool Evaluate(List<Variable> Variables)
        {
            try
            {

                IValue Lval = Left switch
                {
                    LiteralExpression rexpr => rexpr.value,
                    VariableExpression rexpr => rexpr.Eval(Variables),
                    _ => new AutomaInteger(0)
                };

                IValue Rval = Right switch
                {
                    LiteralExpression rexpr => rexpr.value,
                    VariableExpression rexpr => rexpr.Eval(Variables),
                    _ => new AutomaInteger(0)
                };

                if (Lval is AutomaString or AutomaBoolean || Rval is AutomaString or AutomaBoolean)
                {
                    throw new Exception($"{Lval} and {Rval}");
                }

                return (int)Lval.Eval() <= (int)Rval.Eval();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Can only compare numbers! not: \n{ex}");
                return false;
            }
        }
    }


}
