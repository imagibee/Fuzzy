# Imagibee.Fuzzy
## A lightweight C# library for efficient fuzzy logic controllers

The primary goal of this project is to to provide a simple and efficient library that supports creating fuzzy logic controllers within C# code.  It's features center around the implementation of these controllers not around design, visualization, or academics.  It is intended for creating efficient, easy-to-use controllers for applications such as video games.

If you already have (or can create) a set of fuzzy rules, and simply want to implement these rules in your C# code with minimal fuss, using a zero-order Sugeno fuzzy inference model, then this might be the library for you.  On the other hand, if you are looking for a tool to help you design or visualize a fuzzy controller then you will want to look elsewhere.

## API
Here are the main API's.  Refer to the [source code](https://github.com/imagibee/Fuzzy/blob/main/Fuzzy/Fuzzy.cs) for details.  There is also an example later in this document, or feel free to look at the [unit tests](https://github.com/imagibee/Fuzzy/blob/main/Fuzzy.Tests/UnitTest1.cs) if that is helpful.

- `Imagibee.Fuzzy.Input` - define trapezoidal, triangular, or box membership functions
- `Imagibee.Fuzzy.Rule` - combine fuzzy inputs into IF/THEN rules
- `Imagibee.Fuzzy.Defuzzify` - defuzzify rules to a physical value

## A note about `Rule` evaluation
You may have noticed that `Rule` relies on lambda expressions (as opposed to constants).  And if so you may be wondering why that is.  The idea is to have a simple way to define rules once but evaluate them over and over in the control loop.  The way the C# language defines closures for lambda functions provides a flexible and convenient way to do this since lambdas capture references, not their values at the time the lambda is created.  So the values captured in the rules are evaluated each time `Defuzzify` is called, not merely when they are constructed.

