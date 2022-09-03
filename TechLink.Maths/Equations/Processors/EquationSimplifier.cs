using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechLink.Maths.Equations.Helpers;
using TechLink.Maths.Equations.Processors.Core;

namespace TechLink.Maths.Equations.Processors
{
    public class EquationSimplifier
    {
        // TODO: Write fractional removing processor, so you can have "x+1/(1/2)x+(1/2)" be turned into "x+1/2(x+1)" and therefore simplified.
        TreeItem _rootItm;
        PathTreeItem _rootPath;

        public EquationSimplifier(TreeItem itm)
        {
            _rootPath = new(itm);
            _rootItm = itm;
        }

        public PathTreeItem Simplify()
        {
            ProcessPath(_rootItm, _rootPath);
            return _rootPath;
        }

        public void ProcessPath(TreeItem currentTree, PathTreeItem path)
        {
            var exp = new TreeExplorer(currentTree, true);

            // There are two types of processors here: Required and normal.
            // With required processors, we don't bother processing alternatives, and only go forward with the path we just did with the processor.
            // This is a performance (and sanity when debugging) feature for basic operations that just have no reason to explore alternatives.
            // E.g. Number fold or simple tree moves like "(x + y) + 3" to "x + y + 3", where there's literally zero point in *not* doing them.

            // Process required processors across the whole thing first - and don't go any further if they're applied
            bool requiredApplied = false;
            exp.IterateUp((isRoot, itm) => requiredApplied = ProcessSet(itm, isRoot, GetRequiredProcessors(itm), true));

            if (requiredApplied) return;

            // Provided no required changes were made, process everything else
            exp.Reset(currentTree);
            exp.IterateUp((isRoot, itm) => ProcessSet(itm, isRoot, GetProcessors(itm), false));

            bool ProcessSet(TreeItem itm, bool isRoot, IList<Processor> processors, bool isRequired)
            {
                foreach (var processor in processors)
                {
                    TreeItem processed = processor.Perform(itm);

                    // Process the result(s)
                    if (processed is MultiTreeItem multi)
                    {
                        foreach (var res in multi.SubItems)
                            if (HandleProcessorResult(res, processor)) 
                                return true;
                    }
                    else
                    {
                        if (HandleProcessorResult(processed, processor)) 
                            return true;
                    }
                }

                return false;

                bool HandleProcessorResult(TreeItem res, Processor processor)
                {
                    if (res.Equals(itm)) return false;

                    // If the path doesn't already exist, process it!
                    if (!AddToPathAndProcessParent(res, processor, out PathTreeItem? newPath))
                    {
                        ProcessPath(newPath.Item, newPath);

                        if (isRequired)
                        {
                            exp.CancelIteration();
                            return true;
                        }
                    }

                    return false;
                }

                // Returns: Already exists in path
                bool AddToPathAndProcessParent(TreeItem newItm, Processor proc, [NotNullWhen(false)] out PathTreeItem? newPath)
                {
                    newPath = null;

                    // Create a new tree with this item swapped
                    TreeItem newTree = isRoot ? newItm : currentTree.Clone();
                    if (!isRoot) exp.SetCurrentItemInTree(newTree, newItm);

                    // If it's in the tree, don't process further
                    if (CheckIsInPathTree(newTree)) return true;

                    newPath = new PathTreeItem(newTree, proc);
                    path.Children.Add(newPath);

                    return false;
                }
            }
        }
        public bool CheckIsInPathTree(TreeItem itm)
        {
            return IsInAt(itm, _rootPath);

            static bool IsInAt(TreeItem itm, PathTreeItem path)
            {
                if (path.Item.Equals(itm))
                    return true;

                foreach (var child in path.Children)
                    if (IsInAt(itm, child))
                        return true;

                return false;
            }
        }

        // "Required" processors
        static readonly Processor[] _additiveReqProcessors = new Processor[] { new NumberFold(), new SingleItemLineExpander(), new LineInLineExpander() };
        static readonly Processor[] _termLineReqProcessors = new Processor[] { new NumberFold(), new SingleItemLineExpander(), new LineInLineExpander() };
        static readonly Processor[] _divisionReqProcessors = new Processor[] { new NumberFold() };

        // Normal processors
        static readonly Processor[] _additiveProcessors = new Processor[] { new Factoriser() };
        static readonly Processor[] _termLineProcessors = new Processor[] { new Expander() };

        public IList<Processor> GetProcessors(TreeItem itm) => itm switch
        {
            AdditiveLine => _additiveProcessors,
            TermLine => _termLineProcessors,
            _ => Array.Empty<Processor>()
        };

        public IList<Processor> GetRequiredProcessors(TreeItem itm) => itm switch
        {
            AdditiveLine => _additiveReqProcessors,
            TermLine => _termLineReqProcessors,
            Division => _divisionReqProcessors,
            _ => Array.Empty<Processor>()
        };
    }

    public class PathTreeItem
    {
        public List<PathTreeItem> Children { get; set; } = new List<PathTreeItem>();
        public Processor? Processor { get; set; } = null;
        public TreeItem Item { get; set; }

        public PathTreeItem(TreeItem itm) => Item = itm;
        public PathTreeItem(TreeItem itm, Processor processor) => (Item, Processor) = (itm, processor);
    }
}
