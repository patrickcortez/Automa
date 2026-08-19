using Automa.Source.Utility;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;
using System.Threading.Tasks.Dataflow;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Parser(LexerToken[] LexTok,string[]? Tokens = null)
    {
        string[] Keywords = ["Write", "Read", "If","Elif","Else","Run"];

        //string[] LogicalOperators = ["==", "!="];


        // Expression handling: logical or Arithmetic. Currently its Just Logical (for now)
        private Expression? ParseExpression(List<LexerToken> Tokens)
        {
            try
            {
                Expression? expr = null;
                string LogicOP = "", left = "", right = "";
                LexerType PrevType = LexerType.Token_None;

                foreach (LexerToken Current in Tokens)
                {
                    LexerType CT = Current.TokenType;

                    if (CT is LexerType.Token_Identifier)
                    {
                        PrevType = CT;

                        if (LogicOP.Length is 0)
                        {
                            right = Current.GetContent();
                        }
                        else
                        {
                            left = Current.GetContent();
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
                    }
                    else if (CT is LexerType.TokenString or LexerType.TokenInt)
                    {
                        string content = Current.GetContent();

                        if (LogicOP.Length is 0)
                        {
                            right = content;
                        }
                        else
                        {
                            left = content;
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

        private T? ParseStatement<T>(LexerToken Starting, LexerType Ending = LexerType.Token_RBrace)
        {
            try
            {


                Type type = typeof(T);
                List<object> Instructions = new();
                List<LexerToken> expression = new();
                int StartingIndex = LexTok.IndexOf(Starting);
                Expression? expr = null;
                LexerType? PrevTok = null;
                bool inBlock = false, 
                        inParen = false,
                        validParen = false,
                        parseExpression = false,
                        
                        isAssign = false;
                int depth = 0, pdepth = 0; // brace depth and parenthesis depth

                string CurrentInstruction = "",
                        CurrentBlock = "";
                (string value, string type) CurrentContent = ("","");

                List<LexerToken> Toks = LexTok.Skip(StartingIndex).ToList();

                foreach (LexerToken Current in Toks)
                {
                    LexerType CurrentType = Current.TokenType;

                    // Expression Handling
                    if (parseExpression && CurrentType is not LexerType.Token_RBrace)
                    {
                        if(depth > 0)
                        {
                            continue;
                        }
                        expression.Add(Current);
                        continue;
                    }
                    else if(parseExpression && CurrentType is LexerType.Token_RBrace)
                    {
                        if (depth > 0)
                        {
                            continue;
                        }
                        expr = ParseExpression(expression);
                        parseExpression = false;
                        continue;
                    }

                    
                    // Determine if we're entering an Expression
                    if((inBlock && PrevTok is LexerType.Token_Identifier ) && CurrentType is LexerType.Token_LParen && !parseExpression)
                    {
                        if (depth > 0)
                        {
                            continue;
                        }
                        parseExpression = true;
                        continue;
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

                        // If-else control flow.
                        if (content is "If" or "Elif" or "Else")
                        {
                            if (inBlock)
                            {
                                if (content is "If")
                                {
                                    Instructions.Add(ParseStatement<IfBlock>(Current) ?? throw new Exception($"Malformed Block at Line: {Current.Line}"));
                                }
                                else if (content is "Elif")
                                {
                                    Instructions.Add(ParseStatement<Elif>(Current) ?? throw new Exception($"Malformed Block at Line: {Current.Line}"));
                                }
                                else if (content is "Else")
                                {
                                    Instructions.Add(ParseStatement<Else>(Current) ?? throw new Exception($"Malformed Block at Line: {Current.Line}"));
                                }
                                continue;
                            }
                            else
                            {
                                if(content is "If" or "Elif" or "Else")
                                {
                                    CurrentBlock = content;
                                    inBlock = true;
                                    continue;
                                }
                            }

                        }



                        if (isAssign || inParen) 
                        {
                            if (isAssign && Keywords.Contains(content)) // Assignment Type 
                            {
                                if(content is "Read" or "Run")
                                {
                                    if (inParen && CurrentContent.type is "Run" or "Read")
                                    {
                                        throw new Exception("KeyWords inside Assignment types");
                                    }

                                    CurrentContent.type = content;
                                    continue;
                                }
                            }
                            else if(!isAssign && Keywords.Contains(content)) // Read and Run instruction without assignment
                            {
                                if(content is "Read" or "Run")
                                {
                                    if (inParen && CurrentContent.type is "Run" or "Read")
                                    {
                                        throw new Exception("KeyWords inside Instruction types");
                                    }
                                    CurrentInstruction = content;
                                    continue;
                                }

                            }
                            // variable call
                            CurrentContent = (content, "identifier");
                            continue;
                        }
                        else
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
                        else if (inParen && PrevTok != LexerType.Token_Identifier)
                        {
                            throw new Exception($"Invalid Use of Literals at line {Current.Line}");
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
                                Instructions.Add(new WriteInstruction(CurrentContent.value,true));
                                continue;
                            }

                            Instructions.Add(new WriteInstruction(CurrentContent.value));

                            // reset
                            CurrentContent = ("", "");
                            CurrentInstruction = "";
                            isAssign = false;

                            continue;
                        }
                        else if(CurrentInstruction is "Read")
                        {
                            Instructions.Add(new ReadAssign("null", CurrentContent.value));

                            // reset
                            CurrentContent = ("", "");
                            CurrentInstruction = "";
                            isAssign = false;

                            continue;
                        }
                        else if(CurrentInstruction is "Run")
                        {
                            Instructions.Add(new RunAssignment(("null",CurrentContent.value)));

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
                                Instructions.Add(new ReadAssign(varname, CurrentContent.value));

                                // reset
                                CurrentContent = ("", "");
                                CurrentInstruction = "";
                                isAssign = false;

                                continue;
                            }
                            else if(CurrentContent.type is "Run")
                            {
                                Instructions.Add(new RunAssignment((varname,CurrentContent.value)));

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

                            Instructions.Add(new VariableAssign(new(varname, CurrentContent.value,_type)));
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

                if (type == typeof(IfBlock))
                {
                    IfBlock block = new(expr, Instructions, new());
                    return (T)(object)block;
                }
                else if (type == typeof(Elif))
                {
                    Elif block = new(expr, Instructions, new());
                    return (T)(object)block;
                }
                else if (type == typeof(Else))
                {
                    Else block = new(Instructions, new());
                    return (T)(object)block;
                }

                return (T)(object)null; // this is basically unrecheable but we have to return something -_-.
            }catch(Exception ex)
            {
                Console.Error.WriteLine("Parser Statement Error: {0}",ex);
                return (T)(object)null;
            }
        }


        private List<object> _Parse()
        {
            try
            {
                List<object> Instructions = new();
                List<object> BlockInstructions = new();
                (string content,string type) CC =( "", ""); // Current Content
                bool inBlock = false,
                    inParen = false,
                    validParen = false,
                    parseExpression = false,
                    isAssign = false;
                int depth = 0, pdepth = 0; // if-block depth and parenthesis depth
                string CI = "None"; // Current Instruction,
                LexerType prevTok = LexerType.Token_None;
                string  CB = ""; // Logical operator and Current Block

                LexerToken[] _Tokens = LexTok;
                Expression? expr = null;



                List<LexerToken> expression = new();

                foreach (LexerToken Current in _Tokens)
                {
                    LexerType CT = Current.TokenType;

                    // Expression Handling

                    if (CT is LexerType.Token_RParen && parseExpression)
                    {


                        expr = ParseExpression(expression); // Determine Expression
                        
                        parseExpression = false;
                        expression.Clear();
                        continue;
                    }
                    

                    if (parseExpression)
                    {
                        expression.Add(Current);
                        continue;
                    }


                    if((inBlock && prevTok is LexerType.Token_Identifier) && CT is LexerType.Token_LBrace && !parseExpression) // Expression Parsing
                    {
                        parseExpression = true;
                        continue;
                    }

                    // Check Tokens

                    // Identifier handling
                    if (CT is LexerType.Token_Identifier)
                    {
                        prevTok = CT;

                        if (isAssign && inParen) 
                        {
                            throw new Exception($"Cannot Assign inside parenthesis! Error on Line: {Current.Line}");
                        }

                        string ident = Current.GetContent();
                        

                        if (Keywords.Contains(ident))
                        {
                            
                            if(CI is "If" or "Elif" or "Else" && depth == 1)
                            {
                                CB = CI;
                                if (inBlock)
                                {
                                    if(CB is "If")
                                    {
                                        BlockInstructions.Add(ParseStatement<IfBlock>(Current) ?? throw new Exception($"Parsing Error: Nested if is malformed at line: {Current.Line}"));
                                    }
                                    else if(CB is "Elif")
                                    {
                                        BlockInstructions.Add(ParseStatement<Elif>(Current) ?? throw new Exception($"Parsing Error: Nested if is malformed at line: {Current.Line}"));
                                    }
                                    else if(CB is "Else")
                                    {
                                        BlockInstructions.Add(ParseStatement<Else>(Current) ?? throw new Exception($"Parsing Error: Nested if is malformed at line: {Current.Line}"));
                                    }
                                    continue;
                                }

                                if (!inBlock)
                                {
                                    inBlock = true;
                                }

                            }

                            continue;
                        }

                        if (isAssign || inParen) // if identifier is in paren or right hand of the assignment ( Right )
                        {
                            if (Keywords.Contains(ident) && isAssign)
                            {
                                if(ident is "Read" or "Run")
                                {
                                    CC.type = ident;
                                    continue;
                                }
                            }else if(!isAssign && Keywords.Contains(ident))
                            {
                                if (ident is "Read" or "Run")
                                {
                                    CI = ident;
                                    continue;
                                }
                            }

                            CC = (ident, "Identifier"); // current Content
                        }
                        else // Left
                        {
                            CI = ident; // Current Instruction
                            continue;
                        }


                    }
                    else if (CT is LexerType.Token_LParen) // (
                    {
                        prevTok = CT;
                        if (inParen)
                        {
                            pdepth++;
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
                        if (depth > 0)
                        {
                            depth--;
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

                                if(CC.type == "identifier")
                                {
                                    BlockInstructions.Add(new WriteInstruction(CC.content,true));
                                }

                                BlockInstructions.Add(new WriteInstruction(CC.content));

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;

                                continue;
                            }

                            if(!inBlock && depth == 0)
                            {

                                if (CC.type == "identifier")
                                {
                                    Instructions.Add(new WriteInstruction(CC.content, true));
                                }

                                Instructions.Add(new WriteInstruction(CC.content));

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;

                                continue;
                            }

                        }
                        else if(CI is "Read" ) // STDIN
                        {
                            if(inBlock && depth is 1)
                            {
                                BlockInstructions.Add(new ReadAssign("null", CC.content));

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                            if(!inBlock && depth is 0)
                            {
                                Instructions.Add(new ReadAssign("null", CC.content));

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                        }
                        else if(CI is "Run") // Run
                        {
                            if(inBlock && depth is 1)
                            {
                                BlockInstructions.Add(new RunAssignment(("null", CC.content)));
                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                            if(!inBlock && depth is 0)
                            {
                                Instructions.Add(new RunAssignment(("null",CC.content)));
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

                            if(CC.type is "int")
                            {
                                _type = VariableType.Int;
                            }else if(CC.type is "identifier")
                            {
                                _type = VariableType.Identifier;
                            }

                            string varname = CI;

                            if(CC.type is "Read")
                            {

                                if(inBlock && depth is 1)
                                {
                                    BlockInstructions.Add(new ReadAssign(varname, CC.content));
                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;
                                    continue;
                                }

                                if(!inBlock && depth is 0)
                                {
                                    Instructions.Add(new ReadAssign(varname, CC.content));
                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;
                                    continue;
                                }
                            }
                            else if(CC.type is "Run")
                            {
                                if (inBlock && depth is 1)
                                {
                                    BlockInstructions.Add(new RunAssignment((varname, CC.content)));
                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;
                                    continue;
                                }

                                if (!inBlock && depth is 0)
                                {
                                    Instructions.Add(new RunAssignment((varname, CC.content)));
                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;
                                    continue;
                                }
                            }

                            if(inBlock && depth is 1)
                            {
                                BlockInstructions.Add(new VariableAssign(new(varname, CC.content, _type)));
                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }

                            if(!inBlock && depth is 0)
                            {
                                Instructions.Add(new VariableAssign(new(varname, CC.content, _type)));
                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;
                                continue;
                            }
                        }



                    }
                    else if(CT is LexerType.TokenString or LexerType.TokenInt) // Integer or String literals
                    {
                        prevTok = CT;

                        if((isAssign || inParen) && CC.type is "Read" or "Run")
                        {
                            CC.content = Current.GetContent();
                            continue;
                        }

                        if (CT is LexerType.TokenInt)
                        {
                            CC = (Current.GetContent(), "int");
                            continue;
                        }

                        CC = (Current.GetContent(),"string");
                        continue;
                    }
                    else if(CT is LexerType.Token_Equal) // =
                    {

                        if(prevTok is LexerType.Token_Identifier)
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

                        if(prevTok is LexerType.Token_Not)
                        {
                            isAssign = false;
                            prevTok = CT;
                            continue;
                        }

                        throw new Exception($"Invalid Assignment Usage at line: {Current.Line}");
                    }
                    else if(CT is LexerType.Token_LBrace) // {
                    {
                        prevTok = LexerType.Token_LBrace;
                        if (inBlock)
                        {
                            depth++;
                            continue;
                        }

                        
                        continue;
                    }
                    else if(CT is LexerType.Token_RBrace) // }
                    {
                        prevTok = LexerType.Token_RBrace;
                        if (inBlock && depth > 1)
                        {
                            depth--;
                            continue;
                        }

                        if( inBlock && depth == 1)
                        {
                            if(CB is "If")
                            {
                                Instructions.Add(new IfBlock(expr, BlockInstructions, new()));
                                continue;
                            }else if(CB is "Elif")
                            {
                                Instructions.Add(new Elif(expr, BlockInstructions, new()));
                                continue;
                            }
                            else if(CB is "Else")
                            {
                                Instructions.Add(new Else(BlockInstructions, new()));
                                continue;
                            }
                        }

                        if(CB.Length > 0)
                        {
                            CB = "";
                        }


                        inBlock = false;
                    }
                    else if(CT is LexerType.Token_Not) // !
                    {
                        prevTok = CT;
                        continue;
                    }
                   
                    prevTok = CT;
                }

                return Instructions;
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


                var Data = _Parse(); // to be used

                Executor exec = new(Data); // only pass instructions not the entire variables
                

                return exec.Start();
            }
            catch(Exception ex)
            {
                Print("Error while Parsing File", ex, new(PrintOptions.Error, true));
                return 1;
            }
            
        }
    }
}
