using Automa.Source.Core;
using Automa.Source.Utility;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
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
        Token_GreaterThan, // >
        Token_LessThan, // <
        Token_Or, // ||
        Token_And, // &&
        Token_EqualTo, // ==
        Token_NotEqualTo, // !=
        Token_GTE, // >=
        Token_LTE, // <=
        Token_Pipe, // |
        Token_Ampersand, // &
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

        public LexerToken(LexerType _Type, int _Line, string _Content = "") // Constructor
        {
            Content.Append(_Content);
            TokenType = _Type;
            Line = _Line;
        }

        public void Append(string NewContent)
        {

            if (NewContent.Length == 0)
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
    internal record WriteInstruction(string Content, bool isIdent = false) : Instruction;

    internal record ReadAssign(string target, string Prompt) : AssignType;

    internal record AssignInstruction(AssignType type) : Instruction;

    internal record ArithmeticAssign(ArithmeticNode node, string target) : AssignType
    {
        public void UpdateScope(List<Variable> Scope)
        {
            Variable? Result = Scope.FirstOrDefault(e => e.name == target);

            int val = node switch
            {
                NumberNode n => n.value,
                VariableNode v => v.Eval(Scope),
                BinaryOpNode b => b.Eval(Scope),
                _ => 0
            };

            if (Result is null)
            {
                Scope.Add(new Variable(target, val.ToString(), VariableType.Int));
                return;
            }

            Result.value = val.ToString();
            Result.type = VariableType.Int;

        }
    }

    internal record Variable(string _name, string _value, VariableType _type = VariableType.String)
    {
        public string name { get; set; } = _name;
        public string value { get; set; } = _value;

        public VariableType type = _type;
    }

    internal abstract record Block : Instruction
    {
        public Instruction? Body { get; set; } = null;
    }



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
                Executor execute = new(Body,Scope);

                exitc = execute.Start();

                Scope.RemoveRange(before, Scope.Count - before); // filter out new
            }

            return exitc;
        }

    }

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

    // Logical Operators

    internal abstract record LogOp
    {
        public abstract bool Eval(List<Variable> Scope);
    }

    internal record LogicalUnit(Expression Node) : LogOp
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


            if(Result is null)
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

    internal abstract record Expression() // Soon to be added: >,<,>= and <=
    {
        public abstract bool Evaluate(List<Variable> Variables);
    }


    internal record EqualTo(Operand Left,Operand Right) : Expression
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

            return Lval == Rval;
        }
        
    }

    internal record NotEqualTo(Operand Left, Operand Right) : Expression
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

    internal record GreaterThan(Operand Left, Operand Right) : Expression
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
            catch(Exception ex)
            {
                Console.Error.WriteLine($"Cannot numerically compare strings! \n{ex}");
                return false;
            }
        }
    }

    internal record LessThan(Operand Left, Operand Right) : Expression
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

    internal record GTE(Operand Left, Operand Right) : Expression
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

    internal record LTE(Operand Left, Operand Right) : Expression
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

