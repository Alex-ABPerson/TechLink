using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechLink.Maths.Equations.Helpers;
using TechLink.Maths.Equations.Processors.Core;
using TechLink.Maths.Equations.Renderers;

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

                //PathTreeRenderer.Render(_rootPath);
                return false;

                // Returns: Should stop
                bool HandleProcessorResult(TreeItem res, Processor processor)
                {
                    if (res.Equals(itm)) return false;

                    // If the path doesn't already exist, process it!
                    // TODO: For xx(x + 6) + (6x + 6(6)) + x3x + 18x + xx2 + 12x, we're skipping a required processor because of
                    // its path already existing
                    var status = TryAddToPath(res, processor, out PathTreeItem? newPath);
                    if (status == ExistingTreeStatus.NewTree)
                        ProcessPath(newPath!.Item, newPath);

                    // Don't bother processing this tree any further if this was a required processor we just applied.
                    // The only time we *don't* cancel after a required processor is if the required processor happened to give back the exact same thing as our current path item right now, which *can* happen
                    // due to "Equals"'s behaviour with ignoring order and such. In those circumstances, stopping could result in us never going anywhere, so we let only that slide.
                    if (isRequired && status != ExistingTreeStatus.IsCurrentTree)
                    {
                        exp.CancelIteration();
                        return true;
                    }

                    return false;
                }

                ExistingTreeStatus TryAddToPath(TreeItem newItm, Processor proc, [NotNullWhen(true)] out PathTreeItem? newPath)
                {
                    newPath = null;

                    // Create a new tree with this item swapped
                    TreeItem newTree = isRoot ? newItm : currentTree.Clone();
                    if (!isRoot) exp.SetCurrentItemInTree(newTree, newItm);

                    // If it's already in the tree, return back and don't make a new tree
                    PathTreeItem? existingPathItm = FindInPathTree(newTree);
                    if (existingPathItm != null) return existingPathItm == path ? ExistingTreeStatus.IsCurrentTree : ExistingTreeStatus.AlreadyExists;

                    newPath = new PathTreeItem(newTree, proc);
                    path.Children.Add(newPath);

                    return ExistingTreeStatus.NewTree;
                }
            }
        }

        public PathTreeItem? FindInPathTree(TreeItem itm)
        {
            return FindIn(_rootPath);

            PathTreeItem? FindIn(PathTreeItem path)
            {
                if (path.Item.Equals(itm))
                    return path;

                foreach (var child in path.Children)
                {
                    PathTreeItem? childTree = FindIn(child);
                    if (childTree != null) return childTree;
                }

                return null;
            }
        }

        // "Required" processors
        static readonly Processor[] _additiveReqProcessors = new Processor[] { new SingleItemLineExpander(), new LineInLineExpander(), new NumberFold() }; // Putting NumberFold last on these is more efficient
        static readonly Processor[] _termLineReqProcessors = new Processor[] { new SingleItemLineExpander(), new LineInLineExpander(), new NumberFold() }; // as there's scenarios it might try to number fold
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

        enum ExistingTreeStatus
        {
            AlreadyExists,
            IsCurrentTree,
            NewTree
        }
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
