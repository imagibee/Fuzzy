# Imagibee.Fuzzy
## A lightweight C# library for implementing efficient fuzzy logic controllers

The primary goal of this library is to to provide simple and efficient code that supports creating fuzzy logic controllers.  It's features center around the implementation of these controllers not around design, visualization, or academics.  It prioritizes ease-of-integration, ease-of-use, and efficiency. The original use case was controlling NPC's for Unity games.

If you already have (or can create) a set of fuzzy rules, and simply want to implement these rules in your C# code with minimal fuss, then this might be the library for you.  On the other hand, if you are looking for a tool to help you design or visualize a fuzzy controller then you will want to look elsewhere (these aren't the droids you are looking for).

## API
Here is the main API.  Refer to the [source code](https://github.com/imagibee/Fuzzy/blob/main/Fuzzy/Fuzzy.cs) for details.  There are also examples later in this document, and feel free to look at the [unit tests](https://github.com/imagibee/Fuzzy/blob/main/Fuzzy.Tests/UnitTest1.cs).

- `Imagibee.Fuzzy.Input` - define trapezoidal, triangular, or box membership functions
- `Imagibee.Fuzzy.Rule` - define IF/THEN rules based on fuzzy inputs
- `Imagibee.Fuzzy.Fuzzify` - fuzzify inputs from a physical input value
- `Imagibee.Fuzzy.Defuzzify` - defuzzify rules to a physical output value

## Usage concepts
In many cases the usage can be divided into two stages.  A definition stage that occurs once when the system starts up, and a control loop that runs periodically.  During the definition stage the `Input` and `Rule` are instantiated and initialized into persistent storage.

Once definition is completed, the control loop begins.  It is the responsibility of the control loop to dynamically update the system.  In order to do this, it periodically refreshes the inputs, calls `Fuzzify` passing in a refreshed input value, and finally calls `Defuzzify` to update the output to a new physical value.  The control loop usually occurs during an update routine based on a timer interval that continues until the application terminates.

## A note about `Rule` evaluation
You may have noticed that `Rule` relies on lambda expressions (as opposed to constants).  And if so you may be wondering why that is.  The idea is to have a simple way to define rules once but evaluate them over and over in the control loop.  The way the C# language defines closures for lambda functions provides a flexible and convenient way to do this since lambdas capture references, not their values at the time the lambda is created.  The main takeaway here is that rules are evaluated each time `Defuzzify` is called, not merely when they are instantiated.

## Example 1 - fuzzy tip calculator
Here is an example that implements the cannonical fuzzy-logic tip calculator for computing the waiter's tip at a restaraunt.  The tip is calculated based on a combination of the service and food rating (each between 1-5 stars).

### Here are the rules ...
- IF (the service was excellent) THEN (the tip should be generous)
- IF (the service was ok) THEN (the tip should be average)
- IF ((the service was poor) OR (the food was terrible)) THEN (the tip should be low)

### And here is the code ...
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
        new(() => serviceWasExcellent, () => GenerousTip),
        new(() => serviceWasOk, () => AverageTip),
        new(() => Fuzzy.OR(serviceWasPoor, foodWasTerrible), () => LowTip)
    };
}

// Calculate a new tip value based on service rating and food rating
public double Calculate(double serviceStars, double foodStars)
{
    // Fuzzify 1-5 star service rating
    Fuzzy.Fuzzify(serviceStars, serviceWasPoor, serviceWasOk, serviceWasExcellent);

    // Fuzzify 1-5 star food rating
    foodWasTerrible.Fuzzify(foodStars);

    // Defuzzify rules and return the physical tip value
    return Fuzzy.Defuzzify(rules);
}
```

### And here are the tests ...
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

## Example 2 - pole on a cart
Here is an example that implements the cannonical pole on a cart control problem.  You can refer to [this youtube video](https://youtu.be/fU8Lyc8kzto) for an in-depth explanation.  (This example compiles, but the functionality was never thoroughly tested.)

### Here are the rules...
- IF (theta is negative) THEN (force is negative medium)
- IF (theta is positive) THEN (force is positive medium)
- IF (thetaDot is negative) THEN (force is negative large)
- IF (thetaDot is positive) THEN (force is positive large)
- IF (cartPosition is negative) THEN (force is positive small)
- IF (cartPosition is positive) THEN (force is negative small)
- IF (cartVelocity is negative) THEN (force is negative medium)
- IF (cartVelocity is positive) THEN (force is positive medium)

### And here is the code...
```csharp
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
Fuzzy.Rule[] rules = new Fuzzy.Rule[]
{
    new(() => thetaIsNegative, () => forceIsNegativeMedium),
    new(() => thetaIsPositive, () => forceIsPositiveMedium),
    new(() => thetaDotIsNegative, () => forceIsNegativeLarge),
    new(() => thetaDotIsPositive, () => forceIsPositiveLarge),
    new(() => cartPositionIsNegative, () => forceIsPositiveSmall),
    new(() => cartPositionIsPositive, () => forceIsNegativeSmall),
    new(() => cartVelocityIsNegative, () => forceIsNegativeMedium),
    new(() => cartVelocityIsPositive, () => forceIsPositiveMedium),
};

// The 3 stages of a control loop are illustrated below.  The control loop
// is called periodically in some kind of Update() function.
//
// 1) Refresh the inputs (details not shown)
double theta = 0;
double thetaDot = 0;
double cartPosition = 0;
double cartVelocity = 0;

// 2) Fuzzify the inputs
Fuzzy.Fuzzify(theta, thetaIsNegative, thetaIsPositive);
Fuzzy.Fuzzify(thetaDot, thetaDotIsNegative, thetaDotIsPositive);
Fuzzy.Fuzzify(cartPosition, cartPositionIsNegative, cartPositionIsPositive);
Fuzzy.Fuzzify(cartVelocity, cartVelocityIsNegative, cartVelocityIsPositive);

// 3) Update force with a new output value
double force = Fuzzy.Defuzzify(rules);
```

## Testing
Run `Scripts/test` and `Scripts/coverage`. 

## License
[MIT](https://raw.githubusercontent.com/imagibee/Fuzzy/refs/heads/main/LICENSE)

## Issues
Report and track issues [here](https://github.com/imagibee/Fuzzy/issues).

## Contributing
To make minor changes (such as bug fixes) simply make a pull request.  Please open an issue to discuss other changes.
