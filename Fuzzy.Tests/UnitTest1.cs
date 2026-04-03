using NUnit.Framework;
using Imagibee;

#pragma warning disable NUnit2005

public class Tests
{
    const double ALLOWEDERROR = 1e-6;

    [Test]
    public void TestBehavedInputConstructor()
    {
        Fuzzy.Input input = new(-3, -2, -1, 0);
        Assert.AreEqual(-3, input.X1);
        Assert.AreEqual(-2, input.X2);
        Assert.AreEqual(-1, input.X3);
        Assert.AreEqual(0, input.X4);
    }

    [Test]
    public void TestInvalidInputConstructor()
    {
        Assert.Throws<ArgumentException>(
            () => new Fuzzy.Input(-3, -1, -2, 0));
    }

    [Test]
    public void TestDefaultInputConstructor()
    {
        Fuzzy.Input input = new();
        Assert.AreEqual(double.MinValue, input.X1);
        Assert.AreEqual(0, input.X2);
        Assert.AreEqual(0, input.X3);
        Assert.AreEqual(double.MaxValue, input.X4);
    }

    [Test]
    public void TestInputFuzzification()
    {
        Fuzzy.Input input = new(-2, -1, 1, 2);
        Assert.AreEqual(0, input.Fuzzify(-3));
        Assert.AreEqual(0, input.Fuzzify(-2));
        Assert.AreEqual(.5, input.Fuzzify(-1.5));
        Assert.AreEqual(1, input.Fuzzify(-1));
        Assert.AreEqual(1, input.Fuzzify(0));
        Assert.AreEqual(1, input.Fuzzify(1));
        Assert.AreEqual(.5, input.Fuzzify(1.5));
        Assert.AreEqual(0, input.Fuzzify(2));
        Assert.AreEqual(0, input.Fuzzify(3));
    }

    [Test]
    public void TestAND()
    {
        Assert.AreEqual(.2, Fuzzy.AND(.2, .8), ALLOWEDERROR);
        Assert.AreEqual(.2, Fuzzy.AND(.8, .2), ALLOWEDERROR);
#if NET8_0_OR_GREATER
        Assert.AreEqual(.2, Fuzzy.AND(.2, .3, .8), ALLOWEDERROR);
#else
        Assert.AreEqual(.2, Fuzzy.AND(new double[] { .2, .3, .8 }), ALLOWEDERROR);
#endif
    }

    [Test]
    public void TestOR()
    {
        Assert.AreEqual(.8, Fuzzy.OR(.2, .8), ALLOWEDERROR);
        Assert.AreEqual(.8, Fuzzy.OR(.8, .2), ALLOWEDERROR);
#if NET8_0_OR_GREATER
        Assert.AreEqual(.8, Fuzzy.OR(.2, .3, .8), ALLOWEDERROR);
#else
        Assert.AreEqual(.8, Fuzzy.OR(new double[] { .2, .3, .8 }), ALLOWEDERROR);
#endif
    }

    [Test]
    public void TestNOT()
    {
        Assert.AreEqual(.2, Fuzzy.NOT(.8), ALLOWEDERROR);
    }

    [Test]
    public void TestDefuzzify()
    {
        var rules = new Fuzzy.Rule[]
        {
            new(() => 10, () => .5),
            new(() => 30, () => .5),
        };
        Assert.AreEqual(20, Fuzzy.Defuzzify(rules));
    }

    [Test]
    public void TestDefuzzifyZero()
    {
        var rules = new Fuzzy.Rule[]
        {
            new(() => 0, () => 0),
        };
        Assert.AreEqual(0, Fuzzy.Defuzzify(rules));
    }

    [Test]
    public void DefineInputsByPeaksTest()
    {
#if NET8_0_OR_GREATER
        var results = Fuzzy.DefineInputsByPeaks(
            -4, 7,
            new(new(), -3, -2),
            new(new(), 0, 1));
#else
        var results = Fuzzy.DefineInputsByPeaks(
            -4,
            7,
            new Fuzzy.PeakDefinition[]
            {
                new(new(), -3, -2),
                new(new(), 0, 1),
            });
#endif
        Assert.AreEqual(-4, results[0].X1);
        Assert.AreEqual(-3, results[0].X2);
        Assert.AreEqual(-2, results[0].X3);
        Assert.AreEqual(0, results[0].X4);
        Assert.AreEqual(-2, results[1].X1);
        Assert.AreEqual(0, results[1].X2);
        Assert.AreEqual(1, results[1].X3);
        Assert.AreEqual(7, results[1].X4);
    }

