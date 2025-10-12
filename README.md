# Fuzzy
A simple and easy fuzzy logic library

- Define fuzzy inputs
- Convert physical inputs into fuzzy inputs
- Combine fuzzy inputs into IF/THEN rules
- Defuzzify the rules back to physical results

## Examples
```csharp
// Compute tip based on 1 to 5 stars of service
//
// IF the service was excellent THEN the tip should be generous
// IF the service was ok THEN the tip should be average
// IF the service was poor THEN the tip should be low
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
```
## Testing
Run `Scripts/test`.

## License
[MIT](https://raw.githubusercontent.com/imagibee/Fuzzy/refs/heads/main/LICENSE)

## Issues
Report and track issues [here](https://github.com/imagibee/Fuzzy/issues).

## Contributing
To make minor changes (such as bug fixes) simply make a pull request.  Please open an issue to discuss other changes.
