# Fuzzy
A lightweight fuzzy logic library

- Define fuzzy inputs
- Fuzzify physical values into fuzzy values
- Combine fuzzy values using IF/THEN rules
- Defuzzify the rules back to a physical value

## Example - fuzzy tip calculator
```csharp
using Imagibee;

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

// Test Results
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

```
## Testing
Run `Scripts/test`.

## License
[MIT](https://raw.githubusercontent.com/imagibee/Fuzzy/refs/heads/main/LICENSE)

## Issues
Report and track issues [here](https://github.com/imagibee/Fuzzy/issues).

## Contributing
To make minor changes (such as bug fixes) simply make a pull request.  Please open an issue to discuss other changes.