    [Test]
    public void ExampleTest()
    {
        // Tip Results
        MyTipCalculator tip = new()
        {
            LowTip = 7.5,
            AverageTip = 15,
            GenerousTip = 25
        };
        Assert.AreEqual(25, tip.Calculate(5, 3), ALLOWEDERROR);
        Assert.AreEqual(20, tip.Calculate(4, 3), ALLOWEDERROR);
        Assert.AreEqual(17.5, tip.Calculate(3.5, 3), ALLOWEDERROR);
        Assert.AreEqual(15, tip.Calculate(3, 3), ALLOWEDERROR);
        Assert.AreEqual(14.1666666, tip.Calculate(3.5, 2), ALLOWEDERROR);
        Assert.AreEqual(12.5, tip.Calculate(3, 2), ALLOWEDERROR);
        Assert.AreEqual(11.25, tip.Calculate(3, 1), ALLOWEDERROR);
        Assert.AreEqual(10, tip.Calculate(2, 1), ALLOWEDERROR);
        Assert.AreEqual(7.5, tip.Calculate(1, 1), ALLOWEDERROR);
        tip.LowTip = 10;
        Assert.AreEqual(10, tip.Calculate(1, 1), ALLOWEDERROR);
    }

    [Test]
    public void FuzzifyTest()
    {
        Fuzzy.Input ServiceWasExcellent = new(3, 5, double.MaxValue, double.MaxValue);
        Fuzzy.Input ServiceWasOk = new(1, 3, 3, 5);
        Fuzzy.Input ServiceWasPoor = new(double.MinValue, double.MinValue, 1, 3);
#if NET8_0_OR_GREATER
        Fuzzy.InputGroup Service = new(
            ServiceWasPoor,
            ServiceWasOk,
            ServiceWasExcellent);
#else
        var Service = new Fuzzy.InputGroup(
            new Fuzzy.Input[]
            {
                ServiceWasPoor,
                ServiceWasOk,
                ServiceWasExcellent
            });
#endif
        Service.Fuzzify(3);
        Assert.AreEqual(0, ServiceWasExcellent.FX);
        Assert.AreEqual(1, ServiceWasOk.FX);
        Assert.AreEqual(0, ServiceWasPoor.FX);
    }

    public class MyTipCalculator
    {
        // Calculator properties
        public double LowTip;
        public double AverageTip;
        public double GenerousTip;

        // Storage for the inputs and rules
        readonly Fuzzy.Input serviceWasExcellent;
        readonly Fuzzy.Input serviceWasOk;
        readonly Fuzzy.Input serviceWasPoor;
        readonly Fuzzy.Input foodWasTerrible;
        readonly Fuzzy.InputGroup service;
        readonly Fuzzy.Rule[] rules;

        // Construct a MyTipCalculator
        public MyTipCalculator()
        {
            // Define membership function for 1-5 star service rating (5 stars = best)
            //
            // serviceWasExcellent
            //    (FX)
            //     |
            // 1.0 |                     ----
            //     |                   /
            //     |                 /
            //     |               /
            //     |             /
            // 0.0 | -----------
            // ___________________________________ service stars (X)
            //     |    1   2   3   4   5
            serviceWasExcellent = new Fuzzy.Input(3, 5, double.MaxValue, double.MaxValue);

            // serviceWasOk
            //    (FX)
            //     |
            // 1.0 |            -
            //     |           /  \
            //     |         /      \
            //     |       /          \
            //     |     /              \
            // 0.0 | ---                 ----
            // ___________________________________ service stars (X)
            //     |    1   2   3   4   5
            serviceWasOk = new Fuzzy.Input(1, 3, 3, 5);

            // serviceWasPoor
            //    (FX)
            //     |
            // 1.0 | ---
            //     |     \
            //     |       \
            //     |         \
            //     |           \
            // 0.0 |             -----------
            // ___________________________________ service stars (X)
            //     |    1   2   3   4   5
            serviceWasPoor = new Fuzzy.Input(double.MinValue, double.MinValue, 1, 3);

            // Define membership function for 1-5 star food rating (5 stars = best)
            //
            // foodWasTerrible
            //    (FX)
            //     |
            // 1.0 | ---
            //     |     \
            //     |       \
            //     |         \
            //     |           \
            // 0.0 |             -----------
            // ___________________________________ food stars (X)
            //     |    1   2   3   4   5
            foodWasTerrible = new Fuzzy.Input(double.MinValue, double.MinValue, 1, 3);

            // Define the fuzzy rules
            rules = new Fuzzy.Rule[]
            {
                new(() => GenerousTip, () => serviceWasExcellent.FX),
                new(() => AverageTip, () => serviceWasOk.FX),
                new(() => LowTip, () => Fuzzy.OR(serviceWasPoor.FX, foodWasTerrible.FX))
            };

            // Define an input group for serviceStars (optional, for convenience only)
#if NET8_0_OR_GREATER
            service = new Fuzzy.InputGroup(serviceWasPoor, serviceWasOk, serviceWasExcellent);
#else
            service = new Fuzzy.InputGroup(
                new Fuzzy.Input[]
                {
                    serviceWasPoor,
                    serviceWasOk,
                    serviceWasExcellent
                });
#endif
        }

        // Calculate a new tip value based on service rating and food rating
        public double Calculate(double serviceStars, double foodStars)
        {
            // Fuzzify 1-5 star service rating
            service.Fuzzify(serviceStars);

            // Fuzzify 1-5 star food rating
            foodWasTerrible.Fuzzify(foodStars);

            // Defuzzify rules and return the physical tip value
            return Fuzzy.Defuzzify(rules);
        }
    }
}