## Example 1 - pole on a cart
Here is an example that demonstrates how to use this library to implement the cannonical pole on a cart control problem.  You can refer to [this youtube video](https://youtu.be/fU8Lyc8kzto) for an in-depth explanation.  (This example was compiled, but the functionality was not tested.)

```
// Define membership functions for fuzzy inputs
//
// thetaIsNegative
//    (FX)
//     |
// 1.0 | -----------
//     |              \
//     |                 \
//     |                    \
//     |                       \
// 0.0 |                          ----------
// __________|______|______|______|______|_____ radians (X)
//     |   -1.0    -.5     0     .5     1.0
Fuzzy.Input thetaIsNegative = new(double.MinValue, double.MinValue, -.5, .5);

// thetaIsPositive
//    (FX)
//     |
// 1.0 |                          -----------
//     |                       /
//     |                    /
//     |                 /
//     |              /
// 0.0 | -----------
// __________|______|______|______|______|_____ radians (X)
//     |   -1.0    -.5     0     .5     1.0
Fuzzy.Input thetaIsPositive = new(-.5, .5, double.MaxValue, double.MaxValue);

// thetaDotIsNegative
//    (FX)
//     |
// 1.0 | -----------
//     |              \
//     |                 \
//     |                    \
//     |                       \
// 0.0 |                          ----------
// __________|______|______|______|______|_____ radians/s (X)
//     |    -10    -5      0      5      10
Fuzzy.Input thetaDotIsNegative = new(double.MinValue, double.MinValue, -5, 5);

// thetaDotIsPositive
//    (FX)
//     |
// 1.0 |                          -----------
//     |                       /
//     |                    /
//     |                 /
//     |              /
// 0.0 | -----------
// __________|______|______|______|______|_____ radians/s (X)
//     |    -10    -5      0      5      10
Fuzzy.Input thetaDotIsPositive = new(-5, 5, double.MaxValue, double.MaxValue);

// cartPositionIsNegative
//    (FX)
//     |
// 1.0 | -----------
//     |              \
//     |                 \
//     |                    \
//     |                       \
// 0.0 |                          ----------
// __________|______|______|______|______|_____ m (X)
//     |    -2     -1      0      1      2
Fuzzy.Input cartPositionIsNegative = new(double.MinValue, double.MinValue, -1, 1);

// cartPositionIsPositive
//    (FX)
//     |
// 1.0 |                          -----------
//     |                       /
//     |                    /
//     |                 /
//     |              /
// 0.0 | -----------
// __________|______|______|______|______|_____ m (X)
//     |    -2     -1      0      1      2
Fuzzy.Input cartPositionIsPositive = new(-1, 1, double.MaxValue, double.MaxValue);

// cartVelocityIsNegative
//    (FX)
//     |
// 1.0 | -----------
//     |              \
//     |                 \
//     |                    \
//     |                       \
// 0.0 |                          ----------
// __________|______|______|______|______|_____ m/s (X)
//     |    -10    -5      0      5      10
Fuzzy.Input cartVelocityIsNegative = new(double.MinValue, double.MinValue, -5, 5);

// cartVelocityIsPositive
//    (FX)
//     |
// 1.0 |                          -----------
//     |                       /
//     |                    /
//     |                 /
//     |              /
// 0.0 | -----------
// __________|______|______|______|______|_____ m/s (X)
//     |    -10    -5      0      5      10
Fuzzy.Input cartVelocityIsPositive = new(-5, 5, double.MaxValue, double.MaxValue);

// Define output values [Nm]
double forceIsNegativeSmall = -2;
double forceIsPositiveSmall = 2;
double forceIsNegativeMedium = -12;
double forceIsPositiveMedium = 12;
double forceIsNegativeLarge = -20;
double forceIsPositiveLarge = 20;

// Define rules
// IF (theta is negative) THEN  (force is negative medium)
// IF (theta is positive) THEN  (force is positive medium)
// IF (thetaDot is negative) THEN  (force is negative large)
// IF (thetaDot is positive) THEN  (force is positive large)
// IF (cartPosition is negative) THEN  (force is positive small)
// IF (cartPosition is positive) THEN  (force is negative small)
// IF (cartVelocity is negative) THEN  (force is negative medium)
// IF (cartVelocity is positive) THEN  (force is positive medium)
Fuzzy.Rule[] rules = new Fuzzy.Rule[]
{
    new(() => forceIsNegativeMedium, () => thetaIsNegative.FX),
    new(() => forceIsPositiveMedium, () => thetaIsPositive.FX),
    new(() => forceIsNegativeLarge, () => thetaDotIsNegative.FX),
    new(() => forceIsPositiveLarge, () => thetaDotIsPositive.FX),
    new(() => forceIsPositiveSmall, () => cartPositionIsNegative.FX),
    new(() => forceIsNegativeSmall, () => cartPositionIsPositive.FX),
    new(() => forceIsNegativeMedium, () => cartVelocityIsNegative.FX),
    new(() => forceIsPositiveMedium, () => cartVelocityIsPositive.FX),
};

// Physical input values
double theta = 0;
double thetaDot = 0;
double cartPosition = 0;
double cartVelocity = 0;

// Physical output value
double force = 0;

// The guts of the control loop are shown below.  The control loop would
// normally be called periodically in some kind of Update() function.
//
// 1) Refresh values for the inputs (theta, thetaDot, cartPosition, and cartVelocity)
// (Not shown)

// 2) Fuzzify the inputs
thetaIsNegative.Fuzzify(theta);
thetaIsPositive.Fuzzify(theta);
thetaDotIsNegative.Fuzzify(thetaDot);
thetaDotIsPositive.Fuzzify(thetaDot);
cartPositionIsNegative.Fuzzify(cartPosition);
cartPositionIsPositive.Fuzzify(cartPosition);
cartVelocityIsNegative.Fuzzify(cartVelocity);
cartVelocityIsPositive.Fuzzify(cartVelocity);

// 3) Update the force with the newly computed output value
force = Fuzzy.Defuzzify(rules);
```

## Example 2 - fuzzy tip calculator
Here is another example that implements the cannonical fuzzy-logic tip calculator for computing the waiter's tip at a restaraunt.  The tip is calculated based on a combination of the service and food rating (each between 1-5 stars).

Here are the rules ...
- `IF` (the service was excellent) `THEN` (the tip should be generous)
- `IF` (the service was ok) `THEN` (the tip should be average)
- `IF` (the service was poor `OR` the food was terrible) `THEN` (the tip should be low)

And here is a way to code these rules ...
```csharp
using Imagibee;

public class MyTipCalculator
{
    // Properties for the tip levels
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
        service = new Fuzzy.InputGroup(
            new Fuzzy.Input[]
            {
                serviceWasPoor,
                serviceWasOk,
                serviceWasExcellent
            });
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
```

And here are the tests that were used to validate this example ...
```csharp
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
```
## Testing
Run `Scripts/test`.

## License
[MIT](https://raw.githubusercontent.com/imagibee/Fuzzy/refs/heads/main/LICENSE)

## Issues
Report and track issues [here](https://github.com/imagibee/Fuzzy/issues).

## Contributing
To make minor changes (such as bug fixes) simply make a pull request.  Please open an issue to discuss other changes.
