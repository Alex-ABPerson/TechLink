using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechLink.Core.Interfacing;
using TechLink.Maths.Equations;
using TechLink.Maths.Equations.Helpers;
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

        static void TestCombinationIterator()
        {
            Interface.WriteLineFormatted("This page lets you test the 'AdditiveCombinationIterator' helper system.");
            Interface.WriteLineFormatted("$I{Enter the additive line} you want it to iterate through, or $I{type nothing} to exit.");

            string input = Interface.ReadLine();
            if (input == "") return;

            TreeItem item = new ExpressionParser().Run(input);
            if (item is not AdditiveLine line)
            {
                Interface.WriteLineFormatted("$E{Not an additive line}");
                Interface.ReadLine();
                return;
            }

            var iter = new AdditiveCombinationIterator(line);
            while (iter.NextCombinationSize())
            {
                Interface.WriteLine("New Size");

                while (iter.NextCombination())
                {
                    Interface.WriteFormatted("$S{First}: ");
                    ExpressionTextRenderer.Render(iter.GetFirst());
                    
                    Interface.WriteLine();
                    Interface.WriteFormatted("$S{With first}: ");
                    foreach (var iterVal in iter.EnumerateCurrent(false))
                        ExpressionTextRenderer.Render(iterVal);

                    Interface.WriteLine();
                    Interface.WriteFormatted("$S{Without first}: ");
                    foreach (var iterVal in iter.EnumerateCurrent(true))
                        ExpressionTextRenderer.Render(iterVal);

                    Interface.WriteLine();
                }
            }
                

            Interface.ReadLine();
        }

        public override IPage[] SubPages => new IPage[]
        {
            new ActionPage("Compare Two", CompareTwo),
            new ActionPage("View Tokens", PreviewTokens),
            new ActionPage("Combination Iterator", TestCombinationIterator)
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
