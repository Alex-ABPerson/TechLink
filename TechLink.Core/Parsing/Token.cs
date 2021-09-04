using System;
using System.Collections.Generic;
using System.Text;

namespace TechLink.Core.Parsing
{
    public interface IToken
    {
        public abstract byte GetTokenType();
        public abstract int GetStart();
    }
}
