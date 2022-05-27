using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            int noLayers = DetermineDeepestLevel(_rootItm);
            for (int i = noLayers - 1; i >= 0; i--)
            {

            }

            if (_rootItm is Number || _rootItm is Variable) return _rootPath;
            SimplifyItem(_rootItm, _rootItm, null, null!, root => root, null!, null!, true);
            return _rootPath;
        }

        public void ProcessLayer(int layer)
        {
            // First process the layer below this one, then process those items

            void AddItemInLayer(TreeItem itm)
            {

            }
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

            // Simplify the children

            // Now, simplify this item.


            // The way this algorithm works is it simplifies all the children FIRST, then via this callback works its way back up and
            // simplifies the parents for every different combination of simplification the children could get.
            CreatePathsForChildren(originalItm, root, originalPath, getCurrentItemIn, (itm, path) =>
            {
                var processors = GetProcessors(itm);
                foreach (var processor in processors)
                {
                    TreeItem processed = processor.Perform(itm);
                    if (!processed.Equals(itm))
                    {
                        if (AddToPathAndProcessParent(processed, out PathTreeItem? newPath)) continue;

                        // Try simplifying this item again, as it may be possible to simplify it or its children further now.
                        SimplifyItem(processed, root, newPath, getParentIn, getCurrentItemIn, swapInParent, processParent, true);
                    }
                }

                // Returns: Already exists in path
                bool AddToPathAndProcessParent(TreeItem newItm, out PathTreeItem? newPath)
                {
                    newPath = null;
                    TreeItem? parent = null;

                    // Create a new tree with this item swapped
                    TreeItem newTree = isRoot ? newItm : root.Clone();
                    if (!isRoot)
                    {
                        parent = getParentIn(newTree);
                        swapInParent(parent, newItm);
                    }

                    if (CheckIsInPathTree(newTree)) return true;

                    // Otherwise, add it to the pathway - if we're the very first simplification to occur, start this in the root of the paths.
                    newPath = new PathTreeItem(newTree);
                    if (path == null)
                        _rootPath.Children.Add(newPath);
                    else
                        path.Children.Add(newPath);

                    // Now that we've processed this item - make sure we also process our parent!
                    if (!isRoot) processParent(parent!, newPath);
                    return false;
                }
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

        static readonly Processor[] _additiveProcessors = new Processor[] { new PassthroughProcessor(), new NumberFold(), new RedundantLineExpander() };
        static readonly Processor[] _termLineProcessors = new Processor[] { new PassthroughProcessor(), new NumberFold(), new RedundantLineExpander() };

        public IList<Processor> GetProcessors(TreeItem itm) => itm switch
        {
            AdditiveLine => _additiveProcessors,
            TermLine => _termLineProcessors,
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
