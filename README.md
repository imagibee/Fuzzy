# Fuzzy
A practical, lightweight fuzzy logic library inspired by Mamdani

The primary goals of this project are to to provide an open, reliable, flexible, convenient, performant, tested basis of C# classes and functions that support the development of fuzzy logic controllers within C# projects.  I have intentionally avoided a linguistically pure approach that uses interpreted strings to name variables, define rules, etc.  Instead I have chosen to leverage C#'s built-in features for symbol naming.  In essence, this choice reduces complexity and improves performance at the cost of linguistic purity.

## API
Here is an overview of the API.  FWIW, I recommend reading the source code for details.

- `static class Imagibee.Fuzzy` - contains the whole library
- `class Imagibee.Fuzzy.Input` - for defining trapezoidal, triangular, or box membership functions and fuzzifying physical values
- `class Imagibee.Fuzzy.InputGroup` - a <i>convenient</i> way to fuzzify a group of inputs that derive their fuzzy values from the same physical value
- `class Imagibee.Fuzzy.Rule` - for combining fuzzy inputs into IF/THEN rules
- `function Imagibee.Fuzzy.DefuzzifyByCentroid` - defuzzify rules to a physical value
- `function Imagibee.Fuzzy.DefineInputsByPeaks` - An <i>even more convenient</i> way to define inputs that should work for most cases
- `class Imagibee.Fuzzy.PeakDefinition` - used by DefineInputsByPeaks


## Example - fuzzy tip calculator
Here is an example that demonstrates how you might want to use this library.  It shows how you could implement a fuzzy-logic tip calculator.  The kind of thing you would use to calculate a tip when you eat at a restaraunt.  For the sake of this exampe, the physical value of the tip ranges between 7.5% to 25%.  The tip is the ultimate value we want to get from our calculator so we can pay our waiter fairly.  The service rating ranges from 1-5 stars, and it is a physical input value based on how good you thought the service was.  The food rating also ranges from 1-5 stars, and it is another physical input value based on how good you thought the food was.

Here are the rules ...
- `IF` the service was excellent `THEN` the tip should be generous
- `IF` the service was ok `THEN` the tip should be average
- `IF` the service was poor `OR` the food was terrible `THEN` the tip should be low

And here is how I would code the rules ...
```csharp
using Imagibee;

public class MyTipCalculator
{
    // Construct a MyTipCalculator
    public MyTipCalculator(
        double lowTip,
        double averageTip,
        double generousTip)
    {
        // Define membership functions
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
        // Define the fuzzy IF/THEN rules
        rules = new()
        {
            new(generousTip, () => serviceWasExcellent.FX),
            new(averageTip, () => serviceWasOk.FX),
            new(lowTip, () => Fuzzy.OR(serviceWasPoor.FX, foodWasTerrible.FX)),
        };
    }

    public double Calculate(double serviceStars, double foodStars)
    {
        // Fuzzify inputs
        service.Fuzzify(serviceStars);
        foodWasTerrible.Fuzzify(foodStars);
        // defuzzify to a physical tip value
        return Fuzzy.DefuzzifyByCentroid(rules);
    }

    // private data
    readonly Fuzzy.Input serviceWasExcellent;
    readonly Fuzzy.Input serviceWasOk;
    readonly Fuzzy.Input serviceWasPoor;
    readonly Fuzzy.Input foodWasTerrible;
    readonly Fuzzy.InputGroup service;
    readonly List<Fuzzy.Rule> rules;
}
```

And here are the tests that were used to validate this example ...
```csharp
MyTipCalculator tip = new(7.5, 15, 25);
Assert.AreEqual(25, tip.Calculate(5, 3), ALLOWEDERROR);
Assert.AreEqual(20, tip.Calculate(4, 3), ALLOWEDERROR);
Assert.AreEqual(17.5, tip.Calculate(3.5, 3), ALLOWEDERROR);
Assert.AreEqual(15, tip.Calculate(3, 3), ALLOWEDERROR);
Assert.AreEqual(14.1666666, tip.Calculate(3.5, 2), ALLOWEDERROR);
Assert.AreEqual(12.5, tip.Calculate(3, 2), ALLOWEDERROR);
Assert.AreEqual(11.25, tip.Calculate(3, 1), ALLOWEDERROR);
Assert.AreEqual(10, tip.Calculate(2, 1), ALLOWEDERROR);
Assert.AreEqual(7.5, tip.Calculate(1, 1), ALLOWEDERROR);

```
## Testing
Run `Scripts/test`.

## License
[MIT](https://raw.githubusercontent.com/imagibee/Fuzzy/refs/heads/main/LICENSE)

## Issues
Report and track issues [here](https://github.com/imagibee/Fuzzy/issues).

## Contributing
To make minor changes (such as bug fixes) simply make a pull request.  Please open an issue to discuss other changes.
