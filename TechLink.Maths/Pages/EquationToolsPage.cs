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
    class EquationToolsPage : PageSet
    {
        static void CompareTwo()
        {
            Interface.WriteLine("This will let you compare whether two expressions/expressions match or not, which is a mechanism used a lot internally. This can be used to test it's working.");

            Interface.WriteFormatted("Please $I{write the first expression} here: ");
            TreeItem firstTree;
            {
                string input = Interface.ReadLine();

                var parser = new ExpressionParser();
                firstTree = parser.Run(input);
            }

            Interface.WriteFormatted("Please $I{write the first expression} here or $I{type nothing} to use the exact same as the previous: ");
            TreeItem secondTree = firstTree;
            {
                string input = Interface.ReadLine();
                if (input != "")
                {
                    var parser = new ExpressionParser();
                    secondTree = parser.Run(input);
                }
            }

            Interface.WriteLine("First tree:");
            ExpressionTreeRenderer.Render(firstTree);

            Interface.WriteLine("Second tree:");
            ExpressionTreeRenderer.Render(secondTree);

            bool match = firstTree.Equals(secondTree);
            Interface.WriteLine($"Match: {match}");
            Interface.ReadLine();
        }

        static void PreviewTokens()
        {
            Interface.WriteLineFormatted("This page lets you continually lex expression into tokens. Simply $I{type nothing} or navigate back to stop.");

            while (true)
            {
                Interface.WriteLineFormatted("$I{Enter the expression} you want to see the tokens of, or $I{type nothing} to exit.");

                string input = Interface.ReadLine();
                if (input == "") return;

                var lexer = new EquationLexer(input);
                lexer.Run();

                ExpressionTokensRenderer.Render(lexer.Output);

                Interface.WriteLine();
            }
        }

        public override IPage[] SubPages => new IPage[]
        {
            new ActionPage("Compare Two", CompareTwo),
            new ActionPage("View Tokens", PreviewTokens)
        };

        public override string Title => "Equation Tools";

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
