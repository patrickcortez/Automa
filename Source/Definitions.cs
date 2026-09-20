using Automa.Source.Core;
using Automa.Source.Utility;
using System.Diagnostics;
using System.Text;

namespace Automa.Source
{
    internal enum PrintOptions // ConsoleWriteLine Wrapper
    {
        Normal,
        Warning,
        Error
    }

    internal enum VariableType
    {
        String,
        Int,

        Identifier
    }

    internal enum LexerType // Lexer Types
    {
        Token_LBrace, // {
        Token_RBrace, // }
        Token_SemiColon, // ;
        Token_Equal, // =
        Token_LParen, // (
        Token_RParen, // )
        Token_Comma, // ,
        Token_Add, // +
        Token_Minus, // -
        TokenString, // Dave123
        TokenInt, // 123
        Token_Not, // !
        TokenArith, // 2 + 2 - 2
        Token_Identifier, // Write,Read etc...
        Token_Multiply, // *
        Token_Divide, // \
        Token_KeyWord,
        Token_None // Default value;
    }

    //--- for arithmetic parser

    internal abstract record class ArithmeticNode { public abstract int Eval(IEnumerable<Variable> Scope); }

    internal record class NumberNode(int value) : ArithmeticNode
    {
        public override int Eval(IEnumerable<Variable>? Scope) => value;
    }

    internal record class VariableNode(string name) : ArithmeticNode
    {


        public override int Eval(IEnumerable<Variable>? Scope)
        {
            if(Scope is null)
            {
                throw new Exception("Current Scope not Set!");
            }

            Variable? result = Scope.FirstOrDefault(ex => ex.name == name);

            if(result == null)
            {
                throw new ArgumentNullException($"Variable {name} doesn't exist!");
            }

            if(result.type != VariableType.Int)
            {
                throw new Exception($"Variable {name} is not an integer!");
            }

            return int.Parse(result.value);
        }
    }

    internal record class BinaryOpNode(ArithmeticNode left, char Op, ArithmeticNode right) : ArithmeticNode
    {

        public override int Eval(IEnumerable<Variable> Scope) => Op switch
        {
            '+' => left.Eval(Scope) + right.Eval(Scope),
            '-' => left.Eval(Scope) - right.Eval(Scope),
            '*' => left.Eval(Scope) * right.Eval(Scope),
            '/' => left.Eval(Scope) / right.Eval(Scope),
            _ => throw new InvalidOperationException($"Unknown Operation {Op}")
        };
    }

    //---

    internal struct LexerToken // Lexer Token definition
    {
        public StringBuilder Content { get; set; } = new(); // Content Storage (Assignment , Argument & Expression values)
        public LexerType TokenType { get; set; }
        public readonly int Line { get; }

        public LexerToken(LexerType _Type,int _Line,string _Content="") // Constructor
        {
            Content.Append(_Content);
            TokenType = _Type;
            Line = _Line;
        }

        public void Append(string NewContent)
        {

            if(NewContent.Length == 0)
            {
                return;
            }

            Content.Append(NewContent);
        }

        public string GetContent()
        {
            return Content.ToString();
        }
    }

    internal enum AssignmentType
    {
        Variable,
        Run,
        Read
    }

    // Print Configuration definition
    internal record PrintConfiguration(PrintOptions option, bool newline);

    // Assigns

    internal abstract record Instruction
    {
        public Instruction? Next { get; set; }
    }
    internal abstract record AssignType;

    internal record VariableAssign(Variable variable) : AssignType;
    // Instructions
    internal record WriteInstruction(string Content,bool isIdent = false) : Instruction;

    internal record ReadAssign(string target,string Prompt) : AssignType;

    internal record AssignInstruction(AssignType type) : Instruction;
     
    internal record ArithmeticAssign(ArithmeticNode node,string target) : AssignType
    {
        public void Eval(IEnumerable<Variable> Scope)
        {
            Variable? Result = Scope.FirstOrDefault(e => e.name == target);

                int val = 0;

                switch (node)
                {
                    case NumberNode numberNode:

                        val = numberNode.value;

                        break;
                    case VariableNode varNode:

                        val = varNode.Eval(Scope);
                        break;

                    case BinaryOpNode binNode:
                        val = binNode.Eval(Scope);
                        break;
                }

            if(Result is null)
            {
                Scope.ToList().Add(new Variable(target, val.ToString()));
                return;
            }

            Result = Result with { value = val.ToString() };
        }
    }

    internal record Variable(string _name, string _value,VariableType _type = VariableType.String)
    {
       public string name { get; set; } = _name;
       public string value { get; set; } = _value;

       public VariableType type = _type;
    }

