using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechLink.Core.Interfacing;

namespace TechLink.Maths.Equations.Renderers
{
    public static class ExpressionTreeRenderer
    {
        struct Renderer
        {
            int IndentationLevel;

            public void RenderItem(TreeItem item)
            {
                switch (item)
                {
                    // Simple values
                    case Number number:
                        WriteTitle(number.Value.ToString());
                        break;
                    case Variable variable:
                        WriteTitle(variable.Name.ToString());
                        break;
                    case AdditiveLine line:
                        OpenBlock("Add");

                        for (int i = 0; i < line.Items.Count; i++)
                            RenderItem(line.Items[i]);

                        CloseBlock();
                        break;
                    case TermLine term:
                        OpenBlock("Multiply");

                        for (int i = 0; i < term.Terms.Count; i++)
                            RenderItem(term.Terms[i]);

                        CloseBlock();
                        break;
                    case Division term:
                        OpenBlock("Divide");

                        RenderItem(term.Top);
                        RenderItem(term.Bottom);

                        CloseBlock();
                        break;
                    case Root root:
                        OpenBlock("Root");

                        RenderItem(root.Inner);
                        RenderItem(root.Index);

                        CloseBlock();
                        break;
                    case Power power:
                        OpenBlock("Power");

                        RenderItem(power.Base);
                        RenderItem(power.Exponent);

                        CloseBlock();
                        break;
                    case Function function:
                        string funcTitle = function.Type switch
                        {
                            Function.FunctionType.Sin => "Sin",
                            Function.FunctionType.Cos => "Cos",
                            Function.FunctionType.Tan => "Tan",
                            _ => throw new Exception("Invalid function given!")
                        };

                        OpenBlock($"Func{(function.IsInverse ? "-1" : "")} ({funcTitle})");

                        for (int i = 0; i < function.Arguments.Count; i++)
                            RenderItem(function.Arguments[i]);

                        CloseBlock();
                        break;
                }
            }

            void OpenBlock(string title)
            {
                WriteTitle(title);
                IndentationLevel++;

                // Write the opening "|" on the block.
                WriteIndentation();
                Interface.WriteLine();
            }

            private void WriteTitle(string title)
            {
                WriteIndentation();

                // Don't write the arrow if we're on level 0.
                if (IndentationLevel != 0) Interface.Write("-> ", InterfaceColor.Unimportant);
                Interface.Write(title);

                Interface.WriteLine();
            }

            void CloseBlock()
            {
                IndentationLevel--;
                WriteIndentation();
                Console.WriteLine();
            }

            void WriteIndentation()
            {
                if (IndentationLevel == 0) return;

                for (int i = 1; i < IndentationLevel; i++)
                {
                    Interface.Write("|", InterfaceColor.Unimportant);
                    Interface.Write("   ");
                }

                Interface.Write("|", InterfaceColor.Unimportant);
            }
        }

        public static void Render(TreeItem root)
        {
            var state = new Renderer();
            state.RenderItem(root);
        }


    }
}
