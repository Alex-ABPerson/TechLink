using System;
using System.Collections.Generic;
using System.Text;

namespace TechLink.Core.Interfacing
{
    internal class NavigatedBackException : Exception
    {
        public static readonly string INCORRECT_PAGE_PATTERN_MSG = 
            $"A page was navigated back without properly being handled! Please call 'Interface.VisitPage' when entering pages.";

        public NavigatedBackException() : base(INCORRECT_PAGE_PATTERN_MSG) { }
    }
}