    internal abstract record Block : Instruction
    {
        public Instruction? Body { get; set; } = null;
    }

    

    internal record WhileBlock(Expression expr) : Block // While( expr ) { }
    {
        public Instruction? next { get; set; } = null;

        // Make execute block


    }

    internal record IfBlock(Expression expression, List<Variable> Variables) : Block // if(condition)
    {

        public List<Variable> Variable { get; set; } = Variables;

       

        public int ExecuteBlock()
        {
            Executor executor = new(Body, this.Variable); // replace new with Dody later...

            return  executor.Start();
        }
    }

    internal record Elif(Expression expression, List<Variable> Variables) : Block // elif(<condition>)
    {

        public List<Variable> Variable { get; set; } = Variables;

        public int ExecuteBlock()
        {
            Executor executor = new(Body, this.Variable); // replace new with Body Later...

            return  executor.Start();
        }
    }

    internal record Else( List<Variable> Variables) : Block
    {

        public List<Variable> Variable { get; set; } = Variables;
        public int ExecuteBlock()
        {
            Executor executor = new(Body, this.Variable);
            return executor.Start();
        }
    }

    // Expressions

    internal abstract record Expression(); // Soon to be added: >,<,>= and <=

    internal record VariableExpression(string VariableName) : Expression
    {
        private Variable? Value { get; set; }

        public Variable? GetVariable(List<Variable> Variables)
        {
            Value = Utils.FindVariable(VariableName, Variables);
            return Value;
        }
    }

    internal record LiteralExpression(string value) : Expression
    {
        public string Value { get; set; } = value;// well only have string data types anyways =P, so i dont have to cast later
    }

    internal record EqualTo(Expression left,Expression right) : Expression
    {
        private List<Variable> Variables = new();

        public void UpdateVariables(List<Variable> Updated)
        {
            Variables = Updated;
        }

        public bool Evaluate()
        {


            if(left is LiteralExpression LitLeft)
            {
                if(right is LiteralExpression LitRight)
                {
                    Variable? FindLeft = Utils.FindVariable(LitLeft.value, Variables);
                    Variable? FindRight = Utils.FindVariable(LitRight.value, Variables);

                    //Console.WriteLine("[Debug] Left value: {0} , Right value: {1}", FindLeft.value ?? LitLeft.value, FindRight.value ?? LitRight.value);

                    if (FindLeft != null && FindRight != null)
                    {
                        return FindLeft.value == FindRight.value;
                    }else if(FindLeft != null)
                    {
                        return FindLeft.value == LitRight.value;
                    }else if(FindRight != null)
                    {
                        return LitLeft.Value == FindRight.value;
                    }
                    else
                    {
                        return  LitLeft.value == LitRight.value;
                    }
                }
            }

            throw new Exception("Unknown Expression used!");
        }
    }

    internal record NotEqualTo(Expression left, Expression right) : Expression
    {

        private List<Variable> Variables = new();

        public void UpdateVariables(List<Variable> Updated)
        {
            Variables = Updated;
        }


        public bool Evaluate()
        {
            // Console.WriteLine("Debug: Left Type: {0} , Right Type: {1}", left.GetType(), right.GetType());

            if (left is LiteralExpression LitLeft)
            {
                if (right is LiteralExpression LitRight)
                {
                    Variable? FindLeft = Utils.FindVariable(LitLeft.value, Variables);
                    Variable? FindRight = Utils.FindVariable(LitRight.value, Variables);

                    if (FindLeft != null && FindRight != null)
                    {
                        return FindLeft.value != FindRight.value;
                    }
                    else if (FindLeft != null)
                    {
                        return FindLeft.value != LitRight.value;
                    }
                    else if (FindRight != null)
                    {
                        return LitLeft.Value != FindRight.value;
                    }
                    else
                    {
                        return LitLeft.value != LitRight.value;
                    }
                }
            }

            throw new Exception("Unknown Expression used!");
        }
    

}

    //Processes

    internal record RunAssignment((string Target,string Cmd) Properties) : AssignType
    {
        public string Run()
        {
            string[] cmdPart = Properties.Cmd.Split(' ', 2);
            string name = cmdPart[0];
            string args = cmdPart.Length > 1 ? cmdPart[1] : "";

            Process proc = new();
            proc.StartInfo = new()
            {
                FileName=name,
                Arguments=args,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            

            if (proc.Start()){
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                proc.OutputDataReceived += (_, e) =>
                {
                   if(e.Data != null)
                    {
                        //Do nothing
                    }
                };

                proc.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        //Do nothing
                    }
                };

                proc.WaitForExit();
                return $"{proc.ExitCode}";
            }

            return "1";
        }
    }

}

