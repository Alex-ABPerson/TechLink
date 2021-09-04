using System;
using System.Collections.Generic;
using System.Text;

namespace TechLink.Core.Parsing
{
    public class ParserErrorException : Exception
    {
        public ParserErrorException(string msg) : base(msg) { }
    }
}
