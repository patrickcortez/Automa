using Automa.Source.Core;

namespace Automa.Source
{
    internal enum PrintOptions
    {
        Normal,
        Warning,
        Error
    }

    internal enum TokenType
    {
        Write,
        Read,
        If,
        EOP // End of Program
    }

    // Print Configuration definition
    internal record PrintConfiguration(PrintOptions option, bool newline);


    // Instructions
    internal record WriteInstruction(string Content);

    internal record ReadInstruction(string target,string Prompt);

    internal record Variable(string name, string value)
    {
       public string name { get; set; } = name;
       public string value { get; set; } = value;
    }

    internal record IfBlock(Expression expression, List<object> Instructions, List<Variable> Variables)
    {
        public List<object> Instructions { get; set; } = Instructions;

        public List<Variable> Variable { get; set; } = Variables;

        public int ExecuteIF()
        {
            Console.WriteLine("Debug: IF Instructions: {0}", this.Instructions.Count);
            Executor executor = new(this.Instructions, this.Variable);

            return executor.Start();
        }
    }

    // Expressions

    internal abstract record Expression();

    internal record VariableExpression(Variable value) : Expression
    {
        public Variable Value { get; set; } = value;
    }

    internal record LiteralExpression(string value) : Expression
    {
        public string Value { get; set; } = value;// well only have string data types anyways =P, so i dont have to cast later
    }

    internal record EqualTo(Expression left,Expression right) : Expression
    {
        public bool Evaluate()
        {


            Console.WriteLine("Debug: Left Type: {0} , Right Type: {1}", left.GetType(), right.GetType());

            if(left is VariableExpression Lexpr && right is VariableExpression Rexpr)
            {
                return Lexpr.Value.value == Rexpr.Value.value;
            }

            if(left is VariableExpression Lexpr2 && right is LiteralExpression Rexpr2)
            {
                Console.WriteLine("Debug: Values: {0} , {1}", Lexpr2.Value.value, Rexpr2.Value);
                return Lexpr2.Value.value == Rexpr2.Value;
            }

            if(left is LiteralExpression Lexpr3 && right is VariableExpression Rexpr3)
            {
                return Lexpr3.Value == Rexpr3.Value.value;
            }

            if(left is LiteralExpression Lexpr4 && right is LiteralExpression Rexpr4)
            {
                return Lexpr4.Value == Rexpr4.Value;
            }

            throw new Exception("Unknown Expression used");
        }
    }

    internal record NotEqualTo(Expression left, Expression right) : Expression
    {
        public bool Evaluate()
        {

            Console.WriteLine("Debug: Left Type: {0} , Right Type: {1}", left.GetType(), right.GetType());

            if (left is VariableExpression Lexpr && right is VariableExpression Rexpr)
            {
                return Lexpr.Value.value != Rexpr.Value.value;
            }

            if (left is VariableExpression Lexpr2 && right is LiteralExpression Rexpr2)
            {
                return Lexpr2.Value.value != Rexpr2.Value;
            }

            if (left is LiteralExpression Lexpr3 && right is VariableExpression Rexpr3)
            {
                return Lexpr3.Value != Rexpr3.Value.value;
            }

            if (left is LiteralExpression Lexpr4 && right is LiteralExpression Rexpr4)
            {
                return Lexpr4.Value != Rexpr4.Value;
            }

            throw new Exception("Unknown Expression used");
        }
    }

    //public

}
