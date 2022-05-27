using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechLink.Core.Interfacing;
using TechLink.Maths.Equations;
using TechLink.Maths.Equations.Parsing;
using TechLink.Maths.Equations.Renderers;

namespace TechLink.Maths.Pages
{
    public class EquationPage : PageSet
    {
        public Equation? Equation;

        void ParseNew()
        {
            Interface.WriteLine("Enter the new equation:", InterfaceColor.Instruction);

            // Parse
            var equationParser = new EquationParser();
            Equation = (Equation)equationParser.Run(Interface.ReadLine());
        }


        public override string Title => "Equation";
        public override IPage[] SubPages => new IPage[]
        {
            new ActionPage("Parse New", ParseNew)
        };

        public override void Open()
        {
            while (true)
            {
                Interface.Clear();

                if (Equation != null)
                {
                    Interface.Write("You currently have this equation: ", InterfaceColor.Emphasis);
                    EquationRenderer.Render(Equation);
                    Interface.WriteLine();
                }

                ShowMenu();
            }
        }
    }
}
