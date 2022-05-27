using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TechLink.Core;
using TechLink.Core.Interfacing;
using TechLink.Core.Models;
using TechLink.Core.Parsing;
using TechLink.Maths.Equations;
using TechLink.Maths.Equations.Parsing;
using TechLink.Maths.Equations.Processors;
using TechLink.Maths.Equations.Renderers;

namespace TechLink.Maths.Pages
{
    public class ExpressionPage : PageSet
    {
        public TreeItem? Tree;

        void ParseNew()
        {
            Interface.WriteLine("Enter the new expression:", InterfaceColor.Instruction);

            var parser = new ExpressionParser();
            Tree = parser.Run(Interface.ReadLine());
        }

        void Render()
        {
            Interface.Write("You currently have: ", InterfaceColor.Emphasis);

            Interface.SetColor(InterfaceColor.Emphasis);
            ExpressionTextRenderer.Render(Tree!);
            Interface.SetColor(InterfaceColor.Default);

            Interface.ReadLine();
        }

        void Simplify()
        {
            var simplifier = new EquationSimplifier(Tree!);
            var path = simplifier.Simplify();

            PathTreeRenderer.Render(path);

            Interface.ReadLine();
        }

        public override string Title => "Expression";
        public override IPage[] SubPages => new IPage[]
        {
            new ActionPage("Parse New", ParseNew),
            new ActionPage("Render", Render),
            new ActionPage("Simplify", Simplify)
        };

        public override void Open()
        {
            while (true)
            {
                Interface.Clear();

                if (Tree != null)
                {
                    Interface.Write("You currently have this tree parsed: ", InterfaceColor.Emphasis);
                    Interface.WriteLine();

                    ExpressionTreeRenderer.Render(Tree);

                    Interface.WriteLine();
                }

                ShowMenu();
            }
        }
    }
}
