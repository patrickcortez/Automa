using Automa.Source.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Parser(LexerToken[] LexTok,string[]? Tokens = null)
    {
        string[] Keywords = ["Write", "Read", "If","Elif","Else","Run"];

        //string[] LogicalOperators = ["==", "!="];

        private T? ParseBlock<T>(string StartingLine)
        {
            List<object> Instructions = new();
            int index = Tokens.IndexOf(StartingLine), depth=0;
            bool inBlock = false;
            string[] SubTokens = Tokens.Skip(index).ToArray();
            Expression? expr = null;
            Type type = typeof(T);

            TokenType ParsedType = TokenType.Null;

            if(type == typeof(IfBlock))
            {
                ParsedType = TokenType.If;
            }else if(type == typeof(Elif))
            {
                ParsedType = TokenType.Elif;
            }else if(type == typeof(Else))
            {
                ParsedType = TokenType.Else;
            }


            for (int i = 0; i < SubTokens.Length; i++)
            {
                string Line = SubTokens[i];

                if (Line.StartsWith('{') || Line.StartsWith('}'))
                {
                    if (inBlock)
                    {
                        if (Line.StartsWith('{') && depth == 0)
                        {
                            depth++;
                            continue;
                        }

                        if (Line.StartsWith('}') && depth == 0)
                        {
                            break;
                        }
                        else
                        {
                            depth--;
                        }
                    }

                    inBlock = !inBlock;
                    continue;
                }
                else if (Line.StartsWith(Keywords[0], StringComparison.OrdinalIgnoreCase)) // Write
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    Instructions.Add(new WriteInstruction(CleanString(Line.Substring(Keywords[0].Length, Line.Length - Keywords[0].Length))));
                }
                else if (Line.StartsWith(Keywords[1], StringComparison.OrdinalIgnoreCase)) //Read
                {
                    throw new Exception("Cannot use Read outside of Assignment");
                }
                else if (Line.StartsWith(Keywords[2], StringComparison.OrdinalIgnoreCase)) //If
                {
                    if (depth > 0) // Depth Check
                    {
                        continue;
                    }

                    if (inBlock)
                    {
                        Instructions.Add(ParseBlock<IfBlock>(Line));
                        continue;
                    }

                    expr = ExtractExpression(Line.Substring(Keywords[2].Length, Line.Length - Keywords[2].Length));

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    continue;

                }
                else if (Line.StartsWith(Keywords[3], StringComparison.OrdinalIgnoreCase)) // elif
                { // Elif
                    if (depth > 0) // depth check
                    {
                        continue;
                    }

                    if (inBlock)
                    {
                        Instructions.Add(ParseBlock<Elif>(Line));
                    }

                    expr = ExtractExpression(Line.Substring(Keywords[3].Length, Line.Length - Keywords[3].Length));

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    continue;
                }else if (Line.StartsWith("Else", StringComparison.OrdinalIgnoreCase))
                {
                    if (inBlock)
                    {
                        Instructions.Add(ParseBlock<Else>(Line));
                        continue;
                    }
                }
                else if (Line.StartsWith(Keywords[5], StringComparison.OrdinalIgnoreCase)) // run
                {

                    throw new Exception("Cannot use Run Outside of Assignment");
                }
                else if (HasAssignment(Line)) // Variable Declaration
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    
                    Variable newVar = ExtractVariable(Line, out AssignmentType Atype);


                    if (Atype == AssignmentType.Read)
                    {


                        Instructions.Add(new AssignInstruction(new ReadAssign(newVar.name, newVar.value)));
                        continue;
                    }
                    else if (Atype == AssignmentType.Run)
                    {


                        Instructions.Add(new AssignInstruction(new RunAssignment((newVar.name, newVar.value))));
                        continue;
                    }

                    //Console.WriteLine("Debug: Current Variable Name: {0} , Value: {1}", newVar.name, newVar.value);

                    Instructions.Add(new AssignInstruction(new VariableAssign(newVar)));
                    continue;


                }

            }


            if (ParsedType == TokenType.If)
            {
                IfBlock block = new(expr, Instructions, new());
                return (T)(object)block;
            }else if(ParsedType == TokenType.Elif)
            {
                Elif block = new(expr, Instructions, new());
                return (T)(object)block;
            }else if(ParsedType == TokenType.Else)
            {
                Else block = new(Instructions, new());
                return (T)(object)block;
            }

            throw new Exception($"{type.Name} is not a valid block type!");
        }

        private (List<object> Instructions, List<Variable> Variables) Parse()
        {
            List<object> Instructions = new();
            List<object> BlockInstructions = new();
            Expression? expr = null;
            TokenType Current = TokenType.Null;
            bool inBlock = false;
            int depth = 0;

            foreach (string Line in Tokens)
            {


                if (Line.StartsWith(Keywords[0], StringComparison.OrdinalIgnoreCase)) //Write
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    if (inBlock)
                    {
                        BlockInstructions.Add(new WriteInstruction(CleanString(Line.Substring(Keywords[0].Length, Line.Length - Keywords[0].Length))));
                        continue;
                    }

                    Instructions.Add(new WriteInstruction(CleanString(Line.Substring(Keywords[0].Length, Line.Length - Keywords[0].Length))));
                }
                else if (Line.StartsWith(Keywords[1], StringComparison.OrdinalIgnoreCase)) //Read
                {
                    throw new Exception("Read cannot be used outside of assignment operations");
                }
                else if (Line.StartsWith('{') || Line.StartsWith('}')) // Block Identifiers
                {
                    if (inBlock)
                    {
                        if (Line.StartsWith('{'))
                        {
                            depth++;
                            continue;
                        }

                        if (Line.StartsWith('}') && depth == 0)
                        {
                            //Console.WriteLine("Debug: Block Instructions {0}", BlockInstructions.Count);
                            if (Current == TokenType.If)
                            {
                                Instructions.Add(new IfBlock(expr, new(BlockInstructions), new()));
                                BlockInstructions.Clear(); // always clear Block Instruction =P, since List is a reference type.
                            }
                            else if (Current == TokenType.Elif)
                            {
                                Instructions.Add(new Elif(expr, new(BlockInstructions), new()));
                                BlockInstructions.Clear();
                            }
                            else if (Current == TokenType.Else)
                            {
                                Instructions.Add(new Else(new(BlockInstructions), new()));
                                BlockInstructions.Clear();

                            }

                        }
                        else
                        {
                            depth--;
                            continue;
                        }
                    }

                    inBlock = !inBlock; // move to if later... gonna implement nesting first
                    continue;

                }
                else if (Line.StartsWith(Keywords[2], StringComparison.OrdinalIgnoreCase)) //If
                {
                    if (depth > 0)
                    {
                        continue;
                    }


                    if (inBlock)
                    {
                        BlockInstructions.Add(ParseBlock<IfBlock>(Line));
                        continue;
                    }

                    expr = ExtractExpression(Line.Substring(Keywords[2].Length, Line.Length - Keywords[2].Length));
                    Current = TokenType.If;



                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }



                    continue;

                }
                else if (Line.StartsWith(Keywords[3], StringComparison.OrdinalIgnoreCase)) // elif
                { // Elif
                    if (depth > 0) // depth check
                    {
                        continue;
                    }

                    if (inBlock)
                    {
                        BlockInstructions.Add(ParseBlock<IfBlock>(Line));
                        continue;
                    }

                    expr = ExtractExpression(Line.Substring(Keywords[3].Length, Line.Length - Keywords[3].Length));
                    Current = TokenType.Elif;

                    if (expr is null)
                    {
                        throw new Exception($"Expression in \"{Line}\" is malformed");
                    }

                    continue;
                }
                else if (Line.StartsWith(Keywords[4], StringComparison.OrdinalIgnoreCase))
                { // else

                    if (depth > 0) // depth check
                    {
                        continue;
                    }

                    Current = TokenType.Else;
                    continue;

                }
                else if (Line.StartsWith(Keywords[5], StringComparison.OrdinalIgnoreCase)) // run
                {

                    throw new Exception("Cannot use Run Outside of Assignment");
                }
                else if (HasAssignment(Line)) // Variable Declaration
                {
                    if (depth > 0)
                    {
                        continue;
                    }

                    Variable newVar = ExtractVariable(Line, out AssignmentType type);


                    if (type == AssignmentType.Read)
                    {

                        if (inBlock)
                        {
                            BlockInstructions.Add(new AssignInstruction(new ReadAssign(newVar.name, newVar.value)));
                            continue;
                        }

                        Instructions.Add(new AssignInstruction(new ReadAssign(newVar.name, newVar.value)));
                        continue;
                    }else if(type == AssignmentType.Run)
                    {
                        if (inBlock)
                        {
                            BlockInstructions.Add(new AssignInstruction(new RunAssignment((newVar.name, newVar.value))));
                            continue;
                        }

                        Instructions.Add(new AssignInstruction(new RunAssignment((newVar.name, newVar.value))));
                        continue;
                    }

                    //Console.WriteLine("Debug: Current Variable Name: {0} , Value: {1}", newVar.name, newVar.value);

                    if (inBlock)
                    {
                        BlockInstructions.Add(new AssignInstruction(new VariableAssign(newVar)));
                        continue;
                    }


                    Instructions.Add(new AssignInstruction(new VariableAssign(newVar)));
                    continue;


                }



            }

            return (Instructions,null);
        }

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
                int StartingIndex = LexTok.IndexOf(Starting);
                Expression? expr = null;
                LexerType? PrevTok = null;
                bool inParen = false,
                        validParen = false,
                        parseExpression = false,
                        isAssign = false;
                int depth = 0, pdepth = 0; // brace depth and parenthesis depth

                string CurrentInstruction = "";
                (string value, string type) CurrentContent;

                List<LexerToken> Toks = LexTok.Skip(StartingIndex).ToList();

                foreach (LexerToken Current in Toks)
                {
                    LexerType CurrentType = Current.TokenType;



                    if (CurrentType is LexerType.Token_Identifier) // Instructions or variable decl
                    {
                        PrevTok = CurrentType;
                        string content = Current.GetContent();

                        if(inParen && isAssign)
                        {
                            throw new Exception($"Cannot assign inside parenthesis, at line: {Current.Line}");
                        }

                        if (Keywords.Contains(content))
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
                        }



                        if (isAssign || inParen)
                        {
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
                        PrevTok = CurrentType;

                        if(PrevTok is LexerType.Token_Identifier)
                        {
                            isAssign = true;
                            continue;
                        }
                        else if(PrevTok is LexerType.Token_Equal)
                        {
                            isAssign = false;
                            continue;
                        }else if(PrevTok is LexerType.Token_Not)
                        {
                            isAssign = false;
                            continue;
                        }

                        throw new Exception($"Invalid assignment usage at line {Current.Line}");
                    }
                    else if(CurrentType is LexerType.Token_LParen) // (
                    {
                        // to be implemented
                    }
                    else if(CurrentType is LexerType.Token_RParen) // )
                    {
                        // to be implemented
                    }
                    else if(CurrentType is LexerType.TokenString or LexerType.TokenInt) // "abc" or 123
                    {
                        // to be implemented
                    }
                    else if(CurrentType is LexerType.Token_SemiColon) // ;
                    {
                        // to be implemented
                    }
                    else if(CurrentType is LexerType.Token_LBrace)
                    {
                        // to be implemented
                    }
                    else if(CurrentType is LexerType.Token_RBrace)
                    {
                        // to be implemented
                    }
                    else if(CurrentType is LexerType.Token_Not)
                    {
                        // to be implemented
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


        private List<object> _Parse() // To be continued...
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
                            if (!validParen)
                            {
                                throw new Exception($"Missing Left parenthesis on Line {Current.Line}");
                            }

                            if (!isAssign)
                            {

                                if (inBlock && depth == 1)
                                {
                                    BlockInstructions.Add(new AssignInstruction(new ReadAssign("null", CC.content)));

                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;

                                    continue;
                                }

                                if(!inBlock && depth is 0)
                                {
                                    Instructions.Add(new AssignInstruction(new ReadAssign("null", CC.content)));

                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;

                                    continue;
                                }


                            }


                            if (inBlock && depth == 1)
                            {
                                BlockInstructions.Add(new AssignInstruction(new ReadAssign(CI, CC.content)));
                            }

                            if(!inBlock && depth == 0)
                            {
                                Instructions.Add(new AssignInstruction(new ReadAssign(CI,CC.content)));
                                continue;
                            }

                        }
                        else if(CI is "Run")
                        {
                            if (!validParen)
                            {
                                throw new Exception($"Missing Left parenthesis on Line {Current.Line}");
                            }


                            if (!isAssign)
                            {

                                if(CC.type == "identifier")
                                {
                                    throw new Exception($"Run Args must be in string! Error on Line: {Current.Line}");
                                }

                                if (inBlock && depth == 1)
                                {
                                    BlockInstructions.Add(new AssignInstruction(new RunAssignment(("null", CC.content))));

                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;

                                    continue;
                                }

                                if(!inBlock && depth == 0)
                                {
                                    Instructions.Add(new AssignInstruction(new RunAssignment(("null",CC.content))));

                                    //reset all before proceeding to the next

                                    CI = string.Empty; // erase CI for the next...
                                    CC = ("", "");
                                    validParen = false;
                                    isAssign = false;

                                    continue;
                                }


                            }

                            if (inBlock && depth == 1)
                            {
                                BlockInstructions.Add(new AssignInstruction(new RunAssignment((CI, CC.content))));

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;

                                continue;
                            }

                            if(!inBlock && depth == 0)
                            {
                            Instructions.Add(new AssignInstruction(new RunAssignment((CI, CC.content))));


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

                            if(CC.type == "int")
                            {
                                _type = VariableType.Int; // 123
                            }
                            else if (CC.type == "identifier")
                            {
                                _type = VariableType.Identifier; // No Qourte
                            }

                            if (inBlock && depth == 1)
                            {
                                BlockInstructions.Add(new AssignInstruction(new VariableAssign(new(CI, CC.content,_type))));

                                //reset all before proceeding to the next

                                CI = string.Empty; // erase CI for the next...
                                CC = ("", "");
                                validParen = false;
                                isAssign = false;

                                continue;
                            }

                            if(!inBlock && depth == 0)
                            {
                                Instructions.Add(new AssignInstruction(new VariableAssign(new(CI, CC.content, _type))));

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
                        prevTok = CT;

                        if(prevTok is LexerType.Token_Identifier)
                        {
                            isAssign = true;
                            continue;
                        }

                        if (prevTok is LexerType.Token_Equal)
                        {
                            isAssign = false;
                            continue;
                        }

                        if(prevTok is LexerType.Token_Not)
                        {
                            isAssign = false;
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
                var Data = Parse(); // Currently in use for compat

                var _Data = _Parse(); // to be used

                Executor exec = new(Data.Instructions); // only pass instructions not the entire variables
                

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
