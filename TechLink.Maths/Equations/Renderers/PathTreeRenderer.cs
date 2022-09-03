using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechLink.Core.Interfacing;
using TechLink.Maths.Equations.Processors;

namespace TechLink.Maths.Equations.Renderers
{
    public static class PathTreeRenderer
    {
        struct Renderer
        {
            int IndentationLevel;

            public void RenderItem(PathTreeItem item)
            {
                if (item.Children.Count == 0)
                    WriteTitle(item);
                else
                {
                    OpenBlock(item);

                    for (int i = 0; i < item.Children.Count; i++)
                        RenderItem(item.Children[i]);

                    CloseBlock();
                }
            }

            void OpenBlock(PathTreeItem item)
            {
                WriteTitle(item);
                IndentationLevel++;

                // Write the opening "|" on the block.
                WriteIndentation();
                Interface.WriteLine();
            }

            private void WriteTitle(PathTreeItem item)
            {
                WriteIndentation();

                // Don't write the arrow if we're on level 0.
                if (IndentationLevel != 0) Interface.Write("-> ", InterfaceColor.Unimportant);
                ExpressionTextRenderer.Render(item.Item);

                if (item.Processor != null)
                {
                    Interface.Write(" ");
                    Interface.Write(item.Processor.Title, InterfaceColor.Unimportant);
                }
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

        public static void Render(PathTreeItem root)
        {
            var state = new Renderer();
            state.RenderItem(root);
        }
    }
}
