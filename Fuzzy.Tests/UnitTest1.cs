using NUnit.Framework;
using Imagibee;

#pragma warning disable NUnit2005

public class Tests
{
    const double ALLOWEDERROR = 1e-6;
    [SetUp]
    public void Setup()
    {
    }

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
        Fuzzy.Input input = new(-2, 0, 0, 2);
        Assert.AreEqual(0, input.Fuzzify(-3));
        Assert.AreEqual(0, input.Fuzzify(-2));
        Assert.AreEqual(.5, input.Fuzzify(-1));
        Assert.AreEqual(1, input.Fuzzify(0));
        Assert.AreEqual(.5, input.Fuzzify(1));
        Assert.AreEqual(0, input.Fuzzify(2));
        Assert.AreEqual(0, input.Fuzzify(3));
    }

    [Test]
    public void TestAND()
    {
        Assert.AreEqual(.2, Fuzzy.AND(.2, .8), ALLOWEDERROR);
        Assert.AreEqual(.2, Fuzzy.AND(new double[] { .2, .8 }), ALLOWEDERROR);
    }

    [Test]
    public void TestOR()
    {
        Assert.AreEqual(.8, Fuzzy.OR(.2, .8), ALLOWEDERROR);
        Assert.AreEqual(.8, Fuzzy.OR(new double[] { .2, .8 }), ALLOWEDERROR);
    }

    [Test]
    public void TestNOT()
    {
        Assert.AreEqual(.2, Fuzzy.NOT(.8), ALLOWEDERROR);
    }

    [Test]
    public void TestDefuzzifyByCentroid()
    {
        List<Fuzzy.Rule> rules = new()
        {
            new(10, () => .5),
            new(30, () => .5),
        };
        Assert.AreEqual(20, Fuzzy.DefuzzifyByCentroid(rules));
    }

    [Test]
    public void DefineInputsByPeaksTest()
    {
        var results = Fuzzy.DefineInputsByPeaks(
            -4,
            new List<Fuzzy.PeakDefinition>()
            {
                new(new(), -3, -2),
                new(new(), 0, 1),
            },
            7);
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
        TipCalculator tip = new(7.5, 15, 25);
        Assert.AreEqual(25, tip.Calculate(5, 3), ALLOWEDERROR);
        Assert.AreEqual(20, tip.Calculate(4, 3), ALLOWEDERROR);
        Assert.AreEqual(17.5, tip.Calculate(3.5, 3), ALLOWEDERROR);
        Assert.AreEqual(15, tip.Calculate(3, 3), ALLOWEDERROR);
        Assert.AreEqual(14.1666666, tip.Calculate(3.5, 2), ALLOWEDERROR);
        Assert.AreEqual(12.5, tip.Calculate(3, 2), ALLOWEDERROR);
        Assert.AreEqual(11.25, tip.Calculate(3, 1), ALLOWEDERROR);
        Assert.AreEqual(10, tip.Calculate(2, 1), ALLOWEDERROR);
        Assert.AreEqual(7.5, tip.Calculate(1, 1), ALLOWEDERROR);
    }

    [Test]
    public void FuzzifyTest()
    {
        Fuzzy.Input ServiceWasExcellent = new(3, 5, 5, double.MaxValue);
        Fuzzy.Input ServiceWasOk = new(1, 3, 3, 5);
        Fuzzy.Input ServiceWasPoor = new(double.MinValue, 1, 1, 3);
        Fuzzy.InputGroup Service = new(
            new()
            {
                ServiceWasPoor,
                ServiceWasOk,
                ServiceWasExcellent
            });
        Service.Fuzzify(3);
        Assert.AreEqual(0, ServiceWasExcellent.FX);
        Assert.AreEqual(1, ServiceWasOk.FX);
        Assert.AreEqual(0, ServiceWasPoor.FX);
    }

    // Compute the tip based on 1-5 star service and food ratings
    //
    // IF the service was excellent THEN the tip should be generous
    // IF the service was ok THEN the tip should be average
    // IF the service was poor OR the food was terrible THEN the tip should be low
    public class TipCalculator
    {
        // Construct a TipCalculator
        public TipCalculator(
            double lowTip,
            double averageTip,
            double generousTip)
        {
            // Define how service/food ratings are fuzzified
            serviceWasExcellent = new(3, 5, 5, double.MaxValue);
            serviceWasOk = new(1, 3, 3, 5);
            serviceWasPoor = new(double.MinValue, 1, 1, 3);
            foodWasTerrible = new(double.MinValue, 1, 1, 3);
            service = new(
                new()
                {
                    serviceWasPoor,
                    serviceWasOk,
                    serviceWasExcellent
                });
            // Define physical values for defuzzification
            this.lowTip = lowTip;
            this.averageTip = averageTip;
            this.generousTip = generousTip;
        }

        public double Calculate(double serviceStars, double foodStars)
        {
            // Fuzzify inputs
            service.Fuzzify(serviceStars);
            foodWasTerrible.Fuzzify(foodStars);
            // Evaluate the fuzzy IF/THEN rules
            List<Fuzzy.Rule> rules = new()
            {
                new(generousTip, () => serviceWasExcellent.FX),
                new(averageTip, () => serviceWasOk.FX),
                new(lowTip, () => Fuzzy.OR(serviceWasPoor.FX, foodWasTerrible.FX)),
            };
            // defuzzify to a physical tip value
            return Fuzzy.DefuzzifyByCentroid(rules);
        }

        readonly Fuzzy.Input serviceWasExcellent;
        readonly Fuzzy.Input serviceWasOk;
        readonly Fuzzy.Input serviceWasPoor;
        readonly Fuzzy.Input foodWasTerrible;
        readonly Fuzzy.InputGroup service;
        readonly double lowTip;
        readonly double averageTip;
        readonly double generousTip;
    }
}