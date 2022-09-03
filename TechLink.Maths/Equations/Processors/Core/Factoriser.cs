using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechLink.Maths.Equations.Helpers;

namespace TechLink.Maths.Equations.Processors.Core
{
    internal class Factoriser : Processor
    {
        public override string Title => "Factorise";

        // NOTE:
        // Alright so, there's been a lot of argument in my head about whether we should always factorise fully here or what, and I've finally come to a conclusion.
        // We should ALWAYS factorise fully, there's no reason not to. There were two examples that came to mind that seemed to prove otherwise but actually they don't.
        //
        // 1. FRACTIONS - If you have "4x+8/2x+4", it seems like you *need* to factorise the top non-fully by "2" to achieve the cancellation
        // but actually, no, you don't, because we can factorise both the top **and** the bottom and they'll come together.
        //
        // 2. Coefficient "matching" - Same thing here, fully factorising everything lines up perfectly.
        //
        // However, there IS a benefit to factorising "-1" so we always do that. Because -1 is a kind of "in-between" state between full factorisation and not, it logically makes sense that this happened.
        // However, there is 100% no doubt benefit to factorising different groups of items, so we do that.
        //
        // TODO: Investigate fractional divisions.
        public override TreeItem Perform(TreeItem itm)
        {
            AdditiveLine line = (AdditiveLine)itm;

            if (line.Items.Count is 0 or 1) return line;

            List<TreeItem> res = new();

            var iter = new AdditiveCombinationIterator(line, 2);
            while (iter.NextCombination())
            {
                TreeItem? fullFactor = PerformOnTerms(line, ref iter);
                if (fullFactor != null) 
                    res.Add(fullFactor);
            }

            return new MultiTreeItem(res);
        }

        // Returns null if there's any zeros present
        public TreeItem? PerformOnTerms(AdditiveLine line, ref AdditiveCombinationIterator iter)
        {
            // Process the first item
            if (CreateInfoFromFirstItem(ref iter, out long sharedNumGCD, out List<SharedTreeItem> sharedItms))
                return null;

            // Filter down the info from there - stopping if we encounter any 0 items.
            foreach (var itm in iter.EnumerateCurrent(true))
                if (FilterInfo(itm, ref sharedNumGCD, sharedItms)) 
                    return null;

            // If there's nothing shared, stop here.
            if (sharedItms.Count == 0 && sharedNumGCD == 1) return null;

            // Create the final result
            return CreateResult(line, ref iter, sharedNumGCD, sharedItms);
        }

        private static TreeItem CreateResult(AdditiveLine line, ref AdditiveCombinationIterator iter, long sharedNumGCD, List<SharedTreeItem> sharedItms)
        {
            // Create a new additive line to represent the terms divided.
            AdditiveLine dividedLine = CreateDividedLine(ref iter, sharedNumGCD, sharedItms);

            var res = new TermLine();

            // Add the GCD
            if (sharedNumGCD != 1) res.Terms.Add(new Number(sharedNumGCD));

            // Add the shared items
            for (int i = 0; i < sharedItms.Count; i++)
                res.Terms.Add(sharedItms[i].Item);

            // Add the divided additive
            res.Terms.Add(dividedLine);

            // If there are any items that are not included in the current combination, wrap this in an additive line and add them to the end of it.
            if (iter.CurrentCombinationSize != line.Items.Count)
            {
                var wrappedRes = new AdditiveLine(res);

                foreach (var itm in iter.EnumerateNonCurrent(false))
                    wrappedRes.Items.Add(itm);

                return wrappedRes;
            }
            else return res;
        }

        private static bool FilterInfo(TreeItem item, ref long sharedNumGCD, List<SharedTreeItem> sharedItms)
        {
            if (item is TermLine termLine)
            {
                // First, find the number in it and update the GCD as necessary.
                if (UpdateGCDFromNum(ref sharedNumGCD, termLine)) return true;

                // Then, update the shared items from there
                UpdateSharedItms(sharedItms, termLine);
            }
            else
                if (UpdateForSingleItem(item, ref sharedNumGCD, sharedItms)) 
                    return true;

            return false;
        }

        private static bool CreateInfoFromFirstItem(ref AdditiveCombinationIterator iter, out long gcdVal, out List<SharedTreeItem> itm)
        {
            long? currentGcd = null;

            TreeItem first = iter.GetFirst();
            switch (first)
            {
                case TermLine firstLine:
                    itm = new List<SharedTreeItem>(firstLine.Terms.Count);

                    for (int i = 0; i < firstLine.Terms.Count; i++)
                        if (firstLine.Terms[i] is Number number)
                        {
                            if (number.Value == 0) goto ZeroExit;
                            currentGcd = SetOrMultiply(currentGcd, number);
                        }
                        else
                            itm.Add(new SharedTreeItem(firstLine.Terms[i]));

                    break;
                case Number num:
                    itm = new List<SharedTreeItem>();

                    if (num.Value == 0) goto ZeroExit;
                    currentGcd = num.Value;
                    break;
                default:
                    itm = new List<SharedTreeItem>() { new SharedTreeItem(first) };
                    break;
            }

            gcdVal = currentGcd ?? 1;
            return false;

        ZeroExit:
            gcdVal = 0;
            return true;
        }

