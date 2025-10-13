using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Imagibee
{
    // A simple fuzzy logic library
    //
    // - Define fuzzy inputs
    // - Fuzzify physical values into fuzzy values
    // - Combine fuzzy values using IF/THEN rules
    // - Defuzzify the rules back to physical values
    static class Fuzzy
    {
        // Converts physical values (X) to fuzzy values (FX) in the range of 0 to 1
        // where the four values of x determine the shape of the fuzzifier.
        public class Input
        {
            public double FX { get; internal set; } = double.NaN;
            public double X1 { get; internal set; }
            public double X2 { get; internal set; }
            public double X3 { get; internal set; }
            public double X4 { get; internal set; }

            // Construct a fuzzy input
            //
            // The fuzzifier shape is defined by the four values of x where
            // x2 >= x1, x3 >= x2, and x4 >= x3.
            //
            // Trapezoidal when x2 > x1, x3 > x2, x4 > x3
            // Trianglular when  x2 > x1, x3 == x2, x4 > x3
            // Rectangular Left when x1 == double.MinValue
            // Rectangular Right when x4 == double.MaxValue
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

            // Fuzzify the physical value x to a fuzzy value fx
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

        // For fuzzifying a group of related inputs
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

            public void Fuzzify(double value)
            {
                foreach (var input in group)
                {
                    input.Fuzzify(value);
                }
            }
        }

        // Construct a fuzzy IF/THEN rule
        //
        // Used during defuzzification to convert fuzzy values back to a physical value
        public class Rule
        {
            // x - the physical value for the rule
            // rfx - a fuzzy IF/THEN rule formed by creating a closure on 1 or more fuzzified Input
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

        // Defuzzify rules to a physical value by centroid method
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

        // Used to simplify input definition.  Given the peaks (X2, X3), infer the valleys (X1, X4).
        //
        // The valleys are figured so that the slopes of adjacent peaks cross in the middle
        // and reach zero at the adjacent peaks.  Peaks must be given in ascending order.
        //
        // Trapezoid - peaks are different
        // Triangle - peaks are the same
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