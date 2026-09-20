using Automa.Source.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime;
using System.Text;
using System.Threading.Tasks.Dataflow;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Parser(LexerToken[] LexTok,bool isdebug = false)
    {
        string[] Keywords = ["Write", "Read", "If","Elif","Else","Run"];

        // Expression handling: logical or Arithmetic. Currently its Just Logical (for now)
        private Expression? ParseExpression(List<LexerToken> Tokens)
        {
            try
            {
                if(Tokens.Count < 4)
                {
                    throw new Exception($"Insufficient Tokens for Expression in Line: {Tokens[0].Line}");
                }
                Expression? expr = null;
                string LogicOP = "", left = "", right = "";
                LexerType PrevType = LexerType.Token_None;
                if (isdebug)
                {
                    Console.WriteLine("\n[Debug] Current Expression (total tokens in expr: {0}):", Tokens.Count);
                }
                foreach (LexerToken Current in Tokens)
                {
                    LexerType CT = Current.TokenType;

                    if (isdebug)
                    {
                        Console.Write(", {0} ,", CT.ToString());
                    }

                    if (CT is LexerType.Token_Identifier)
                    {
                        PrevType = CT;

                        if (LogicOP.Length is 0)
                        {
                            left = Current.GetContent();
                        }
                        else
                        {
                            right = Current.GetContent();
                        }

                        continue;
                    }
                    else if (CT is LexerType.Token_Not)
                    {
                        PrevType = CT;
                        continue;
                    }
                    else if (CT is LexerType.Token_Equal)
                    {
                        if (PrevType is LexerType.Token_Equal)
                        {
                            LogicOP = "EQ";
                        }
                        else if (PrevType is LexerType.Token_Not)
                        {
                            LogicOP = "NEQ";
                        }

                        PrevType = CT;
                        continue;
                    }
                    else if (CT is LexerType.TokenString or LexerType.TokenInt)
                    {
                        string content = Current.GetContent();

                        if (LogicOP.Length is 0)
                        {
                            left = content;
                        }
                        else
                        {
                            right = content;
                        }

                    }
                    else
                    {
                        throw new Exception($"Invalid Token in Expression: {CT}, in Line: {Current.Line}");
                    }


                }

                if (LogicOP == "EQ")
                {
                    expr = new EqualTo(new LiteralExpression(left), new LiteralExpression(right));
                }
                else if (LogicOP == "NEQ")
                {
                    expr = new NotEqualTo(new LiteralExpression(left), new LiteralExpression(right));
                }

                return expr;
            }catch(Exception ex)
            {
                Console.Error.WriteLine("Parser Expression Error: {0}", ex);
                return null;
            }
        }

        private T? ParseStatement<T>(LexerToken Starting,out int tokensConsumed, LexerType Ending = LexerType.Token_RBrace)
        {
            try
            {
                NestBuilder Bob = new();
                Type type = typeof(T);
                List<LexerToken> expression = new();
                int StartingIndex = LexTok.IndexOf(Starting);
                Expression? expr = null;
                LexerType? PrevTok = null;
                bool inBlock = false, 
                        inParen = false,
                        parseExpression = false,
                        isAssign = false;
                int depth = 0, pdepth = 0; // brace depth and parenthesis depth

                bool isArith = false;

                string CurrentInstruction = "",
                        CurrentBlock = "";
                (string value, string type) CurrentContent = ("","");

                List<LexerToken> Toks = LexTok.Skip(StartingIndex).ToList();

                if (isdebug)
                {
                    Console.WriteLine("[Debug] Statement Tokens (Statement Token total: {0}):",Toks.Count);
                }

                int tc = 0;

                // Iterate through all the tokens
                for (int i = 0; i < Toks.Count;i++)
                {
                    LexerToken Current = Toks[i];
                    LexerType CurrentType = Current.TokenType;

                    tc++;

                    if (isdebug)
                    {
                        Console.WriteLine("{0}", CurrentType.ToString());
                    }

                    // Expression Handling
                    if (parseExpression && CurrentType is not LexerType.Token_RParen && depth is 0)
                    {
                        expression.Add(Current);
                        continue;
                    }
                    else if(parseExpression && CurrentType is LexerType.Token_RParen && depth is 0)
                    {
                        expr = ParseExpression(expression);
                        parseExpression = false;
                        continue;
                    }

                    
                    // Determine if we're entering an Expression
                    if((inBlock && PrevTok is LexerType.Token_Identifier ) && CurrentType is LexerType.Token_LParen && !parseExpression && depth is 0)
                    {
                        parseExpression = true;
                        continue;
                    }

                    if(CurrentType is LexerType.Token_KeyWord) // keyword handling: if,elif,else and etc...
                    {
                        string keyword = Current.GetContent();

                        if(keyword is "If" or "Elif" or "Else")
                        {

                            if (!inBlock)
                            {
                                CurrentBlock = keyword;
                                inBlock = true;
                                continue;
                            }
                            else
                            {
                                if(keyword is "If")
                                {
                                    Block? parsedBlock = ParseStatement<IfBlock>(Current, out int skip);

                                    if (parsedBlock is null)
                                    {
                                        throw new Exception($"Malformed block at line: {Current.Line}");
                                    }

                                    Bob.AddNode(parsedBlock);

                                    if (skip > 0) i += (skip - 1);
                                }else if(keyword is "Elif")
                                {
                                    Block? parsedBlock = ParseStatement<Elif>(Current, out int skip);

                                    if (parsedBlock is null)
                                    {
                                        throw new Exception($"Malformed block at line: {Current.Line}");
                                    }

                                    Bob.AddNode(parsedBlock);
                                }else if(keyword is "Else")
                                {
                                    Block? parsedBlock = ParseStatement<Else>(Current, out int skip);

                                    if (parsedBlock is null)
                                    {
                                        throw new Exception($"Malformed block at line: {Current.Line}");
                                    }

                                    Bob.AddNode(parsedBlock);
                                }

                                continue;
                            }

                        }

                        if (isAssign || inParen)
                        {
                            if(keyword is "Read" or "Run")
                            {
                                CurrentContent.type = keyword;
                                continue;
                            }else if(keyword is "Write")
                            {
                                throw new Exception($"Cannot use write as an assign type! at {Current.Line}");
                            }
                            continue;
                        }else if (!isAssign)
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

                        if(inParen && isAssign)
                        {
                            throw new Exception($"Cannot assign inside parenthesis, at line: {Current.Line}");
                        }


                        if (isAssign || inParen) // if identifier is on right side or on a parenthesis
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
                    else if(CurrentType is LexerType.Token_Equal)
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
                        else if(PrevTok is LexerType.Token_Equal)
                        {
                            isAssign = false;
                            PrevTok = CurrentType;
                            continue;
                        }else if(PrevTok is LexerType.Token_Not)
                        {
                            isAssign = false;
                            PrevTok = CurrentType;
                            continue;
                        }

                        throw new Exception($"Invalid assignment usage at line {Current.Line}");
                    }
                    else if(CurrentType is LexerType.Token_LParen) // (
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
                            if(CurrentContent.type is "Run" or "Read")
                            {
                                throw new Exception($"Cannot have one or more parenthesis in Assign types Run or Read at Line {Current.Line}");
                            }

                            pdepth++;
                            continue;
                        }
                    }
                    else if(CurrentType is LexerType.Token_RParen) // )
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

                        if(pdepth > 0)
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
                    else if(CurrentType is LexerType.TokenString or LexerType.TokenInt) // "abc" or 123
                    {
                        if (depth > 1)
                        {
                            continue;
                        }

                        if (!isAssign && !inParen)
                        {
                            throw new Exception($"Cannot assign value to Literals at line {Current.Line}");
                        }


                        if((inParen && CurrentContent.type is "Run" or "Read")) // assuming the next is a R paren ')'
                        {
                            CurrentContent.value = Current.GetContent(); // store the arg of Run and Read
                            continue;
                        }

                        if(CurrentType is LexerType.TokenInt) // int 
                        {
                            CurrentContent = (Current.GetContent(), "int");
                            continue;
                        }

                        CurrentContent = (Current.GetContent(), "string"); // string
                        continue;
                    }
                    else if(CurrentType is LexerType.Token_SemiColon) // ;
                    {
                        if (depth > 1)
                        {
                            continue;
                        }

                        if (CurrentInstruction is "Write") // Instruction: Write  (STDOUT)
                        {
                            if(CurrentContent.type is "identifier")
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
                        else if(CurrentInstruction is "Read")
                        {
                            Bob.AddNode(new AssignInstruction(new ReadAssign("null", CurrentContent.value)));



                            // reset
                            CurrentContent = ("", "");
                            CurrentInstruction = "";
                            isAssign = false;

                            continue;
                        }
                        else if(CurrentInstruction is "Run")
                        {
                            Bob.AddNode(new AssignInstruction(new RunAssignment(("null",CurrentContent.value))));

                            // reset
                            CurrentContent = ("", "");
                            CurrentInstruction = "";
                            isAssign = false;

                            continue;
                        }
                        else
                        {


                            string varname = CurrentInstruction;

                            if(CurrentContent.type is "Read")
                            {
                                Bob.AddNode(new AssignInstruction(new ReadAssign(varname, CurrentContent.value)));
                                
                                // reset
                                CurrentContent = ("", "");
                                CurrentInstruction = "";
                                isAssign = false;

                                continue;
                            }
                            else if(CurrentContent.type is "Run")
                            {
                                Bob.AddNode(new AssignInstruction(new RunAssignment((varname, CurrentContent.value))));

                                // reset
                                CurrentContent = ("", "");
                                CurrentInstruction = "";
                                isAssign = false;

                                continue;
                            }

                            VariableType _type = VariableType.String;

                            if(CurrentContent.type is "int")
                            {
                                _type = VariableType.Int;
                            }else if(CurrentContent.type is "identifier")
                            {
                                _type = VariableType.Identifier;
                            }

                            Bob.AddNode(new AssignInstruction(new VariableAssign(new(varname, CurrentContent.value, _type))));
                            // reset
                            CurrentContent = ("", "");
                            CurrentInstruction = "";
                            isAssign = false;
                            continue;

                        }

                    }
                    else if(CurrentType is LexerType.Token_LBrace)
                    {
                        if (inBlock) // depth always starts at 1;
                        {
                            depth++;
                            continue;
                        }
                    }
                    else if(CurrentType is LexerType.Token_RBrace)
                    {
                        if(depth > 1)
                        {
                            depth--;
                            continue;
                        }

                        break; // if depth reaches 1
                    }
                    else if(CurrentType is LexerType.Token_Not)
                    {
                        PrevTok = CurrentType;
                        continue;
                    }
                }

                tokensConsumed = tc;

                if (type == typeof(IfBlock))
                {
                    IfBlock block = new IfBlock(expr, new());
                    block.Body = Bob.Build();

                    return (T)(object)block;
                }
                else if (type == typeof(Elif))
                {
                    Elif block = new Elif(expr, new());
                    block.Body = Bob.Build();

                    return (T)(object)block;
                }
                else if (type == typeof(Else))
                {
                    Else block = new Else(new());
                    block.Body = Bob.Build();

                    return (T)(object)block;
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
                    isAssign = false;
                int depth = 0, pdepth = 0; // if-block depth and parenthesis depth
                string CI = "None"; // Current Instruction,
                LexerType prevTok = LexerType.Token_None;
                string  CB = ""; // Logical operator and Current Block

                // Arithmetic tokens for Arithmetic parser;
                List<LexerToken> ArithmeticTokens = new();

                LexerToken[] _Tokens = LexTok;
                Expression? expr = null;
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
                        if((Peek.Value.TokenType is LexerType.Token_Add or LexerType.Token_Minus or LexerType.Token_Multiply or LexerType.Token_Divide && CT is LexerType.TokenInt or LexerType.Token_Identifier && isAssign && !isArith) || (Peek.Value.TokenType is LexerType.TokenInt or LexerType.Token_Identifier && CT is LexerType.Token_LParen && isAssign && !isArith)) // 2 + or -
                        {
                            isArith = true;
                        }
                    }

                    if (isdebug)
                    {
                        Console.WriteLine("[Debug] Current Token: {0}",CT.ToString());
                    }

                    // Expression Handling

                    if (CT is LexerType.Token_RParen && parseExpression && depth is 0)
                    {

                        expr = ParseExpression(expression); // Determine Expression

                        if (CB is "If") // add Block nodes before  the next token comes;
                        {
                            NodeBuilder.AddNode(new IfBlock(expr, new()));
                        }
                        else if (CB is "Elif")
                        {
                            NodeBuilder.AddNode(new Elif(expr, new()));
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
                                if (keyword is "If")
                                {
                                    NodeBuilder.AddNode(ParseStatement<IfBlock>(Current, out int tokenConsumed), true);
                                    if (tokenConsumed > 0) i += (tokenConsumed - 1); // Jump to the end of the block
                                }
                                else if (keyword is "Elif")
                                {
                                    NodeBuilder.AddNode(ParseStatement<Elif>(Current, out int tokenConsumed), true);
                                    if (tokenConsumed > 0) i += (tokenConsumed - 1); // Jump to the end of the block
                                } else if (keyword is "Else")
                                {
                                    NodeBuilder.AddNode(ParseStatement<Else>(Current, out int tokenConsumed), true);
                                    if (tokenConsumed > 0) i += (tokenConsumed - 1); // Jump to the end of the block
                                }
                            }
                            else
                            {
                                CI = keyword;
                                CB = CI;
                                inBlock = true;

                                if (keyword is "Else")
                                {
                                    NodeBuilder.AddNode(new Else(new()));
                                }

                                continue;

                            }
                        }
                        else // Read, Write and Run
                        {

                            if (isAssign || inParen)
                            {
                                if (keyword is "Run" or "Read")
                                {
                                    CC.type = keyword;
                                    continue;
                                }else if(keyword is "Write")
                                {
                                    throw new Exception($"Cannot assign instruction 'WRITE' at {Current.Line}");
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

                        if (CI is "Write")
                        {
                            if (!validParen)
                            {
                                throw new Exception($"Missing Left parenthesis on Line {Current.Line}");
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
                        else
                        {
                            VariableType _type = VariableType.String;

                            if (CC.type is "int")
                            {
                                _type = VariableType.Int;
                            }
                            else if (CC.type is "identifier")
                            {
                                _type = VariableType.Identifier;
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
                                NodeBuilder.AddNode(new AssignInstruction(new VariableAssign(new(varname, CC.content, _type))), inBlock);
                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                            if ((!inBlock && !isArith) && depth is 0)
                            {
                                NodeBuilder.AddNode(new AssignInstruction(new VariableAssign(new(varname, CC.content, _type))));
                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }
                        }



                    }
                    else if (CT is LexerType.TokenString or LexerType.TokenInt) // Integer or String literals
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
