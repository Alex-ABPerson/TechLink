using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Processors.Core
{
    /// <summary>
    /// Expands inner lines within an outer line into the outer line.
    /// </summary>
    public class LineInLineExpander : Processor
    {
        public override bool Required => true;
        public override string Title => "Tree Op (Line-in-Line)";
        public override TreeItem Perform(TreeItem itm)
        {
            return itm switch
            {
                TermLine line => PerformTermLine(line),
                AdditiveLine additiveLine => PerformAdditiveLine(additiveLine),
                _ => throw new Exception()
            };
        }

        public TreeItem PerformTermLine(TermLine line)
        {
            TermLine? newLine = null;

            for (int i = 0; i < line.Terms.Count; i++)
                if (line.Terms[i] is TermLine innerLine)
                {
                    newLine ??= (TermLine)line.Clone();
                    newLine.Terms.RemoveAt(i);
                    newLine.Terms.AddRange(innerLine.Terms);
                }

            return newLine ?? line;
        }

        public TreeItem PerformAdditiveLine(AdditiveLine line)
        {
            AdditiveLine? newLine = null;

            int posInNewLine = 0;
            for (int i = 0; i < line.Items.Count; i++)
                if (line.Items[i] is AdditiveLine innerLine)
                {
                    newLine ??= (AdditiveLine)line.Clone();
                    newLine.Items.RemoveAt(posInNewLine);
                    newLine.Items.AddRange(innerLine.Items);
                }
                else posInNewLine++;

            return newLine ?? line;
        }
    }
}
