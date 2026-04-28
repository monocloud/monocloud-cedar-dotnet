namespace MonoCloud.Cedar.Value;

public sealed class Decimal : Value
{
  private static readonly Regex Pattern = new("^-?[0-9]+\\.[0-9]{1,4}$", RegexOptions.Compiled);
  private const decimal MinValue = -922337203685477.5808m;
  private const decimal MaxValue = 922337203685477.5807m;
  private readonly string value;

  public Decimal(string decimalValue)
  {
    if (decimalValue is null ||
        !Pattern.IsMatch(decimalValue) ||
        !System.Decimal.TryParse(decimalValue, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed) ||
        parsed < MinValue ||
        parsed > MaxValue)
    {
      throw new ArgumentException($"Input string is not a valid decimal. E.g., \"1.0000\") \n {decimalValue}", nameof(decimalValue));
    }

    value = decimalValue;
  }

  public override string ToCedarExpr() => $"decimal(\"{value}\")";

  public override string ToString() => value;

  public override bool Equals(object? obj) => obj is Decimal other && value == other.value;

  public override int GetHashCode() => value.GetHashCode();
}
