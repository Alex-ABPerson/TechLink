using System;
using System.Collections.Generic;
using System.Text;

namespace TechLink.Core.Models
{
    public class Fraction
    {
        public int Numerator { get; set; }
        public int Denominator { get; set; }

        public Fraction(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        static int GCF(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public Fraction Simplify()
        {
            var gcf = GCF(Denominator, Numerator);

            return new Fraction(Numerator / gcf, Denominator / gcf);
        }

        public Fraction Add(Fraction second)
        {
            if (Denominator == second.Denominator)
                return new Fraction(Numerator + second.Numerator, Denominator);
            else
                return new Fraction((Numerator * second.Denominator) + (second.Numerator * Denominator), Denominator * second.Denominator);
        }

        public Fraction Subtract(Fraction second)
        {
            if (Denominator == second.Denominator)
                return new Fraction(Numerator - second.Numerator, Denominator);
            else
                return new Fraction((Numerator * second.Denominator) - (second.Numerator * Denominator), Denominator * second.Denominator);
        }

        public Fraction Multiply(Fraction second)
        {
            return new Fraction(Numerator * second.Numerator, Denominator * second.Denominator);
        }

        public Fraction Divide(Fraction second)
        {
            return new Fraction(Numerator * second.Denominator, Denominator * second.Numerator);
        }

        public static Fraction FromNumber(string number)
        {
            var decimalPoint = number.IndexOf('.');
            var hasDecimalPoint = decimalPoint != -1;
            var withoutDecimal = hasDecimalPoint ? number.Substring(0, decimalPoint) + number.Substring(decimalPoint + 1, number.Length - decimalPoint - 1) : number;

            return From10sGeneration(withoutDecimal, hasDecimalPoint, number.Length - decimalPoint);
        }

        public static Fraction From10sGeneration(string number, bool hasDecimalPoint, int decimalPosition)
        {
            var numerator = int.Parse(number);
            var denominator = hasDecimalPoint ? (int)Math.Pow(10, decimalPosition - 1) : 1;
            return new Fraction(numerator, denominator);
        }
    }
}
