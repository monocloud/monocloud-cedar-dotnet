namespace MonoCloud.Cedar.Value;

public sealed class PrimString(string value) : Value
{
  private readonly string value = value;

  public string GetValue() => value;

  public override string ToString() => value;

  public override string ToCedarExpr() => $"\"{value}\"";

  public override bool Equals(object? obj) => obj is PrimString other && value == other.value;

  public override int GetHashCode() => value.GetHashCode();
}
