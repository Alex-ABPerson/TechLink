using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core;
using TechLink.Core.Interfacing;
using TechLink.Maths.Pages;

namespace TechLink.Maths
{
    public class MathsFeatures : PageSet
    {
        public override string Title => "Maths";
        public override IPage[] SubPages => new IPage[]
        {
            new ExpressionPage(),
            new EquationPage(),
            new EquationToolsPage()
        };

        public override void Open()
        {
            while (true)
            {
                Interface.Clear();
                ShowMenu();
            }
        }
    }
}
