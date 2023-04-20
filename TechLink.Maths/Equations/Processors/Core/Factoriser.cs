#define FEATURE_WAIT_FOR_FULL_EXPANSION

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
        // WAIT FOR FULL EXPANSION:
        //
        // Factorising can introduce THOUSANDS of unnecessary paths and there has to be some smart way of reducing them. One such method I'm trialling here is to refuse to factorise terms like:
        // xx(x+1) + x
        // It could turn this into "x(x(x+1) + 1)", but we're going to introduce a new rule that forces it to wait until the "xx(x+1)" is expanded before anything happens:
        // It rejects things with additive lines embedded. This should hopefully help us stop being as silly as we are in so many places.
        // If factorisation isn't working properly, here's what to remove, it's added at the top.
        // Note that this rule DOES NOT wait for full expansion in situation like "2(x+1) + x(x+1)" for obvious reasons.
        //
        // TODO: Investigate fractional divisions.
        public override TreeItem Perform(TreeItem itm)
        {
            AdditiveLine line = (AdditiveLine)itm;

            if (line.Items.Count is 0 or 1) return line;

#if FEATURE_WAIT_FOR_FULL_EXPANSION
            // If we have any additive lines embedded directly in there, like "xx(x+1) + x", flag them up in this list, and we'll reject anything that's not using it as a shared term.
            var expandableTerms = GetWFFERequirementTerms(line);
#endif

            // Test out every combination, starting from the widest to the narrowest, and as soon as we find the best one for a given combination size, apply it!
            int currentMaxSharedCount = 0;
            int currentMaxSharedIdx = 0;
            var iter = new AdditiveCombinationIterator(line);
            while (iter.NextCombinationSize())
            {
                List<TestInfo> tests = new();

                while (iter.NextCombination())
                {
                    // Populate the info
                    // TODO: This is broken, (x + 1)(x + 3) / (x + 2)(x + -1(3)) - actually, is it?
                    var newInfo = new TestInfo();
                    if (!TestTerms(line, ref iter, newInfo)) continue;
                    newInfo.Combination = iter.GetCurrentCombination();

#if FEATURE_WAIT_FOR_FULL_EXPANSION
                    if (ContainsNonSharedExpandableItems(newInfo)) continue;
#endif

                    // Update our max tracking
                    int thisSharedCount = newInfo.Shared.Count + (newInfo.SharedNumGCD == 1 ? 0 : 1);
                    if (thisSharedCount > currentMaxSharedCount)
                    {
                        currentMaxSharedCount = thisSharedCount;
                        currentMaxSharedIdx = tests.Count;
                    }
                    
                    tests.Add(newInfo);
                }

                // If we found something at this size, use that.
                if (currentMaxSharedCount > 0)
                {
                    var info = tests[currentMaxSharedIdx];
                    iter.ResetToNoCopy(info.Combination);
                    return CreateResult(line, ref iter, info);
                }
            }

#if FEATURE_WAIT_FOR_FULL_EXPANSION
            bool ContainsNonSharedExpandableItems(TestInfo info)
            {
                if (expandableTerms == null) return false;

                for (int i = 0; i < expandableTerms.Count; i++)
                {
                    // If it's in one of the shared, it's fine, otherwise, stop here.
                    for (int j = 0; j < info.Shared.Count; j++)
                        if (info.Shared[j].Item.Equals(expandableTerms[i]))
                            goto Fine;

                    return true;

                Fine:
                    continue;
                }
                    

                return false;
            }
#endif

            // We found nothing at any size, stop here
            return itm;
        }

        static List<TreeItem>? GetWFFERequirementTerms(AdditiveLine line)
        {
            List<TreeItem>? res = null;

            foreach (var itm in line.Items)
                if (itm is TermLine innerLine)
                    for (int i = 0; i < innerLine.Terms.Count; i++)
                        if (innerLine.Terms[i] is AdditiveLine addLine)
                        {
                            res ??= new List<TreeItem>();
                            res.Add(addLine);
                        }

            return res;
        }

        public class TestInfo
        {
            public AdditiveCombination Combination;
            public long SharedNumGCD;
            public List<SharedTreeItem> Shared = null!;

            public TestInfo() { }
            public TestInfo(AdditiveCombination comb, List<SharedTreeItem> shared) => (Combination, SharedNumGCD, Shared) = (comb, 1, shared);
        }

        public bool TestTerms(AdditiveLine line, ref AdditiveCombinationIterator iter, TestInfo info)
        {
            // Process the first item
            if (FillInfoFromFirstItem(ref iter, info))
                return false;

            // Filter down the info from there - stopping if we encounter any 0 items.
            foreach (var itm in iter.EnumerateCurrent(true))
                if (FilterInfo(itm, info)) 
                    return false;

            return true;
        }

        private static TreeItem CreateResult(AdditiveLine line, ref AdditiveCombinationIterator iter, TestInfo info)
        {
            // Create a new additive line to represent the terms divided.
            AdditiveLine dividedLine = CreateDividedLine(ref iter, info.SharedNumGCD, info.Shared);

            var res = new TermLine();

            // Add the GCD
            if (info.SharedNumGCD != 1) res.Terms.Add(new Number(info.SharedNumGCD));

            // Add the shared items
            for (int i = 0; i < info.Shared.Count; i++)
                res.Terms.Add(info.Shared[i].Item);

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

        private static bool FilterInfo(TreeItem item, TestInfo info)
        {
            if (item is TermLine termLine)
            {
                // First, find the number in it and update the GCD as necessary.
                if (UpdateGCDFromNum(ref info.SharedNumGCD, termLine)) return true;

                // Then, update the shared items from there
                UpdateSharedItms(info.Shared, termLine);
            }
            else
                if (UpdateForSingleItem(item, ref info.SharedNumGCD, info.Shared)) 
                    return true;

            return false;
        }

        private static bool FillInfoFromFirstItem(ref AdditiveCombinationIterator iter, TestInfo info)
        {
            long? currentGcd = null;

            TreeItem first = iter.GetFirst();
            switch (first)
            {
                case TermLine firstLine:
                    info.Shared = new List<SharedTreeItem>(firstLine.Terms.Count);

                    for (int i = 0; i < firstLine.Terms.Count; i++)
                        if (firstLine.Terms[i] is Number number)
                        {
                            if (number.Value == 0) goto ZeroExit;
                            currentGcd = SetOrMultiply(currentGcd, number);
                        }
                        else
                            info.Shared.Add(new SharedTreeItem(firstLine.Terms[i]));

                    break;
                case Number num:
                    info.Shared = new List<SharedTreeItem>();

                    if (num.Value == 0) goto ZeroExit;
                    currentGcd = num.Value;
                    break;
                default:
                    info.Shared = new List<SharedTreeItem>() { new SharedTreeItem(first) };
                    break;
            }

            info.SharedNumGCD = currentGcd == null ? 1 : Math.Abs(currentGcd.Value);
            return false;

        ZeroExit:
            info.SharedNumGCD = 0;
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

            sharedNumGCD = GCD(sharedNumGCD, Math.Abs(num.Value));
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
                    if (!sharedItms[j].Mark && termLine.Terms[i].Equals(sharedItms[j].Item))
                    {
                        sharedItms[j] = new SharedTreeItem(sharedItms[j], true);
                        break;
                    }

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

                sharedNumGCD = GCD(sharedNumGCD, Math.Abs(num.Value));
            }    
            else
            {
                // The common GCD is now 1.
                sharedNumGCD = 1;

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
