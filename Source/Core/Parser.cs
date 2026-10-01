using Automa.Source.Definitions;
using System.ComponentModel.DataAnnotations;
using static Automa.Source.Utility.Utils;
using ExpOperand = (string Content, string Type);

// implement function call parsing in logical expression and assignment, =P

namespace Automa.Source.Core
{
    internal class Parser(LexerToken[] LexTok,bool isdebug = false)
    {
        string[] Keywords = ["Write", "Read", "If","Elif","Else","Run"];
        string[] LogicOps = ["||", "&&"];

        // Expression handling: logical or Arithmetic. Currently its Just Logical (for now)
        private LogOp? ParseExpression(List<LexerToken> Tokens)
        {
            try
            {

                LogOp? expr = null;

                bool hasLogicalOps = false;

                string Operator = "",LogicOp="",Funcname="";

                List<LexerToken>? NextUnit = null;
                List<Parameter>? Args = new(),Pargs=new();

                void SetNext(List<LexerToken> Trimmed,LexerType type)
                {
                    NextUnit = Trimmed.ToList();

                    LogicOp = (type is LexerType.Token_And) ? "And" : "Or";
                }

                ExpOperand left = new(),right=new();

                
                LexerType PrevType = LexerType.Token_None;
                if (isdebug)
                {
                    Console.WriteLine("\n[Debug] Current Expression (total tokens in expr: {0}):", Tokens.Count);
                }
                for(int i = 0; i < Tokens.Count;i++)
                {
                    LexerToken Current = Tokens[i];

                    LexerToken? Next = null;

                    LexerType CT = Current.TokenType;

                    if(i < Tokens.Count - 1)
                    {
                        Next = Tokens[i + 1];
                    }

                    if (isdebug)
                    {
                        Console.Write(", {0} ,", CT.ToString());
                    }

                    if(CT is LexerType.Token_KeyWord)
                    {
                        PrevType = CT;

                        string value = Current.GetContent();

                        if(Operator.Length is 0)
                        {
                            left = (value, "Bool");
                        }
                        else
                        {
                            right = (value, "Bool");
                        }

                        continue;
                    }

                    if (CT is LexerType.Token_Identifier)
                    {
                        PrevType = CT;

                        if(Next.Value.TokenType is LexerType.Token_LParen)
                        {
                            Funcname = Current.GetContent();
                            continue;
                        }

                        if (Operator.Length is 0)
                        {
                            left = (Current.GetContent(), "Identifier");
                        }
                        else
                        {
                            right = (Current.GetContent(), "Identifier");
                        }

                        continue;
                    }
                    else if (CT is LexerType.Token_Not)
                    {
                        PrevType = CT;
                        continue;
                    }
                    else if (CT is LexerType.Token_EqualTo or LexerType.Token_NotEqualTo)
                    {
                        Operator = (CT is LexerType.Token_EqualTo)?"EQ":"NEQ";
                        PrevType = CT;
                        continue;
                    } 
                    else if (CT is LexerType.Token_GreaterThan or LexerType.Token_GTE) 
                    {
                        Operator = (CT is LexerType.Token_GreaterThan)?"GT":"GTE";
                        PrevType = CT;
                        continue;
                    } 
                    else if(CT is LexerType.Token_LessThan or LexerType.Token_LTE)
                    {
                        Operator = (CT is LexerType.Token_LessThan)?"LT":"LTE";
                        PrevType = CT;
                        continue;
                    }
                    else if (CT is LexerType.Token_And or LexerType.Token_Or)
                    {
                        hasLogicalOps = true;
                        int skip = Tokens.IndexOf(Current) + 1;
                        List<LexerToken> newList = Tokens.Skip(skip).ToList();

                        SetNext(newList, CT);
                        break;
                    }
                    else if (CT is LexerType.TokenString or LexerType.TokenInt)
                    {
                        string content = Current.GetContent();
                        string type = (CT is LexerType.TokenString) ? "String" : "Int";

                        if (Operator.Length is 0)
                        {
                            left = (Current.GetContent(), type);
                        }
                        else
                        {
                            right = (Current.GetContent(), type);
                        }

                    }else if(CT is LexerType.Token_LParen)
                    {
                        int skip = 0;

                        if(Args.Count is not 0)
                        {
                            Pargs = new(Args);
                            Args.Clear();
                        }

                        while(Tokens[i + 1 + skip].TokenType is not LexerType.Token_RParen)
                        {
                            LexerToken? Node = Tokens[i + 1 + skip];
                            string content = Node.Value.GetContent();
                            LexerType NPM = Node.Value.TokenType;

                            if(NPM is LexerType.Token_Comma)
                            {
                                skip++;
                                continue;
                            }

                            if(NPM is LexerType.Token_Identifier)
                            {
                                Args.Add(new(new AutomaString(content), VariableType.Identifier)); 
                            }else if(NPM is LexerType.TokenInt)
                            {
                                Args.Add(new(new AutomaString(content), VariableType.Int));
                            }else if(NPM is LexerType.TokenString)
                            {
                                Args.Add(new(new AutomaString(content), VariableType.String));
                            }else if(NPM is LexerType.TokenBool)
                            {
                                Args.Add(new(new AutomaString(content), VariableType.Boolean));
                            }

                            skip++;
                        }

                        if(Operator.Length is 0)
                        {
                            left = (Funcname, "FunctionCall");
                        }
                        else
                        {
                            right = (Funcname, "FunctionCall");
                        }

                        if(skip > 0)
                        {
                            i += skip;
                        }
                    }
                    else
                    {
                        throw new Exception($"Invalid Token in Expression: {CT}, in Line: {Current.Line}");
                    }


                }

                string lcontent = left.Content, rcontent = right.Content;

                if (Operator.Length is 0)
                {
                    throw new Exception("Missing Operator, must have an operator!");
                }

                if (Operator == "EQ")
                {
                   
                    EqualTo eq = new EqualTo(new LiteralExpression(new AutomaString(lcontent)), new LiteralExpression( new AutomaString(rcontent)));



                    if(left.Type is "FunctionCall")
                    {
                        eq = eq with { Left = new FunctionExpression(new AutomaFunction(left.Content, Pargs)) };
                    }

                    if(right.Type is "FunctionCall")
                    {
                        eq = eq with { Right = new FunctionExpression(new AutomaFunction(right.Content, Args)) };
                    }

                    if(left.Type is "Identifier")
                    {
                        eq = eq with { Left = new VariableExpression(lcontent) };
                    }


                    if(right.Type is "Identifier")
                    {
                        eq = eq with { Right = new VariableExpression(rcontent) };
                    }

                    if (!hasLogicalOps)
                    {
                        expr = new LogicalUnit(eq);
                    }
                    else
                    {
                        if(LogicOp is "And")
                        {

                            LogOp Current = new LogicalUnit(eq);

                            expr = new And(Current, ParseExpression(NextUnit));
                        }else if(LogicOp is "Or")
                        {
                            LogOp Current = new LogicalUnit(eq);

                            expr = new Or(Current, ParseExpression(NextUnit));
                        }
                    }

                }
                else if (Operator == "NEQ")
                {
                    NotEqualTo nexpr = new NotEqualTo(new LiteralExpression(new AutomaString(lcontent)), new LiteralExpression(new AutomaString(rcontent)));

                    if (left.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Left = new FunctionExpression(new AutomaFunction(left.Content, Pargs)) };
                    }

                    if (right.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Right = new FunctionExpression(new AutomaFunction(right.Content, Args)) };
                    }

                    if (left.Type is "Identifier")
                    {
                        nexpr = nexpr with { Left = new VariableExpression(lcontent) };
                    }


                    if (right.Type is "Identifier")
                    {
                        nexpr = nexpr with { Right = new VariableExpression(rcontent) };
                    }

                    if (!hasLogicalOps)
                    {
                        expr = new LogicalUnit(nexpr); // Single Logical Unit
                    }
                    else
                    {
                        LogOp Current = new LogicalUnit(nexpr);

                        if (LogicOp is "And")
                        {
                            expr = new And(Current, ParseExpression(NextUnit));
                        }
                        else if (LogicOp is "Or")
                        {
                            expr = new Or(Current, ParseExpression(NextUnit));
                        }
                    }
                }else if(Operator is "GT")
                {
                    GreaterThan nexpr = new GreaterThan(new LiteralExpression(new AutomaString(lcontent)), new LiteralExpression(new AutomaString(rcontent)));

                    if (left.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Left = new FunctionExpression(new AutomaFunction(left.Content, Pargs)) };
                    }

