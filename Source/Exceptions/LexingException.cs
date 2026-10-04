using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Exceptions
{
    internal class LexingException(string msg) : Exception(msg)
    {
    }
}