        // Returns: Whether the number found was 0.
        private static bool UpdateGCDFromNum(ref long sharedNumGCD, TermLine termLine)
        {
            long? num = null;
            for (int i = 0; i < termLine.Terms.Count; i++)
                if (termLine.Terms[i] is Number numItm)
                {
                    if (numItm.Value == 0) return true;

                    num = SetOrMultiply(num, numItm);
                }

            if (num == null) num = 1;

            sharedNumGCD = GCD(sharedNumGCD, num.Value);
            return false;
        }

        private static long SetOrMultiply(long? num, Number numItm)
        {
            // If it's not null we're for some reason doing this before the number folder's gotten here... Oh well, just multiply them to achieve the desired effect
            return num == null ? numItm.Value : num.Value * numItm.Value;
        }

        private static void UpdateSharedItms(List<SharedTreeItem> sharedItms, TermLine termLine)
        {
            // Go through the items and "mark" anything that's in the shared list.
            for (int i = 0; i < termLine.Terms.Count; i++)
                for (int j = 0; j < sharedItms.Count; j++)
                    if (termLine.Terms[i].Equals(sharedItms[j].Item))
                        sharedItms[j] = new SharedTreeItem(sharedItms[j], true);

            // Remove the items that didn't get marked
            for (int i = 0; i < sharedItms.Count; i++)
                if (sharedItms[i].Mark)
                    sharedItms[i] = new SharedTreeItem(sharedItms[i], false);
                else
                    sharedItms.RemoveAt(i--);
        }

        private static bool UpdateForSingleItem(TreeItem item, ref long sharedNumGCD, List<SharedTreeItem> sharedItms)
        {
            if (item is Number num)
            {
                // Clearly nothing is shared if this is all there was here
                sharedItms.Clear();

                // Ignore zeros
                if (num.Value == 0) return true;

                sharedNumGCD = GCD(sharedNumGCD, num.Value);
            }    
            else
            {
                // See if this item exists anywhere in the shared items, and if so, keep only that one.
                for (int j = 0; j < sharedItms.Count; j++)
                    if (sharedItms[j].Item.Equals(item))
                    {
                        var keep = sharedItms[j];
                        sharedItms.Clear();
                        sharedItms.Add(keep);
                        return false;
                    }

                // If none of them matched, then there are no shared items!
                sharedItms.Clear();
            }

            return false;
        }

        private static AdditiveLine CreateDividedLine(ref AdditiveCombinationIterator iter, long sharedNumGCD, List<SharedTreeItem> sharedItms)
        {
            AdditiveLine res = new();

            foreach (var itm in iter.EnumerateCurrent(false))
                switch (itm)
                {
                    // For TermLines
                    case TermLine termLine:
                        var items = DivideTermLineBy(termLine, sharedNumGCD, sharedItms);

                        if (items.Count == 0) res.Items.Add(new Number(1));
                        else if (items.Count == 1) res.Items.Add(items[0]);
                        else res.Items.Add(new TermLine(items));

                        break;

                    // For Numbers
                    case Number num:
                        long numDivided = num.Value / (sharedNumGCD == 0 ? 1 : sharedNumGCD);
                        res.Items.Add(new Number(numDivided));
                        break;

                    // For our end result
                    default:
                        if (DividesBy(itm, sharedItms))
                            res.Items.Add(new Number(1));
                        else
                            res.Items.Add(itm);

                        break;
                }

            return res;
        }

        private static List<TreeItem> DivideTermLineBy(TermLine termLine, long numDiv, List<SharedTreeItem> otherDiv)
        {
            long numComp = 1;
            List<TreeItem> items = new();

            // Unmark all of the items
            for (int i = 0; i < otherDiv.Count; i++)
                otherDiv[i] = new SharedTreeItem(otherDiv[i], false);

            // Add all terms that aren't shared + Find coefficient
            for (int i = 0; i < termLine.Terms.Count; i++)
            {
                // Numbers - For coefficient
                if (termLine.Terms[i] is Number num)
                {
                    numComp *= num.Value;
                    continue;
                }

                // Everything else
                bool isShared = false;
                for (int j = 0; j < otherDiv.Count; j++)
                {
                    if (!otherDiv[j].Mark && termLine.Terms[i].Equals(otherDiv[j].Item))
                    {
                        // Mark this item so we don't try this one again
                        otherDiv[j] = new SharedTreeItem(otherDiv[j], true);

                        isShared = true;
                        break;
                    }
                }

                if (!isShared) items.Add(termLine.Terms[i]);
            }

            // Add the coefficient.
            long numCompDivided = numComp / numDiv;
            if (numCompDivided != 1) items.Insert(0, new Number(numCompDivided));

            return items;
        }

        private static bool DividesBy(TreeItem item, List<SharedTreeItem> sharedItms)
        {
            for (int j = 0; j < sharedItms.Count; j++)
                if (item.Equals(sharedItms[j].Item))
                    return true;

            return false;
        }

        public static long GCD(long a, long b)
        {
            while ((a % b) > 0)
            {
                long val = a % b;
                a = b;
                b = val;
            }
            return b;
        }

        public struct SharedTreeItem
        {
            public TreeItem Item;
            public bool Mark;

            public SharedTreeItem(TreeItem item) => (Item, Mark) = (item, false);
            public SharedTreeItem(SharedTreeItem prev, bool newMark) => (Item, Mark) = (prev.Item, newMark);
        }
    }
}
