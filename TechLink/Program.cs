using System;
using System.Collections.Generic;
using TechLink.Core;
using TechLink.Core.Interfacing;
using TechLink.Maths;
using TechLink.Pages;

namespace TechLink
{
    class Program
    {
        static void Main(string[] args)
        {
            Interface.Initialize();
            Interface.VisitPage(new HomePage());
        }
    }
}
