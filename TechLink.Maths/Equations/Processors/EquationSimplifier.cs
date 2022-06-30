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
        TreeItem _rootItm;
        PathTreeItem _rootPath;

        public EquationSimplifier(TreeItem itm)
        {
            _rootPath = new(itm);
            _rootItm = itm;
        }


        public void SimplifyItem(TreeItem originalItm, TreeItem root)
        {
            // for (var itm in items)
            // {
            //     List<PathTreeItem> possibilities = 
            //     ...Per-item stuff...
            // }
            //
            // 
        }

        public PathTreeItem Simplify()
        {
            ProcessPath(_rootItm, _rootPath);
            return _rootPath;
        }

        public void ProcessPath(TreeItem currentTree, PathTreeItem path)
        {
            var exp = new TreeExplorer(currentTree, true);
            exp.IterateUp((isRoot, itm) =>
            {
                var processors = GetProcessors(itm);
                foreach (var processor in processors)
                {
                    TreeItem processed = processor.Perform(itm);
                    if (!processed.Equals(itm))
                    {
                        // If the path doesn't already exist, process it!
                        if (!AddToPathAndProcessParent(processed, out PathTreeItem? newPath))
                        {
                            ProcessPath(newPath.Item, newPath);

                            // If this processor was "required", don't bother processing alternatives, only go forward with the path we just did with this processor.
                            // This is a performance (and sanity when debugging) feature for really basic operations that just have no reason to explore alternatives.
                            // E.g. Number fold or simple tree moves like "(x + y) + 3" to "x + y + 3", where there's literally zero point in *not* doing them.
                            if (processor.Required)
                            {
                                exp.CancelIteration();
                                return;
                            }
                        }
                    }
                }

                // Returns: Already exists in path
                bool AddToPathAndProcessParent(TreeItem newItm, [NotNullWhen(false)]out PathTreeItem? newPath)
                {
                    newPath = null;

                    // Create a new tree with this item swapped
                    TreeItem newTree = isRoot ? newItm : currentTree.Clone();
                    if (!isRoot) exp.SetCurrentItemInTree(newTree, newItm);

                    // If it's in the tree, don't process further
                    if (CheckIsInPathTree(newTree)) return true;

                    newPath = new PathTreeItem(newTree);
                    path.Children.Add(newPath);

                    return false;
                }
            });
        }

        public int DetermineDeepestLevel(TreeItem itm)
        {
            var children = GetItemChildren(itm);

            if (children.Count == 0) return 1;

            int max = 0;
            foreach (var child in children)
            {
                int childDepth = DetermineDeepestLevel(child);
                if (childDepth > max) max = childDepth;
            }

            return max + 1;
        }

        public void SimplifyItem(TreeItem originalItm, TreeItem root, PathTreeItem? originalPath, Func<TreeItem, TreeItem> getParentIn, Func<TreeItem, TreeItem> getCurrentItemIn, Action<TreeItem, TreeItem> swapInParent, Action<TreeItem, PathTreeItem?> processParent, bool isRoot = false)
        {
            // If there's no children to simplify, do nothing.
            if (originalItm is Number or Variable)
            {
                processParent(getParentIn(root), originalPath);
                return;
            }

            // The way this algorithm works is it simplifies all the children FIRST, then via this callback works its way back up and
            // simplifies the parents for every different combination of simplification the children could get.
            CreatePathsForChildren(originalItm, root, originalPath, getCurrentItemIn, (itm, path) =>
            {
                

                
            });
        }

        public void CreatePathsForChildren(TreeItem itm, TreeItem root, PathTreeItem? path, Func<TreeItem, TreeItem> getCurrentItemIn, Action<TreeItem, PathTreeItem?> processParent)
        {
            switch (itm)
            {
                case TermLine line:
                    CreateTermLinePath(line, root, path, getCurrentItemIn, processParent);
                    break;
                case AdditiveLine line:
                    CreateAdditiveLinePath(line, root, path, getCurrentItemIn, processParent);
                    break;
            }
        }

        public IList<TreeItem> GetItemChildren(TreeItem itm) => itm switch
        {
            TermLine line => line.Terms,
            AdditiveLine line => line.Items,
            _ => throw new Exception()
        };

        public void CreateTermLinePath(TermLine line, TreeItem root, PathTreeItem? path, Func<TreeItem, TreeItem> getCurrentItemIn, Action<TreeItem, PathTreeItem?> processParent)
        {
            for (int i = 0; i < line.Terms.Count; i++)
                SimplifyItem(line.Terms[i], root, path,
                    getCurrentItemIn,
                    itm => ((TermLine)getCurrentItemIn(itm)).Terms[i],
                    (l, newItm) => ((TermLine)l).Terms[i] = newItm,
                    processParent);
        }

        public void CreateAdditiveLinePath(AdditiveLine line, TreeItem root, PathTreeItem? path, Func<TreeItem, TreeItem> getCurrentItemIn, Action<TreeItem, PathTreeItem?> processParent)
        {
            for (int i = 0; i < line.Items.Count; i++)
                SimplifyItem(line.Items[i], root, path,
                    getCurrentItemIn,
                    itm => ((AdditiveLine)getCurrentItemIn(itm)).Items[i],
                    (l, newItm) => ((AdditiveLine)l).Items[i] = newItm,
                    processParent);
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

        static readonly Processor[] _additiveProcessors = new Processor[] { 
            // Required (must come first)
            new NumberFold(), new SingleItemLineExpander(), new LineInLineExpander() 
        };
        static readonly Processor[] _termLineProcessors = new Processor[] { 
            // Required (must come first)
            new NumberFold(), new SingleItemLineExpander(), new LineInLineExpander(), 
            
            // General:
            new Expander() };
        static readonly Processor[] _divisionProcessors = new Processor[] { 
            // Required (must come first)
            new NumberFold() 
            };

        public IList<Processor> GetProcessors(TreeItem itm) => itm switch
        {
            AdditiveLine => _additiveProcessors,
            TermLine => _termLineProcessors,
            Division => _divisionProcessors,
            _ => Array.Empty<Processor>()
        };
    }

    public class PathTreeItem
    {
        public List<PathTreeItem> Children { get; set; } = new List<PathTreeItem>();
        public TreeItem Item { get; set; }

        public PathTreeItem(TreeItem itm) => Item = itm;
    }
}
