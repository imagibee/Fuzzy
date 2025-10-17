using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Imagibee
{
    // A lightweight fuzzy logic library inspired by Mamdani
    public static class Fuzzy
    {
        // The Input class is used for defining trapezoidal, triangular, or box
        // membership functions and fuzzifying physical values.
        //
        // It maps physical values (X) to fuzzy values (FX) in the range of
        // 0 to 1 where the four values of x determine the shape of the
        // membership function.
        public class Input
        {
            public double FX { get; internal set; } = double.NaN;
            public double X1 { get; internal set; }
            public double X2 { get; internal set; }
            public double X3 { get; internal set; }
            public double X4 { get; internal set; }

            // Construct a fuzzy input
            //
            // The membership function trapezoid is defined by the four values of x
            // where x2 >= x1, x3 >= x2, and x4 >= x3.
            //
            // Trapezoidal when x2 > x1, x3 > x2, x4 > x3
            // Trianglular when  x2 > x1, x3 == x2, x4 > x3
            // Infinite left when x1 == double.MinValue
            // Infinite right when x4 == double.MaxValue
            //
            // x1 - the value of x where the left valley of the membership
            // trapezoid starts ascending from 0
            //
            // x2 - the value of x where the left peak of the membership
            // trapezoid reaches 1
            //
            // x3 - the value of x where the right peak of the membership
            // trapezoid starts descending from 1
            //
            // x4 - the value of x where the right valley of the membership
            // trapezoid reaches 0
            public Input(double x1, double x2, double x3, double x4)
            {
                if (x2 < x1 || x3 < x2 || x4 < x3)
                {
                    throw new ArgumentException("input thresholds must be ascending");
                }
                X1 = x1;
                X2 = x2;
                X3 = x3;
                X4 = x4;
            }
            public Input() : this(double.MinValue, 0, 0, double.MaxValue) { }

            // Map the physical value x to a fuzzy value fx
            public double Fuzzify(double x)
            {
                if (x <= X1)
                {
                    FX = 0;
                }
                else if (x <= X2)
                {
                    FX = (x - X1) / (X2 - X1);
                }
                else if (x <= X3)
                {
                    FX = 1;
                }
                else if (x <= X4)
                {
                    FX = 1 - ((x - X3) / (X4 - X3));
                }
                else
                {
                    FX = 0;
                }
                return FX;
            }
        }

        // InputGroup provides a convenient way to map a group of inputs
        // to the same physical value
        public class InputGroup
        {
            readonly List<Input> group;

            public InputGroup(List<Input> inputs)
            {
                group = new();
                foreach (var input in inputs)
                {
                    group.Add(input);
                }
            }

            // Map the physical value x to a fuzzy value fx for each input in
            // the group
            public void Fuzzify(double x)
            {
                foreach (var input in group)
                {
                    input.Fuzzify(x);
                }
            }
        }

        // Rule provides a way to combine fuzzy inputs into IF/THEN rules
        public class Rule
        {
            // Construct a Rule
            //
            // x - the physical value that equates to the rule output
            //
            // rfx - a fuzzy IF/THEN rule expressed as an anonymous function
            // that operates on one or more fuzzy inputs
            public Rule(double x, Func<double> rfx)
            {
                X = x;
                RFX = rfx;
            }
            public Func<double> RFX { get; private set; }
            public double X { get; private set; }
        }

        // Used by DefineInputsByPeak
        public class PeakDefinition
        {
            public PeakDefinition(Input i, double x2, double x3)
            {
                I = i;
                X2 = x2;
                X3 = x3;
            }
            public Input I;
            public double X2;
            public double X3;
        }

        // AND of fuzzified values
        public static double AND(double fx1, double fx2)
        {
            return fx1 < fx2 ? fx1 : fx2;
        }
        public static double AND(IEnumerable<double> fxs)
        {
            double fxMin = double.MaxValue;
            foreach (var fx in fxs)
            {
                if (fx < fxMin)
                {
                    fxMin = fx;
                }
            }
            return fxMin;
        }

        // OR of fuzzified values
        public static double OR(double fx1, double fx2)
        {
            return fx1 > fx2 ? fx1 : fx2;
        }
        public static double OR(IEnumerable<double> fxs)
        {
            double fxMax = double.MinValue;
            foreach (var fx in fxs)
            {
                if (fx > fxMax)
                {
                    fxMax = fx;
                }
            }
            return fxMax;
        }

        // NOT of a fuzzified value
        public static double NOT(double fx)
        {
            return 1 - fx;
        }

        // DefuzzifyByCentroid defuzzifies rules back to a physical value by
        // using the centroid method
        public static double DefuzzifyByCentroid(IList<Rule> rfxs)
        {
            double nx = 0;
            double dx = 0;
            foreach (var rfx in rfxs)
            {
                var fx = rfx.RFX();
                nx += fx * rfx.X;
                dx += fx;
            }
            if (dx == 0 && nx == 0)
            {
                // Define numertor == denominator == 0 as zero
                return 0;
            }
            else
            {
                return nx / dx;
            }
        }

        // DefineInputsByPeaks is an even more convenient way to define inputs
        //
        // Given only the peaks (X2, X3) the valleys (X1, X4) are inferred.
        // The valleys are figured so that the slopes of adjacent peaks cross
        // in the middle and reach zero at the adjacent peaks.  Peaks must be
        // given in ascending order.
        //
        // startValley - the X1 value of first input be specified (use
        // double.MinValue for infinite left)
        //
        // inDefs - defines the PeakDefinition for each input
        //
        // endValley - the X4 valley of the last input be specified (use
        // double.MaxValue for infinite right)
        public static IList<Input> DefineInputsByPeaks(
            double startValley,
            IList<PeakDefinition> inDefs,
            double endValley)
        {
            List<Input> result = new();
            double lastPeak = startValley;
            for (var i = 0; i < inDefs.Count; i++)
            {
                if (i < inDefs.Count - 1)
                {
                    inDefs[i].I.X1 = lastPeak;
                    inDefs[i].I.X2 = inDefs[i].X2;
                    inDefs[i].I.X3 = inDefs[i].X3;
                    inDefs[i].I.X4 = inDefs[i + 1].X2;
                    lastPeak = inDefs[i].X3;
                }
                else
                {
                    inDefs[i].I.X1 = lastPeak;
                    inDefs[i].I.X2 = inDefs[i].X2;
                    inDefs[i].I.X3 = inDefs[i].X3;
                    inDefs[i].I.X4 = endValley;
                }
                result.Add(inDefs[i].I);
            }
            return result;
        }
    }
}