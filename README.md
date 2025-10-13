# Fuzzy
A simple fuzzy logic library

- Define fuzzy inputs
- Fuzzify physical values into fuzzy values
- Combine fuzzy values using IF/THEN rules
- Defuzzify the rules back to physical values

## Example - fuzzy tip calculator
```csharp
using Imagibee;

// Compute the tip based on 1-5 star service and food ratings
//
// IF the service was excellent THEN the tip should be generous
// IF the service was ok THEN the tip should be average
// IF the service was poor OR the food was terrible THEN the tip should be low
public double ComputeTip(double serviceStars, double foodStars)
{
    // Define service rating based on 1-5 star review
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
    Service.Fuzzify(serviceStars);

    // Define food rating based on 1-5 star review
    Fuzzy.Input FoodWasTerrible = new(double.MinValue, 1, 1, 3);
    FoodWasTerrible.Fuzzify(foodStars);

    // Evaluate the fuzzy IF/THEN rules
    List<Fuzzy.Rule> rules = new()
    {
        // Generous tip is 25%
        new(25, () => ServiceWasExcellent.FX),
        // Average tip is 15%
        new(15, () => ServiceWasOk.FX),
        // Low tip is 7.5%
        new(7.5, () => Fuzzy.OR(ServiceWasPoor.FX, FoodWasTerrible.FX)),
    };

    // return the tip
    return Fuzzy.DefuzzifyByCentroid(rules);
}

// Test Results
Assert.AreEqual(25, ComputeTip(5, 3), ALLOWEDERROR);
Assert.AreEqual(20, ComputeTip(4, 3), ALLOWEDERROR);
Assert.AreEqual(17.5, ComputeTip(3.5, 3), ALLOWEDERROR);
Assert.AreEqual(15, ComputeTip(3, 3), ALLOWEDERROR);
Assert.AreEqual(14.1666666, ComputeTip(3.5, 2), ALLOWEDERROR);
Assert.AreEqual(12.5, ComputeTip(3, 2), ALLOWEDERROR);
Assert.AreEqual(11.25, ComputeTip(3, 1), ALLOWEDERROR);
Assert.AreEqual(10, ComputeTip(2, 1), ALLOWEDERROR);
Assert.AreEqual(7.5, ComputeTip(1, 1), ALLOWEDERROR);

```
## Testing
Run `Scripts/test`.

## License
[MIT](https://raw.githubusercontent.com/imagibee/Fuzzy/refs/heads/main/LICENSE)

## Issues
Report and track issues [here](https://github.com/imagibee/Fuzzy/issues).

## Contributing
To make minor changes (such as bug fixes) simply make a pull request.  Please open an issue to discuss other changes.
