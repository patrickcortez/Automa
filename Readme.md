# Automa

> [!NOTE] 
> This project is still under development, more features will come. Interpreter may not be stable yet =P.

**Automa** is a Simple Automation Language developed in C#. **Automa's** over all functionality is for
automation in your machine, Automa has atleast 5 instructions for automation.

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

## Supported Operands and type

The only two types in Automa is: `String` and `Int`.

### Example Usage:

```Automa

name = "Cortez"

sum = 20 + 20; # arithmetic assignment

diff = sum - 20; 

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