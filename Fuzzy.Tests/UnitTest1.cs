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
        Input input = new(-3, -2, -1, 0);
        Assert.AreEqual(-3, input.X1);
        Assert.AreEqual(-2, input.X2);
        Assert.AreEqual(-1, input.X3);
        Assert.AreEqual(0, input.X4);
    }

    [Test]
    public void TestInvalidInputConstructor()
    {
        Assert.Throws<ArgumentException>(
            () => new Input(-3, -1, -2, 0));
    }

    [Test]
    public void TestDefaultInputConstructor()
    {
        Input input = new();
        Assert.AreEqual(double.MinValue, input.X1);
        Assert.AreEqual(0, input.X2);
        Assert.AreEqual(0, input.X3);
        Assert.AreEqual(double.MaxValue, input.X4);
    }

    [Test]
    public void TestInputFuzzification()
    {
        Input input = new(-2, 0, 0, 2);
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
        Assert.AreEqual(.2, Functions.AND(.2, .8), ALLOWEDERROR);
        Assert.AreEqual(.2, Functions.AND(new double[] { .2, .8 }), ALLOWEDERROR);
    }

    [Test]
    public void TestOR()
    {
        Assert.AreEqual(.8, Functions.OR(.2, .8), ALLOWEDERROR);
        Assert.AreEqual(.8, Functions.OR(new double[] { .2, .8 }), ALLOWEDERROR);
    }

    [Test]
    public void TestNOT()
    {
        Assert.AreEqual(.2, Functions.NOT(.8), ALLOWEDERROR);
    }

    [Test]
    public void TestDefuzzifyByCentroid()
    {
        List<Rule> rules = new()
        {
            new(10, () => .5),
            new(30, () => .5),
        };
        Assert.AreEqual(20, Functions.DefuzzifyByCentroid(rules));
    }

    [Test]
    public void DefineInputsByPeaksTest()
    {
        var results = Functions.DefineInputsByPeaks(
            -4,
            new List<PeakDefinition>()
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
        // Tip based on 3 service stars and 1 food stars
        Assert.AreEqual(17.5, ComputeTip(3.5));
    }

    [Test]
    public void FuzzifyTest()
    {
        Input ServiceWasExcellent = new(3, 5, 5, double.MaxValue);
        Input ServiceWasOk = new(1, 3, 3, 5);
        Input ServiceWasPoor = new(double.MinValue, 1, 1, 3);
        Fuzzifier Service = new(new() { ServiceWasPoor, ServiceWasOk, ServiceWasExcellent });
        Service.Fuzzify(3);
        Assert.AreEqual(0, ServiceWasExcellent.FX);
        Assert.AreEqual(1, ServiceWasOk.FX);
        Assert.AreEqual(0, ServiceWasPoor.FX);
    }

    //IF the service was excellent THEN the tip should be generous
    //IF the service was ok THEN the tip should be average
    //IF the service was poor THEN the tip should be low
    public double ComputeTip(double serviceStars)
    {
        // Define service rating based 1 - 5 stars
        Input ServiceWasExcellent = new(3, 5, 5, double.MaxValue);
        Input ServiceWasOk = new(1, 3, 3, 5);
        Input ServiceWasPoor = new(double.MinValue, 1, 1, 3);
        Fuzzifier Service = new(new() { ServiceWasPoor, ServiceWasOk, ServiceWasExcellent });
        Service.Fuzzify(serviceStars);

        // Evaluate the fuzzy IF/THEN rules
        List<Rule> rules = new()
        {
            // Generous tip is 25%
            new(25, () => ServiceWasExcellent.FX),
            // Average tip is 15%
            new(15, () => ServiceWasOk.FX),
            // Low tip is 7.5%
            new(7.5, () => ServiceWasPoor.FX),
        };

        // return the tip
        return Functions.DefuzzifyByCentroid(rules);
    }
}