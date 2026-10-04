using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Exceptions
{
    internal class ParserException(string msg) : Exception(msg)
    {
    }
}
