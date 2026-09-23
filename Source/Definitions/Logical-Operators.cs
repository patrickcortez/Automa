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
        public abstract string Eval(List<Variable> Scope);
    }

    internal record VariableExpression(string VariableName) : Operand
    {
        private Variable? Value { get; set; }

        public Variable? GetVariable(List<Variable> Variables)
        {
            Value = Utils.FindVariable(VariableName, Variables);
            return Value;
        }

        public override string Eval(List<Variable> Scope)
        {
            Variable? Result = Scope.FirstOrDefault(ex => ex.name == VariableName);


            if (Result is null)
            {
                throw new Exception($"{VariableName} is not in current scope");
            }

            return Result.value;
        }
    }

    internal record LiteralExpression(string value) : Operand
    {
        public override string Eval(List<Variable> Scope)
        {
            return value;
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
            string Lval = Left switch
            {
                LiteralExpression lexpr => lexpr.value,
                VariableExpression lexpr => lexpr.Eval(Variables),
                _ => ""
            };

            string Rval = Right switch
            {
                LiteralExpression rexpr => rexpr.value,
                VariableExpression rexpr => rexpr.Eval(Variables),
                _ => ""
            };

            if (Lval is "True" or "False" || Rval is "True" or "False")
            {
                bool Lb = false, Rb = false;
                if (Lval is "True" or "False")
                {
                    Lval = Lval.ToLower();
                }

                if (Lval is "True" or "False")
                {
                    Rval = Lval.ToLower();
                }

                return Lb == Rb;
            }

            return Lval == Rval;
        }

    }

    internal record NotEqualTo(Operand Left, Operand Right) : Expression<bool>
    {

        public override bool Evaluate(List<Variable> Variables)
        {
            string Lval = Left switch
            {
                LiteralExpression lexpr => lexpr.value,
                VariableExpression lexpr => lexpr.Eval(Variables),
                _ => ""
            };

            string Rval = Right switch
            {
                LiteralExpression rexpr => rexpr.value,
                VariableExpression rexpr => rexpr.Eval(Variables),
                _ => ""
            };

            return Lval != Rval;
        }


    }

    internal record GreaterThan(Operand Left, Operand Right) : Expression<bool>
    {
        public override bool Evaluate(List<Variable> Variables)
        {
            try
            {

                int Lval = Left switch
                {
                    LiteralExpression rexpr => int.Parse(rexpr.value),
                    VariableExpression rexpr => int.Parse(rexpr.Eval(Variables)),
                    _ => 0
                };

                int Rval = Right switch
                {
                    LiteralExpression rexpr => int.Parse(rexpr.value),
                    VariableExpression rexpr => int.Parse(rexpr.Eval(Variables)),
                    _ => 0
                };



                return Lval > Rval;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Cannot numerically compare strings! \n{ex}");
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

                int Lval = Left switch
                {
                    LiteralExpression rexpr => int.Parse(rexpr.value),
                    VariableExpression rexpr => int.Parse(rexpr.Eval(Variables)),
                    _ => 0
                };

                int Rval = Right switch
                {
                    LiteralExpression rexpr => int.Parse(rexpr.value),
                    VariableExpression rexpr => int.Parse(rexpr.Eval(Variables)),
                    _ => 0
                };



                return Lval < Rval;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Cannot numerically compare strings! \n{ex}");
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

                int Lval = Left switch
                {
                    LiteralExpression rexpr => int.Parse(rexpr.value),
                    VariableExpression rexpr => int.Parse(rexpr.Eval(Variables)),
                    _ => 0
                };

                int Rval = Right switch
                {
                    LiteralExpression rexpr => int.Parse(rexpr.value),
                    VariableExpression rexpr => int.Parse(rexpr.Eval(Variables)),
                    _ => 0
                };



                return Lval >= Rval;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Cannot numerically compare strings! \n{ex}");
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

                int Lval = Left switch
                {
                    LiteralExpression rexpr => int.Parse(rexpr.value),
                    VariableExpression rexpr => int.Parse(rexpr.Eval(Variables)),
                    _ => 0
                };

                int Rval = Right switch
                {
                    LiteralExpression rexpr => int.Parse(rexpr.value),
                    VariableExpression rexpr => int.Parse(rexpr.Eval(Variables)),
                    _ => 0
                };



                return Lval <= Rval;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Cannot numerically compare strings! \n{ex}");
                return false;
            }
        }
    }


}
