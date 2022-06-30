using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Processors.Core
{
    internal class Factoriser : Processor
    {
        public override TreeItem Perform(TreeItem itm)
        {
            // QUICK NOTE: There's literally zero point that I can see in NOT doing a full factorisation of all terms.
            // So we always fully factorise here.
            AdditiveLine line = (AdditiveLine)itm;

            // Iterate through each term and create a list of shared things between them.
            List<SharedPiece> shared = new();
            for (int i = 0; i < line.Items.Count; i++)
            {

            }

            return null!;
        }

        public static int GCD(int a, int b)
        {
            while ((a % b) > 0)
            {
                int val = a % b;
                a = b;
                b = val;
            }
            return b;
        }

        public struct SharedPiece
        {
            public bool IsNumber;

            // If it's a tree that's shared: What tree is it?
            public TreeItem Tree;

            // If it's a number: What's the greatest common divisor between the numbers currently?
            public int GCD;
        }

        public enum SharedPieceType
        {
            Number,
            Tree
        }
    }
}
