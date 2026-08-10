# Automa

> [!NOTE] 
> This project is still under development, more features will come. Interpreter may not be stable yet =P.

**Automa** is a Simple Automation Language developed in C#. **Automa's** over all functionality is for
automation in your machine, Automa has atleast 7 instructions for automation.

## Instructions

The Following instructions of **Automa**:

- **Write** : Write to Console.
- **Read**  : Read Input 
- **Variable** : Declare variable
- **If** : If Block which can be nested
- **Elif** : Elif/Else if Block
- **Else** : Else Block
- **Run** : Run Block for running processes

### Example Usage:

```Automa

name = "Cortez"

Write("Hello World")

Write("Your name is $name")

country = Read("What is your home origin? ")

If(country == "USA")
{
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