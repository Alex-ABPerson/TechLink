//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TechLink.Maths.Equations.Processors
//{
//    public class Factorise : Processor
//    {
//        public override List<Possibility> Prepare(TreeItem itm)
//        {
//            if (itm is not AdditiveLine al || al.Items.Count < 2) return null;

//            var res = new List<Possibility>();
            
//            // Try to check all the different combinations of terms
//            // e.g. With (2x + 2xy + 3y), we need to check all possibilities, such as (2x, 2xy), (2x, 3y), (2xy, 3y), (2x, 2xy, 3y)

//            // Start with binomials.
//            for (int x = 0; x < al.Items.Count; x++)
//            {
//                for (int y = 0; y < al.Items.Count; y++)
//                {
//                    if (x == y) continue;



//                    res.Add(new Possibility());
//                }
//            }
//        }

//        public TreeItem GetAllShared(TreeItem first, TreeItem second)
//        {
//            // If the second is a term-line, put it into "first" to simplify some logic.
//            if (second is TermLine line)
//            {
//                // Sanity check: They can't BOTH be TermLines
//                Debug.Assert(first is not TermLine);
//                (first, second) = (second, first);
//            }

//            switch (first)
//            {
//                case TermLine line:
//                    for (int i = 0; i < line.Terms.Count; i++)
//                    {
                        
//                    }

//                    break;
//                case Number num:
//                    if (second is Number number)
//                    {

//                    }

//                    // If it's a term line, push it onto the 

//                    break;
//            }
//        }

//        public override void Perform(TreeItem itm, ProcessingContext ctx)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
