# Fuzzy
A practical, lightweight fuzzy logic library inspired by Mamdani

The primary goals of this project are to to provide an open, reliable, convenient, performant, tested basis of C# classes and functions for the development of fuzzy logic controllers within C# projects.  I have intentionally avoided using text to name variables, or writing parsers to parse rules, etc.  The C# language already provides the features we need if we are willing to live within it's constraints.  So, for this project at least, there is no need to resort to these more complex measures.

## API
FWIW, here is an overview of the API.  I recommend reading the source code for details.

- <span style="color: orange;">static class</span> <span style="color: violet;">Imagibee.Fuzzy</span> - contains the whole library
- <span style="color: orange;">class</span> <span style="color: violet;">Imagibee.Fuzzy.Input</span> - for defining trapezoidal, triangular, or box membership functions and fuzzifying physical values
- <span style="color: orange;">class</span> <span style="color: violet;">Imagibee.Fuzzy.InputGroup</span> - a <i>convenient</i> way to fuzzify a group of inputs that derive their fuzzy values from the same physical value
- <span style="color: orange;">class</span> <span style="color: violet;">Imagibee.Fuzzy.Rule</span> - for combining fuzzy inputs into IF/THEN rules (by utilizing lambda expressions)
- <span style="color: orange;">function</span> <span style="color: violet;">Imagibee.Fuzzy.DefuzzifyByCentroid</span> - defuzzify rules to a physical value
- <span style="color: orange;">function</span> <span style="color: violet;">Imagibee.Fuzzy.DefineInputsByPeaks</span> - An <i>even more convenient</i> way to define inputs that should work for most cases
- <span style="color: orange;">class</span> <span style="color: violet;">Imagibee.Fuzzy.PeakDefinition</span> - used by DefineInputsByPeaks


## Example - fuzzy tip calculator
Here is an example that demonstrates how you might want to use this library.  It shows how you could implement a fuzzy-logic tip calculator.  The kind of thing you would use to calculate a tip when you eat at a restaraunt.  For the sake of this exampe, the physical value of the tip ranges between 7.5% to 25%.  The tip is the ultimate value we want to get from our calculator so we can pay our waiter fairly.  The service rating ranges from 1-5 stars, and it is a physical input value based on how good you thought the service was.  The food rating also ranges from 1-5 stars, and it is another physical input value based on how good you thought the food was.

Here are the rules ...
- <span style="color: orange;">IF</span> the service was excellent <span style="color: orange;">THEN</span> the tip should be generous
- <span style="color: orange;">IF</span> the service was ok <span style="color: orange;">THEN</span> the tip should be average
- <span style="color: orange;">IF</span> the service was poor <span style="color: orange;">OR</span> the food was terrible <span style="color: orange;">THEN</span> the tip should be low

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