                    if (right.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Right = new FunctionExpression(new AutomaFunction(right.Content, Args)) };
                    }

                    if (left.Type is "Identifier")
                    {
                        nexpr = nexpr with { Left = new VariableExpression(lcontent) };
                    }


                    if (right.Type is "Identifier")
                    {
                        nexpr = nexpr with { Right = new VariableExpression(rcontent) };
                    }

                    if (!hasLogicalOps)
                    {
                        expr = new LogicalUnit(nexpr); // Single Logical Unit
                    }
                    else
                    {
                        LogOp Current = new LogicalUnit(nexpr);

                        if (LogicOp is "And")
                        {
                            expr = new And(Current, ParseExpression(NextUnit));
                        }
                        else if (LogicOp is "Or")
                        {
                            expr = new Or(Current, ParseExpression(NextUnit));
                        }
                    }
                }
                else if (Operator is "LT")
                {
                    LessThan nexpr = new LessThan(new LiteralExpression(new AutomaString(lcontent)), new LiteralExpression(new AutomaString(rcontent)));

                    if (left.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Left = new FunctionExpression(new AutomaFunction(left.Content, Pargs)) };
                    }

                    if (right.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Right = new FunctionExpression(new AutomaFunction(right.Content, Args)) };
                    }

                    if (left.Type is "Identifier")
                    {
                        nexpr = nexpr with { Left = new VariableExpression(lcontent) };
                    }


                    if (right.Type is "Identifier")
                    {
                        nexpr = nexpr with { Right = new VariableExpression(rcontent) };
                    }

                    if (!hasLogicalOps)
                    {
                        expr = new LogicalUnit(nexpr); // Single Logical Unit
                    }
                    else
                    {
                        LogOp Current = new LogicalUnit(nexpr);

                        if (LogicOp is "And")
                        {
                            expr = new And(Current, ParseExpression(NextUnit));
                        }
                        else if (LogicOp is "Or")
                        {
                            expr = new Or(Current, ParseExpression(NextUnit));
                        }
                    }
                }
                else if (Operator is "GTE")
                {
                    GTE nexpr = new GTE(new LiteralExpression(new AutomaString(lcontent)), new LiteralExpression(new AutomaString(rcontent)));

                    if (left.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Left = new FunctionExpression(new AutomaFunction(left.Content, Pargs)) };
                    }

                    if (right.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Right = new FunctionExpression(new AutomaFunction(right.Content, Args)) };
                    }

                    if (left.Type is "Identifier")
                    {
                        nexpr = nexpr with { Left = new VariableExpression(lcontent) };
                    }


                    if (right.Type is "Identifier")
                    {
                        nexpr = nexpr with { Right = new VariableExpression(rcontent) };
                    }

                    if (!hasLogicalOps)
                    {
                        expr = new LogicalUnit(nexpr); // Single Logical Unit
                    }
                    else
                    {
                        LogOp Current = new LogicalUnit(nexpr);

                        if (LogicOp is "And")
                        {
                            expr = new And(Current, ParseExpression(NextUnit));
                        }
                        else if (LogicOp is "Or")
                        {
                            expr = new Or(Current, ParseExpression(NextUnit));
                        }
                    }
                }
                else if (Operator is "LTE")
                {
                    LTE nexpr = new LTE(new LiteralExpression(new AutomaString(lcontent)), new LiteralExpression(new AutomaString(rcontent)));

                    if (left.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Left = new FunctionExpression(new AutomaFunction(left.Content, Pargs)) };
                    }

                    if (right.Type is "FunctionCall")
                    {
                        nexpr = nexpr with { Right = new FunctionExpression(new AutomaFunction(right.Content, Args)) };
                    }

                    if (left.Type is "Identifier")
                    {
                        nexpr = nexpr with { Left = new VariableExpression(lcontent) };
                    }


                    if (right.Type is "Identifier")
                    {
                        nexpr = nexpr with { Right = new VariableExpression(rcontent) };
                    }

                    if (!hasLogicalOps)
                    {
                        expr = new LogicalUnit(nexpr); // Single Logical Unit
                    }
                    else
                    {
                        LogOp Current = new LogicalUnit(nexpr);

                        if (LogicOp is "And")
                        {
                            expr = new And(Current, ParseExpression(NextUnit));
                        }
                        else if (LogicOp is "Or")
                        {
                            expr = new Or(Current, ParseExpression(NextUnit));
                        }
                    }
                }

                return expr;
            }catch(Exception ex)
            {
                Console.Error.WriteLine("Parser Expression Error: {0}", ex);
                return null;
            }
        }

        // will refactor later =P
        private T? ParseStatement<T>(LexerToken Starting,out int tokensConsumed, LexerType Ending = LexerType.Token_RBrace,string FN="",List<Variable>? args=null)
        {
            try
            {
                NestBuilder Bob = new();
                Type type = typeof(T);
                List<LexerToken> expression = new();
                int StartingIndex = LexTok.IndexOf(Starting);
                LogOp? expr = null;
                LexerType? PrevTok = null;
                bool inBlock = false,
                        inParen = false,
                        parseExpression = false,
                        isAssign = false,
                        IsReturn = false;
                int depth = 0, pdepth = 0; // brace depth and parenthesis depth

                bool isArith = false;

                string CurrentInstruction = "";
                (string value, string type) CurrentContent = ("", "");

                List<LexerToken> Toks = LexTok.Skip(StartingIndex).ToList();
                List<LexerToken> ArithmeticTokens = new();
                LexerToken? Peek = null;
                Return? CR = null;

                if (isdebug)
                {
                    Console.WriteLine("[Debug] Statement Tokens (Statement Token total: {0}):", Toks.Count);
                }

                int tc = 0;

                // Iterate through all the tokens
                for (int i = 0; i < Toks.Count; i++)
                {
                    LexerToken Current = Toks[i];
                    LexerType CurrentType = Current.TokenType;

                    tc++;

                    if (isdebug)
                    {
                        Console.WriteLine("{0}", CurrentType.ToString());
                    }

                    int peekIndex = i + 1;



                    if (peekIndex < Toks.Count)
                    {
                        Peek = Toks[i + 1];

                        // Arithmetic toggler
                        if ((Peek.Value.TokenType is LexerType.Token_Add or LexerType.Token_Minus or LexerType.Token_Multiply or LexerType.Token_Divide && CurrentType is LexerType.TokenInt or LexerType.Token_Identifier && isAssign && !isArith) || (Peek.Value.TokenType is LexerType.TokenInt or LexerType.Token_Identifier && CurrentType is LexerType.Token_LParen && isAssign && !isArith)) // 2 + or -
                        {
                            isArith = true;
                        }
                    }

                    // Expression Handling
                    if (parseExpression && CurrentType is not LexerType.Token_RParen && depth is 0)
                    {
                        expression.Add(Current);
                        continue;
                    }
                    else if (parseExpression && CurrentType is LexerType.Token_RParen && depth is 0)
                    {
                        expr = ParseExpression(expression);
                        parseExpression = false;
                        continue;
                    }




                    // Determine if we're entering an Expression
                    if ((inBlock && PrevTok is LexerType.Token_KeyWord) && CurrentType is LexerType.Token_LParen && !parseExpression && depth is 0)
                    {
                        parseExpression = true;
                        continue;
                    }

                    if (isArith) // Arithmetic Assignment Handler
                    {
                        if (isAssign)
                        {
                            if (CurrentType is LexerType.TokenInt or LexerType.Token_Add or LexerType.Token_LParen or LexerType.Token_RParen or LexerType.Token_Minus or LexerType.Token_Multiply or LexerType.Token_Divide or LexerType.Token_Identifier)
                            {
                                ArithmeticTokens.Add(Current);
                                continue;
                            }
                        }
                    }


                    if (CurrentType is LexerType.Token_KeyWord) // keyword handling: if,elif,else and etc...
                    {
                        PrevTok = CurrentType;
                        string keyword = Current.GetContent();

                        if (keyword is "If" or "Elif" or "Else")
                        {

                            if (!inBlock)
                            {
                                inBlock = true;
                                continue;
                            }
                            else
                            {

                                if (isdebug)
                                {
                                    Console.WriteLine($"[DEBUG] Parsing Nested Block: {keyword}");
                                }

                                if (keyword is "If")
                                {
                                    Block? parsedBlock = ParseStatement<IfBlock>(Current, out int skip);

                                    if (isdebug)
                                    {
                                        Console.WriteLine($"[DEBUG] with block instruction count: {skip}");
                                    }

                                    if (parsedBlock is null)
                                    {
                                        throw new Exception($"Malformed block at line: {Current.Line}");
                                    }

                                    Bob.AddNode(parsedBlock);

                                    if (skip > 0)
                                    {
                                        i += (skip - 1);
                                        tc += (skip - 1);
                                    }
                                }
                                else if (keyword is "Elif")
                                {
                                    Block? parsedBlock = ParseStatement<Elif>(Current, out int skip);

                                    if (isdebug)
                                    {
                                        Console.WriteLine($"[DEBUG] with block instruction count: {skip}");
                                    }

                                    if (parsedBlock is null)
                                    {
                                        throw new Exception($"Malformed block at line: {Current.Line}");
                                    }

                                    Bob.AddNode(parsedBlock);

                                    if (skip > 0)
                                    {
                                        i += (skip - 1);
                                        tc += (skip - 1);
                                    }
                                }
                                else if (keyword is "Else")
                                {
                                    Block? parsedBlock = ParseStatement<Else>(Current, out int skip);

                                    if (parsedBlock is null)
                                    {
                                        throw new Exception($"Malformed block at line: {Current.Line}");
                                    }

                                    Bob.AddNode(parsedBlock);

                                    if (skip > 0)
                                    {
                                        i += (skip - 1);
                                        tc += (skip - 1);
                                    }
                                }

                                continue;
                            }

                        }
                        else if (keyword is "While")
                        {
                            if (!inBlock)
                            {
                                inBlock = true;
                                continue;
                            }
                            else
                            {
                                Block? parsedBlock = ParseStatement<WhileBlock>(Current, out int skip);

                                if (parsedBlock is null)
                                {
                                    throw new Exception($"Malformed block at line: {Current.Line}");
                                }

                                Bob.AddNode(parsedBlock);

                                if (skip > 0)
                                {
                                    i += (skip - 1);
                                    tc += (skip - 1);
                                }
                            }

                        }else if(keyword is "Return")
                        {
                            CurrentInstruction = keyword;
                            IsReturn = true;
                            continue;
                        }else if(keyword is "True" or "False")
                        {
                            if (isAssign)
                            {
                                CurrentContent = (keyword, "bool");
                                continue;
                            }
                            
                        }

                        if (isAssign || inParen || IsReturn)
                        {
                            if (keyword is "Read" or "Run")
                            {
                                CurrentContent.type = keyword;
                                continue;
                            }
                            else if (keyword is "Write")
                            {
                                throw new Exception($"Cannot use write as an assign type! at {Current.Line}");
                            }
                            continue;
                        }
                        else if (!isAssign && !IsReturn)
                        {
                            CurrentInstruction = keyword;
                            continue;
                        }

                    }

                    if (CurrentType is LexerType.Token_Identifier) // Instructions or variable decl
                    {
                        if (depth > 1)
                        {
                            continue;
                        }
                        PrevTok = CurrentType;
                        string content = Current.GetContent();

                        if (inParen && isAssign)
                        {
                            throw new Exception($"Cannot assign inside parenthesis, at line: {Current.Line}");
                        }


                        if (isAssign || inParen || IsReturn) // if identifier is on right side or on a parenthesis
                        {

                            CurrentContent = (content, "identifier");
                            continue;
                        }
                        else // left side, possible variable decl
                        {
                            CurrentInstruction = content;
                            continue;
                        }


                    }
                    else if (CurrentType is LexerType.Token_Equal)
                    {
                        if (depth > 1)
                        {
                            continue;
                        }


                        if (PrevTok is LexerType.Token_Identifier)
                        {
                            isAssign = true;
                            PrevTok = CurrentType;
                            continue;
                        }
                        else if (PrevTok is LexerType.Token_Equal)
                        {
                            isAssign = false;
                            PrevTok = CurrentType;
                            continue;
                        }
                        else if (PrevTok is LexerType.Token_Not)
                        {
                            isAssign = false;
                            PrevTok = CurrentType;
                            continue;
                        }

                        throw new Exception($"Invalid assignment usage at line {Current.Line}");
                    }
                    else if (CurrentType is LexerType.Token_LParen) // (
                    {
                        if (depth > 1)
                        {
                            continue;
                        }

                        PrevTok = CurrentType;
                        if (!inParen)
                        {
                            inParen = true;
                            continue;
                        }
                        else
                        {
                            if (CurrentContent.type is "Run" or "Read")
                            {
                                throw new Exception($"Cannot have one or more parenthesis in Assign types Run or Read at Line {Current.Line}");
                            }

                            pdepth++;
                            continue;
                        }
                    }
                    else if (CurrentType is LexerType.Token_RParen) // )
                    {
                        if (depth > 1)
                        {
                            continue;
                        }

                        PrevTok = CurrentType;
                        if (!inParen)
                        {
                            throw new Exception($"Missing Left parenthesis in Line {Current.Line}");
                        }

                        if (pdepth > 0)
                        {
                            pdepth--;
                            continue;
                        }
                        else
                        {
                            inParen = false;
                            continue;
                        }


                    }
                    else if (CurrentType is LexerType.TokenString or LexerType.TokenInt) // "abc" or 123
                    {
                        if (depth > 1)
                        {
                            continue;
                        }

                        if (!isAssign && !inParen)
                        {
                            throw new Exception($"Cannot assign value to Literals at line {Current.Line}");
                        }


                        if ((inParen && CurrentContent.type is "Run" or "Read")) // assuming the next is a R paren ')'
                        {
                            CurrentContent.value = Current.GetContent(); // store the arg of Run and Read
                            continue;
                        }

                        if (CurrentType is LexerType.TokenInt) // int 
                        {
                            CurrentContent = (Current.GetContent(), "int");
                            continue;
                        }

                        CurrentContent = (Current.GetContent(), "string"); // string
                        continue;
                    }
                    else if (CurrentType is LexerType.Token_SemiColon) // ;
                    {
                        if (depth > 1)
                        {
                            continue;
                        }

                        if (CurrentInstruction is "Write") // Instruction: Write  (STDOUT)
                        {
                            if (CurrentContent.type is "identifier")
                            {
                                Bob.AddNode(new WriteInstruction(CurrentContent.value, true));
                                continue;
                            }

                            Bob.AddNode(new WriteInstruction(CurrentContent.value));

                            // reset
                            CurrentContent = ("", "");
                            CurrentInstruction = "";
                            isAssign = false;

                            continue;
                        }
                        else if (CurrentInstruction is "Read")
                        {
                            Bob.AddNode(new AssignInstruction(new ReadAssign("null", CurrentContent.value)));



                            // reset
                            CurrentContent = ("", "");
                            CurrentInstruction = "";
                            isAssign = false;

                            continue;
                        }
                        else if (CurrentInstruction is "Run")
                        {
                            Bob.AddNode(new AssignInstruction(new RunAssignment(("null", CurrentContent.value))));

                            // reset
                            CurrentContent = ("", "");
                            CurrentInstruction = "";
                            isAssign = false;

                            continue;
                        }else if(CurrentInstruction is "Return")
                        {
                            string Rtype = CurrentContent.type;

                            if(Rtype is "int")
                            {
                                CR = new(new AutomaInteger(int.Parse(CurrentContent.value)), VariableType.Int);
                            } else if(Rtype is "string")
                            {
                                CR = new(new AutomaString(CurrentContent.value), VariableType.String);
                            }else if(Rtype is "bool")
                            {
                                CR = new(new AutomaBoolean(bool.Parse(CurrentContent.value)), VariableType.Boolean);
                            }else if(Rtype is "identifier")
                            {
                                CR = new(new AutomaString(CurrentContent.value), VariableType.Identifier);
                            }

                            Bob.AddNode(new ReturnInstruction(CR));

                            continue;
                        }
                        else
                        {


                            string varname = CurrentInstruction;

                            if (CurrentContent.type is "Read")
                            {
                                Bob.AddNode(new AssignInstruction(new ReadAssign(varname, CurrentContent.value)));

                                // reset
                                CurrentContent = ("", "");
                                CurrentInstruction = "";
                                isAssign = false;

                                continue;
                            }
                            else if (CurrentContent.type is "Run")
                            {
                                Bob.AddNode(new AssignInstruction(new RunAssignment((varname, CurrentContent.value))));

                                // reset
                                CurrentContent = ("", "");
                                CurrentInstruction = "";
                                isAssign = false;

                                continue;
                            }

                            VariableType _type = VariableType.String;

                            if (CurrentContent.type is "int")
                            {
                                _type = VariableType.Int;
                            }
                            else if (CurrentContent.type is "identifier")
                            {
                                _type = VariableType.Identifier;
                            }

                            if (isArith)
                            {
                                Arithmetic arith = new(ArithmeticTokens, isdebug);

                                Bob.AddNode(new AssignInstruction(new ArithmeticAssign(arith.ParseArithmetic(out int _), varname)));
                            }
                            else
                            {
                                IValue? value = null;

                                if (_type is VariableType.Int)
                                {
                                    value = new AutomaInteger(int.Parse(CurrentContent.value));
                                }else if(_type is VariableType.String)
                                {
                                    value = new AutomaString(CurrentContent.value);
                                }

                                Bob.AddNode(new AssignInstruction(new VariableAssign(new(varname, value , _type))));
                            }


                            // reset
                            if (ArithmeticTokens.Count > 0)
                            {
                                ArithmeticTokens.Clear();
                            }

                            CurrentContent = ("", "");
                            CurrentInstruction = "";
                            isAssign = false;
                            continue;

                        }

                    }
                    else if (CurrentType is LexerType.Token_LBrace)
                    {
                        if (inBlock) // depth always starts at 1;
                        {
                            depth++;
                            continue;
                        }
                    }
                    else if (CurrentType is LexerType.Token_RBrace)
                    {
                        if (depth > 1)
                        {
                            depth--;
                            continue;
                        }

                        break; // if depth reaches 1
                    }
                    else if (CurrentType is LexerType.Token_Not)
                    {
                        PrevTok = CurrentType;
                        continue;
                    }
                }

                tokensConsumed = tc;

                if (type == typeof(IfBlock))
                {
                    IfBlock block = new IfBlock(expr);
                    block.Body = Bob.Build();

                    return (T)(object)block;
                }
                else if (type == typeof(Elif))
                {
                    Elif block = new Elif(expr);
                    block.Body = Bob.Build();

                    return (T)(object)block;
                }
                else if (type == typeof(Else))
                {
                    Else block = new Else();
                    block.Body = Bob.Build();

                    return (T)(object)block;
                }
                else if (type == typeof(WhileBlock))
                {
                    WhileBlock block = new WhileBlock(expr);
                    block.Body = Bob.Build();

                    return (T)(object)block;
                }
                else if (type == typeof(Function))
                {
                    Function fn = new(CR ,args);
                    fn.Body = Bob.Build();

                    return (T)(object)fn;
                }

                return (T)(object)null; // this is basically unrecheable but we have to return something -_-.
            }catch(Exception ex)
            {
                Console.Error.WriteLine("Parser Statement Error: {0}",ex);
                tokensConsumed = 0;
                return (T)(object)null;
            }
        }


        private Instruction Parse()
        {
            try
            {
                (string content,string type) CC =( "", ""); // Current Content
                bool inBlock = false,
                    inParen = false,
                    validParen = false,
                    parseExpression = false,
                    isArith = false,
                    parseArgs = false,
                    inFunc = false,
                    isAssign = false;
                int depth = 0, pdepth = 0; // if-block depth and parenthesis depth
                string CI = "None"; // Current Instruction,
                LexerType prevTok = LexerType.Token_None;
                string  CB = "",FN=""; // Logical operator and Current Block

                // Arithmetic tokens for Arithmetic parser;
                List<LexerToken> ArithmeticTokens = new();
                List<Parameter> args = new();

                List<Variable> AddArgs()
                {
                    List<Variable> nlist = new();
                    foreach(Parameter arg in args)
                    {

                        if(arg.type is VariableType.String or VariableType.Identifier)
                        {
                            nlist.Add(new((arg.value is AutomaString str) ? str.value : throw new Exception("Arg name can only be a string"), new AutomaString(""), arg.type));
                        }else if(arg.type is VariableType.Int)
                        {
                            nlist.Add(new((arg.value is AutomaString str) ? str.value : throw new Exception("Arg name can only be a string"), new AutomaInteger(0), arg.type));
                        }else if(arg.type is VariableType.Boolean)
                        {
                            nlist.Add(new((arg.value is AutomaString str) ? str.value : throw new Exception("Arg name can only be a string"), new AutomaBoolean(false), arg.type));
                        }

                    }

                    return nlist;
                }

                List<Parameter> AddParams()
                {
                    List<Parameter> nlist = new();
                    foreach (Parameter arg in args)
                    {
                        nlist.Add(new(arg.value,arg.type));
                    }

                    return nlist;
                }

                LexerToken[] _Tokens = LexTok;
                LogOp? expr = null;
                LexerToken? Peek = null;


                List<LexerToken> expression = new();

                if (isdebug)
                {
                    Console.WriteLine("[Debug] TopLevel Statement (Total Tokens: {0}):",_Tokens.Length);
                }

                void ArithReset()
                {
                    isArith = false;
                    ArithmeticTokens.Clear();

                    CI = string.Empty;
                    CC = ("", "");
                    validParen = false;
                    isAssign = false;
                }

                for (int i = 0; i < _Tokens.Length;i++)
                {
                    LexerToken Current = _Tokens[i];
                    LexerType CT = Current.TokenType;
                    int peekIndex = i + 1;

                    if ( peekIndex < _Tokens.Length)
                    {
                        Peek = _Tokens[i + 1];

                        // Arithmetic toggler
                        if((Peek.Value.TokenType is LexerType.Token_Add or LexerType.Token_Minus or LexerType.Token_Multiply or LexerType.Token_Divide && CT is LexerType.TokenInt or LexerType.Token_Identifier && isAssign && !isArith) || (Peek.Value.TokenType is LexerType.TokenInt or LexerType.Token_Identifier && CT is LexerType.Token_LParen && isAssign && !isArith && FN is "")) // 2 + or -, might change the Fn is "" toggle
                        {
                            isArith = true;
                        }
                    }

                    if (isdebug)
                    {
                        Console.WriteLine("[Debug] Current Token: {0}",CT.ToString());
                    }

                    if(parseArgs && CT is not LexerType.Token_RParen)
                    {
                        string val = Current.GetContent();

                        if (CT is LexerType.Token_Identifier)
                        {
                            args.Add(new(new AutomaString(val),VariableType.Identifier));
                        } else if (CT is LexerType.TokenInt or LexerType.TokenString) {
                            args.Add(new(new AutomaString(val), (CT is LexerType.TokenInt) ? VariableType.Int : VariableType.String));
                        } else if (CT is LexerType.Token_Comma)
                        {
                            continue;
                        }
                        else
                        {
                            throw new Exception($"Invalid token {CT} in args, at line {Current.Line}");
                        }
                    }

                    // Expression Handling

                    if (CT is LexerType.Token_RParen && (parseExpression && depth is 0))
                    {

                        expr = ParseExpression(expression); // Determine Expression

                        if (CB is "If") // add Block nodes before  the next token comes;
                        {
                            NodeBuilder.AddNode(new IfBlock(expr));
                        }
                        else if (CB is "Elif")
                        {
                            NodeBuilder.AddNode(new Elif(expr));
                        }else if(CB is "While")
                        {
                            NodeBuilder.AddNode(new WhileBlock(expr));
                        }

                        parseExpression = false;
                        expression.Clear();
                        continue;
                    }
                    

                    if (parseExpression && CT is not LexerType.Token_RParen && depth is 0)
                    {
                        expression.Add(Current);
                        continue;
                    }

                    if(isArith) // Arithmetic Assignment Handler
                    {
                        if (isAssign)
                        {
                            if(CT is LexerType.TokenInt or LexerType.Token_Add or LexerType.Token_LParen or LexerType.Token_RParen or LexerType.Token_Minus or LexerType.Token_Multiply or LexerType.Token_Divide or LexerType.Token_Identifier)
                            {
                                ArithmeticTokens.Add(Current);
                                continue;
                            }
                        }
                    }

                    // Expression Parsing toggler
                    if((inBlock && prevTok is LexerType.Token_KeyWord) && CT is LexerType.Token_LParen && !parseExpression && depth is 0) 
                    {
                        parseExpression = true;
                        continue;
                    }

                    if(FN != string.Empty && CB is "Function")
                    {
                        if(CT == LexerType.Token_LParen)
                        {
                            parseArgs = true;
                            continue;
                        }else if(CT is LexerType.Token_RParen)
                        {
                            parseArgs = false;

                            continue;
                        }
                    }

                    // Check Tokens

                    // Handle keywords: if,elif,else and etc...
                    if(CT is LexerType.Token_KeyWord) 
                    {
                        string keyword = Current.Content.ToString();
                        prevTok = CT;

                        if (isdebug)
                        {
                            Console.WriteLine($"[DEBUG] current keyword: {keyword}");
                        }

                        if (keyword is "If" or "Elif" or "Else") // Conditional
                        {
                            if (inBlock && depth == 1)
                            {

                                if (isdebug)
                                {
                                    Console.WriteLine($"[DEBUG] Parsing Nested {keyword}");
                                }

                                if (keyword is "If")
                                {
                                    NodeBuilder.AddNode(ParseStatement<IfBlock>(Current, out int tokenConsumed), true);

                                    if (isdebug)
                                    {
                                        Console.WriteLine($"[DEBUG] With {tokenConsumed} instructions");
                                    }

                                    if (tokenConsumed > 0) i += (tokenConsumed - 1); // Jump to the end of the block
                                }
                                else if (keyword is "Elif")
                                {
                                    NodeBuilder.AddNode(ParseStatement<Elif>(Current, out int tokenConsumed), true);

                                    if (isdebug)
                                    {
                                        Console.WriteLine($"[DEBUG] With {tokenConsumed} instructions");
                                    }

                                    if (tokenConsumed > 0) i += (tokenConsumed - 1); // Jump to the end of the block
                                } else if (keyword is "Else")
                                {
                                    NodeBuilder.AddNode(ParseStatement<Else>(Current, out int tokenConsumed), true);

                                    if (isdebug)
                                    {
                                        Console.WriteLine($"[DEBUG] With {tokenConsumed} instructions");
                                    }

                                    if (tokenConsumed > 0) i += (tokenConsumed - 1); // Jump to the end of the block
                                }
                            }
                            else
                            {
                                CB = keyword;
                                inBlock = true;

                                if (keyword is "Else")
                                {
                                    NodeBuilder.AddNode(new Else());
                                }

                                continue;

                            }
                        }
                        else if (keyword is "While")
                        {
                            if (inBlock && depth is 1)
                            {
                                NodeBuilder.AddNode(ParseStatement<WhileBlock>(Current, out int tokenConsumed),true);

                                if (isdebug)
                                {
                                    Console.WriteLine($"[DEBUG] With {tokenConsumed} instructions");
                                }

                                if (tokenConsumed > 0) i += (tokenConsumed - 1); // Jump to the end of the block
                            }
                            else if(!inBlock && depth is 0)
                            {
                                CB = keyword;
                                inBlock = true;
                            }

                            continue;
                        }else if(keyword is "Function")
                        {
                            if (!inBlock)
                            {
                                inBlock = true;
                                inFunc = true;
                                CB = keyword;
                            }else if (inBlock)
                            {
                                // inblock function-call implementation, will do later =P
                            }

                            continue;
                        }
                        else // Read, Write and Run
                        {

                            if (isAssign || inParen)
                            {
                                if (keyword is "Run" or "Read")
                                {
                                    CC.type = keyword;
                                    continue;
                                }else if(keyword is "Write" or "While")
                                {
                                    throw new Exception($"Cannot assign instruction 'WRITE' or 'WHILE' at {Current.Line}");
                                }


                            } 
                            else if (!isAssign)
                            {
                                CI = keyword;
                                continue;

                            }
                        }

                        continue;
                    }

                    // Identifier handling
                    if (CT is LexerType.Token_Identifier) // Handle Variable identifiers
                    {
                        prevTok = CT;


                        if (isAssign && inParen)
                        {
                            throw new Exception($"Cannot Assign inside parenthesis! Error on Line: {Current.Line}");
                        }

                        string ident = Current.GetContent();

                        if(Peek.Value.TokenType is LexerType.Token_LParen)
                        {
                            CB = "Function";
                           
                        }

                        if(CB is "Function" && FN is "")  // if its a function call
                        {
                            FN = new(ident);

                            if (isdebug)
                            {
                                Console.WriteLine("Detected Function Call: {0}", ident);
                            }

                            continue;
                        }

                        if (isAssign || inParen) // if identifier is in paren or right hand of the assignment ( Right )
                        {
                            CC = (ident, "Identifier"); // current Content
                        }
                        else // Left
                        {
                            CI = ident; // Current Instruction

                        }

                        continue;
                    }
                    else if (CT is LexerType.Token_LParen) // (
                    {
                        prevTok = CT;

                        if (inParen)
                        {
                            pdepth++;
                            continue;
                        }

                        if (isArith)
                        {
                            ArithmeticTokens.Add(Current);
                            continue;
                        }

                        if (validParen)
                        {
                            validParen = false;
                        }

                        inParen = true;


                        continue;
                    }
                    else if (CT is LexerType.Token_RParen) // )
                    {
                        prevTok = CT;


                        if (pdepth > 0)
                        {
                            pdepth--;
                            continue;
                        }

                        if (inParen)
                        {
                            validParen = true;
                        }


                        inParen = false;
                        continue;
                    }
                    else if (CT is LexerType.Token_SemiColon) // ;
                    {
                        prevTok = CT;
                        if (inParen)
                        {
                            throw new Exception($"Missing Closing Parenthesis on {Current.Line}");
                        }

                        if(CB == "Function")
                        {

                            if (isdebug)
                            {
                                Console.WriteLine("[DEBUG] Registering function call with name: {0} with target: {1}", FN,CI);
                            }

                            if (CI is "")
                            {
                                NodeBuilder.AddNode(new AssignInstruction(new FunctionCall(FN, null, AddParams())));
                            }
                            else
                            {
                                NodeBuilder.AddNode(new AssignInstruction(new FunctionCall(FN,CI,AddParams())));
                            }



                            // reset
                            args.Clear();
                            CB = "";
                            FN = "";
                            CI = "";
                            validParen = false;
                            isAssign = false;
                            inBlock = false;
                            continue;
                        }

                        if (CI is "Write")
                        {
                            if (!validParen)
                            {
                                throw new Exception($"Missing Left parenthesis on Line {Current.Line}, instruction: {CI}");
                            }

                            if (inBlock && depth == 1)
                            {


                                if (CC.type == "identifier")
                                {
                                    NodeBuilder.AddNode(new WriteInstruction(CC.content, true), true);
                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    continue;
                                }

                                NodeBuilder.AddNode(new WriteInstruction(CC.content), true);

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                continue;
                            }

                            if (!inBlock && depth == 0)
                            {

                                if (CC.type == "identifier")
                                {
                                    NodeBuilder.AddNode(new WriteInstruction(CC.content, true));

                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;

                                    continue;
                                }

                                NodeBuilder.AddNode(new WriteInstruction(CC.content));

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;

                                continue;
                            }

                        }
                        else if (CI is "Read") // STDIN
                        {
                            if (inBlock && depth is 1)
                            {
                                NodeBuilder.AddNode(new AssignInstruction(new ReadAssign("null", CC.content)), inBlock);

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                            if (!inBlock && depth is 0)
                            {
                                NodeBuilder.AddNode(new AssignInstruction(new ReadAssign("null", CC.content)));
                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                        }
                        else if (CI is "Run") // Run
                        {
                            if (inBlock && depth is 1)
                            {
                                NodeBuilder.AddNode(new AssignInstruction((new RunAssignment(("null", CC.content)))), inBlock);

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                            if (!inBlock && depth is 0)
                            {
                                NodeBuilder.AddNode(new AssignInstruction((new RunAssignment(("null", CC.content)))));

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                        }
                        else if (validParen && !isAssign && FN is not "" and not "None") // standalone targetless functions
                        {
                            // Standalone function call: identifier(...);

                            if (isdebug)
                            {
                                Console.WriteLine("Registering function call with name {0}", FN);
                            }

                            if (inBlock && depth is 1)
                            {
                                NodeBuilder.AddNode(new AssignInstruction(new FunctionCall(FN, null, AddParams())), inBlock);
                            }
                            else if (!inBlock && depth is 0)
                            {
                                NodeBuilder.AddNode(new AssignInstruction(new FunctionCall(FN, null, AddParams())));
                            }

                            // reset
                            args.Clear();
                            
                            CI = string.Empty;
                            CC = ("", "");
                            validParen = false;
                            continue;
                        }
                        else
                        {
                            VariableType _type = VariableType.String;
                            IValue value = new AutomaString(CC.content);

                            if (CC.type is "int")
                            {
                                _type = VariableType.Int;
                                value = new AutomaInteger(int.Parse(CC.content));
                            }
                            else if (CC.type is "identifier")
                            {
                                _type = VariableType.Identifier;
                            }
                            else if( CC.type is "bool")
                            {
                                _type = VariableType.Boolean;
                                value = new AutomaBoolean(bool.Parse(CC.content));
                            }

                            string varname = CI;




                            if (CC.type is "Read")
                            {

                                if (inBlock && depth is 1)
                                {
                                    NodeBuilder.AddNode(new AssignInstruction(new ReadAssign(varname, CC.content)), inBlock);
                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;
                                    continue;
                                }

                                if (!inBlock && depth is 0)
                                {
                                    NodeBuilder.AddNode(new AssignInstruction(new ReadAssign(varname, CC.content)));
                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;
                                    continue;
                                }
                            }
                            else if (CC.type is "Run")
                            {
                                if (inBlock && depth is 1)
                                {
                                    NodeBuilder.AddNode(new AssignInstruction(new RunAssignment((varname, CC.content))), inBlock);
                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;
                                    continue;
                                }

                                if (!inBlock && depth is 0)
                                {
                                    NodeBuilder.AddNode(new AssignInstruction(new RunAssignment((varname, CC.content))), inBlock);
                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;
                                    continue;
                                }
                            }

                            if (isArith && !inBlock)
                            {
                                Arithmetic arith = new(ArithmeticTokens,isdebug);

                                NodeBuilder.AddNode(new AssignInstruction(new ArithmeticAssign(arith.ParseArithmetic(out int _),varname)));

                                ArithReset();
                                continue;
                            }

                            if(isArith && inBlock)
                            {
                                Arithmetic arith = new(ArithmeticTokens,isdebug);

                                NodeBuilder.AddNode(new AssignInstruction(new ArithmeticAssign(arith.ParseArithmetic(out int _), varname)), true);

                                ArithReset();
                                continue;

                            }

                            if ((inBlock && !isArith) && depth is 1)
                            {
                                NodeBuilder.AddNode(new AssignInstruction(new VariableAssign(new(varname, value, _type))), inBlock);
                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                            if ((!inBlock && !isArith) && depth is 0)
                            {
                                NodeBuilder.AddNode(new AssignInstruction(new VariableAssign(new(varname, value, _type))));
                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }
                        }



                    }
                    else if (CT is LexerType.TokenString or LexerType.TokenInt or LexerType.TokenBool) // Integer,Boolean or String literals
                    {
                        prevTok = CT;

                        if ((isAssign || inParen) && CC.type is "Read" or "Run")
                        {
                            CC.content = Current.GetContent();
                            continue;
                        }

                        if (CT is LexerType.TokenInt)
                        {

                            CC = (Current.GetContent(), "int");
                            continue;
                        }else if(CT is LexerType.TokenBool)
                        {
                            CC = (Current.GetContent(), "bool");
                        }

                        CC = (Current.GetContent(), "string");
                        continue;
                    }
                    else if (CT is LexerType.Token_Equal) // =
                    {

                        if (prevTok is LexerType.Token_Identifier)
                        {
                            isAssign = true;
                            prevTok = CT;
                            continue;
                        }

                        if (prevTok is LexerType.Token_Equal)
                        {
                            isAssign = false;
                            prevTok = CT;
                            continue;
                        }

                        if (prevTok is LexerType.Token_Not)
                        {
                            isAssign = false;
                            prevTok = CT;
                            continue;
                        }

                        throw new Exception($"Invalid Assignment Usage at line: {Current.Line}");
                    }
                    else if (CT is LexerType.Token_LBrace) // {
                    {
                        prevTok = LexerType.Token_LBrace;

                        if(FN is not "")
                        {
                            if (isdebug)
                            {
                                Console.WriteLine("[DEBUG] Adding function {0} to table", FN);
                            }
                            FunctionTable.Add(FN,ParseStatement<Function>(Current, out int skip, LexerType.Token_RBrace, FN, AddArgs()));
                            args.Clear();
                            FN = "";
                            CB = "";
                            inBlock = false;
                            inFunc = false;

                            if(skip > 0)
                            {
                                i += skip - 1;
                            }
                            continue;
                        }

                        if (inBlock)
                        {
                            depth++;
                            continue;
                        }

                        

                        continue;
                    }
                    else if (CT is LexerType.Token_RBrace) // }
                    {
                        prevTok = LexerType.Token_RBrace;
                        if (inBlock && depth > 1)
                        {
                            depth--;
                            // Reset every depth greater than 1
                            CC = ("", "");
                            CI = "";
                            isAssign = false;
                            inParen = false;
                            continue;
                        }

                        if (inBlock && depth == 1)
                        {
                            depth = 0;
                            CB = "";
                            inBlock = false;
                        }

                    }
                    else if (CT is LexerType.Token_Not) // !
                    {
                        prevTok = CT;
                        continue;
                    }
                    else if(CT is LexerType.Token_Add) // +
                    {
                        prevTok = CT;
                        continue;
                    }else if(CT is LexerType.Token_Minus) // -
                    {
                        prevTok = CT;

                        continue;
                    }else if(CT is LexerType.Token_Multiply) // *
                    {
                        prevTok = CT;

                        continue;
                    }else if(CT is LexerType.Token_Divide) // /
                    {
                        prevTok = CT;
                        continue;
                    }else if(CT is LexerType.Token_Increment or LexerType.Token_Decrement)
                    {
                        prevTok = CT;

                        if(prevTok is not LexerType.Token_Identifier)
                        {
                            throw new Exception($"Invalid use of Unary Assignment at line {Current.Line}");
                        }

                        NodeBuilder.AddNode(new AssignInstruction(new UnaryAssign(CI, (CT is LexerType.Token_Increment) ? UnaryKind.Increment : UnaryKind.Decrement)));

                        CI = "";
                        continue;
                    }
                   
                    prevTok = CT;
                }

                return NodeBuilder.Build();
            }catch(Exception ex)
            {
                Console.WriteLine("Parsing Error; {0}", ex);
                return null;
            }
        }

        public int Start()
        {
            try
            {
                var Data = Parse(); // to be used

                Executor exec = new(Data); // only pass instructions not the entire variables
                

                return exec.Start(isdebug);
            }
            catch(Exception ex)
            {
                Print("Error while Parsing File", ex, new(PrintOptions.Error, true));
                return 1;
            }
            
        }
    }
}
