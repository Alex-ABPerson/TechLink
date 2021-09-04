using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Core
{
    public class ProcessorErrorException : Exception
    {
        public ProcessorErrorException(string message) : base(message) { }
    }
}
