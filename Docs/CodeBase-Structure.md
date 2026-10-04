# Code-base

**Automa**'s codebase has been refactored to different parts, especially how huge it has gotten:

*Source*	- Automa's source folder.

*Docs*		- Automa's Documentations.

*Tests*		- Test Scripts for Automa.

---

## Source

Inside the *Source* folder is the codebase of **Automa** which has 3 folders,
each containing list of files that holds **Automa**'s functionality:

*Core*			- Contains the core interpreter of **Automa**
		
		- Shell : Contains Automa's CLI
		- I/O : Contains Automa's I/O handlers
		- FileSystem : Contains Automa's file system operations
		- Lexer
		- Parser
		- Executor
		- Function Table
		- Arithmetic Parser
		- Command Handler : Commandline Arguments from the client.

*Definitions*	- Contains the core definitions of **Automa**, its records, classes and structs being
used all through out the interpreter.

*Utility*		- just some utility for Automa.

---

## File System Operations

**Automa**'s File-system operations are just using the *File* and *Directory* class, then getting the path from what ever the user/dev wants to work on.
So far **Automa** has 4 operations:

	- Create
	- Delete
	- Copy
	- Move

> [!NOTE]
> I will be adding FWrite() and FRead() soon, which are **Automa**'s upcoming file I/O.
> Right after I add arrays.

--- 

## Lexer

Which is *Engine.cs* is the lexer which lexes line by line and stores all the lexertokens that it generates in a list, which is then parsed to the parser.
Any Comments and spaces are stripped out here.

## Parser

**Automa**'s main parser which is `Parse()` is a very long and messy. Which sucks but I will restructure it in the future and seperate concerns and be less reliant on boolean toggles.
`ParseStatement<T>()` handles the recursive parsing when entering blocks or nested statements. `ParseExpression()` handles logical expression parsing, and the *Arithmetic.cs* handles all arithmetic operations in **Automa**.

## Executor

The *Executor* handles all of the execution after the Source file has been tokenized and parsed, every *block* contains an instance of the *Executor* and a Return as an argument, so its value can be manipulated inside the executor but thats for functions. For loops, we rerun executor until the expression becomes false. 


## Types

For now **Automa** has only 3 types which are *Integer*, *String* and *Boolean*, I will add char and float in the future.

## Functions and variables

Both Functions and variables in the language are dynamically typed and evaluated. What ever the type of the value is, is whats the type of the variable/function.





