# Automa

> [!NOTE] 
> This project is still under development, more features will come. Interpreter may not be stable yet =P.

**Automa** is a Simple Automation Language developed in C#. **Automa's** over all functionality is for
automation in your machine, Automa has atleast 8 instructions for automation.

> [!NOTE]
> Always end your instructions in `;`.

## Instructions

The Following instructions of **Automa**:

- **Write** : Write to Console.
- **Assignment** : Declare a variable with different assignment types.
- **If** : If Block which can be nested
- **Elif** : Elif/Else if Block
- **Else** : Else Block
- **Run** : Run a external command/application.
- **While** : for looping purposes
- **Functions** : for making reusable blocks of code

## Assignment Types

The Following are the assignment types:

- **Run** : Run Block for running processes and assign its exit code to a variable.
- **Read**  : Read Input 
- **Variable** : Declare variable
- **Arithmetic** : Mathematical Assignment type: `num=20+19`. Supports:
	- Addition
	- Subtraction
	- Multiplication
	- Division
	- Parenthesis
- **Function Call** : Returns the value of function based on what the function returns

## Logical Operators:

**Automa** has 2 LogOps:

- **And** : `&&`
- **Or** : `||`

## Comparators:

**Automa** has about 6:

- **EqualTo** : `==`
- **NotEqualTo** : `!=`
- **LessThan** : `<`
- **LTE**  : `<=`
- **GreaterThan** : `>`
- **GTE** : `>=`

## Functions:

**Automa** finally supports functions and its loosely evaluated based on what it returns.
*Functions* are stored on a function table at Parse time, so it can be used anywhere at Runtime.

## Quirks

- In arithmetic: Functions must be declared after the left side, 
I really do not want to add another case on what is arithmetic and what is not.

- All instructions must be written with the first letter as capitalized. Because the interpreter is case sensitive.

- Functions and variables types are implied and is loosely typed/evaluated.

> [!NOTE]
> I will improve the whole interpreter.
> There are alot of very bad implementations all over the code-base,
> especially the interpreter. I will improve them soon, but for now
> I'm way too mentally exhausted from this project.

## Supported Operands and type

The only two types in Automa is: `String`, `Boolean` and `Int`.

> [!NOTE]
> Will add char, double and null soon...

### Example Usage:

```Automa

Function Something(){
	Write("In a function!");

	Return 20;
}

name = "Cortez"

sum = 20 + 20; # arithmetic assignment

diff = sum - 20; 

Something(); # Standalone function!

num = Something(); # Function call in assignment.

num = 2 + Something(); # Function call in arithmetic. 

Secnum = 0;

While(Secnum < 10){

	Write("Current Iteration: $Secnum");
	Secnum = Secnum + 1; # Will add increment and decrement soon... =P

}

If(sum > diff){ 
	Write("$sum is greater than $diff");
}

Write("Hello World")

Write("Your name is $name")

country = Read("What is your home origin? ")

If(country == "USA")
{

	If(counter == "USA" && sum == 40){
		Write("Secret option!");
	}

Write("Good Choice!")

If(name == "Cortez")
{
Write("Nice Name")
}

}
Elif(country == "Greece")
{
Write("Also a good choice!")
}
Else
{
Write("Nice Country!")
}

task = Run("cmd /c dir")

Write("Result of Task $task")

```

## CLI Usage:

To use **Automa** in the CommandLine, Simple do:

`./Automa.exe run <path-to-.auto>`

## Installation

> [!NOTE]
> To be added...

## License

This project is under the License of *GNU General Public Licesne V3*, see [LICENSE](LICENSE.txt)