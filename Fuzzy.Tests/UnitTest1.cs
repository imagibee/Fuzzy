namespace Fuzzy.Tests;

#pragma warning disable NUnit2005

public class Tests
{
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
    public void TestDefuzzifyByCentroid()
    {
        List<Rule> rules = new()
        {
            new(10, () => .5),
            new(30, () => .5),
        };
        Assert.AreEqual(20, Functions.DefuzzifyByCentroid(rules));
    }

}