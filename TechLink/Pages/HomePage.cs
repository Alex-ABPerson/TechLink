using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core;
using TechLink.Core.Interfacing;
using TechLink.Maths;

namespace TechLink.Pages
{
    public class HomePage : PageSet
    {
        public override string Title => "Home";
        public override IPage[] SubPages => new IPage[]
        {
            new MathsFeatures(),
            new ToolsFeatures()
        };

        public override void Open()
        {
            Interface.WriteLine("Welcome to 'TechLink'!");
            Interface.WriteLine("Type 'BACK' at any point to return to the previous page");

            while (true)
            {
                ShowMenu();
                Interface.Clear();
            }
        }
    }
}
