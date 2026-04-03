using System;
using System.Diagnostics;
using System.Collections.Generic;

#pragma warning disable 8600, 8603

namespace Imagibee
{
    // A lightweight C# library for implementing efficient fuzzy logic controllers
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
            // Box starting at x1 when x2 == x1
            // Box ending at x4 when x3 == x4
            // Infinite box left when x2 == x1 == double.MinValue
            // Infinite box right when x3 == x4 == double.MaxValue
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

        // Map the physical value x to a fuzzy value fx
#if NET8_0_OR_GREATER
        public static void Fuzzify(double x, params Input[] inputs)
#else
        public static void Fuzzify(double x, Input[] inputs)
#endif
        {
            foreach (var i in inputs)
            {
                i.Fuzzify(x);
            }
        }

        // A Rule provides a way to express a physical output value in terms of
        // fuzzy membership.  Rules are defined as lambda expressions to take advantage
        // of the dynamic nature of the closure. Rules will be calculated using the
        // values of the parameters enclosed by the lambda at the time of evaluation,
        // not at the time of construction.
        //
        // In a typical scenario, a set of rules is constructed for a control system.
        // These rules are then invoked periodically using Defuzzify to
        // re-calculate the output using the current value of the inputs.
        public class Rule
        {
            public Input I { get; private set; }
            public Func<double> Y { get; private set; }

            // i - the fuzzy input that belongs to the rule.
            //
            // y - an anonymous function that defines a physical output value.  In many
            // scenarios y will be a constant value (zero-order).  In other scenarios it
            // may be variable; for example, a threshold value changes depending on the
            // state of the system.
            public Rule(Input i, Func<double> y)
            {
                I = i;
                Y = y;
            }
        }

        // Define peaks used by DefineInputsByPeak
        public class PeakDefinition
        {
            public Input I;
            public double X2;
            public double X3;

            public PeakDefinition(Fuzzy.Input i, double x2, double x3)
            {
                I = i;
                X2 = x2;
                X3 = x3;
            }
        }

        // AND of fuzzified values
        public static Input AND(Input i1, Input i2)
        {
            return i1.FX < i2.FX ? i1 : i2;
        }
#if NET8_0_OR_GREATER
        public static Input AND(params Input[] inputs)
#else
        public static Input AND(Input[] inputs)
#endif
        {
            Input iMin = null;
            foreach (var i in inputs)
            {
                if (iMin == null || i.FX < iMin.FX)
                {
                    iMin = i;
                }
            }
            return iMin;
        }

        // OR of fuzzified values
        public static Input OR(Input i1, Input i2)
        {
            return i1.FX > i2.FX ? i1 : i2;
        }
#if NET8_0_OR_GREATER
        public static Input OR(params Input[] inputs)
#else
        public static Input OR(Input[] inputs)
#endif
        {
            Input iMax = null;
            foreach (var i in inputs)
            {
                if (iMax == null || i.FX > iMax.FX)
                {
                    iMax = i;
                }
            }
            return iMax;
        }
        // NOT of a fuzzified value
        public static Input NOT(Input i)
        {
            Input result = new(i.X1, i.X2, i.X3, i.X4)
            {
                FX = 1 - i.FX
            };
            return result;
        }

        // Defuzzify defuzzifies rules back to a physical value
#if NET8_0_OR_GREATER
        public static double Defuzzify(params Rule[] rules)
#else
        public static double Defuzzify(Rule[] rules)
#endif
        {
            double nx = 0;
            double dx = 0;
            foreach (var rule in rules)
            {
                var fx = rule.I.FX;
                nx += fx * rule.Y();
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

        // DefineInputsByPeaks simplifies creating a set of Input for the
        // common case that the peaks and valleys of adjacent inputs are
        // aligned to the same X values.
        //
        // Given only the peaks (X2, X3) the valleys (X1, X4) are inferred.
        // The valleys are figured so that the slopes of adjacent peaks cross
        // in the middle and reach zero at the adjacent peaks.  Peaks must be
        // given in ascending order.
        //
        // startValley - the X1 value of first input being specified (use
        // double.MinValue for infinite left)
        //
        // endValley - the X4 value of the last input being specified (use
        // double.MaxValue for infinite right)
        //
        // inDefs - defines the PeakDefinition for each input
#if NET8_0_OR_GREATER
        public static Input[] DefineInputsByPeaks(
            double startValley,
            double endValley,
            params PeakDefinition[] inDefs)
#else
        public static Input[] DefineInputsByPeaks(
            double startValley,
            double endValley,
            PeakDefinition[] inDefs)
#endif
        {
            var result = new Input[inDefs.Length];
            double lastPeak = startValley;
            for (var i = 0; i < inDefs.Length; i++)
            {
                if (i < inDefs.Length - 1)
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
                result[i] = inDefs[i].I;
            }
            return result;
        }
    }
}