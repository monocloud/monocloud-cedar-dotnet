namespace MonoCloud.Cedar.Value;

public sealed class PrimBool(bool value) : Value
{
  private readonly bool value = value;

  public bool GetValue() => value;

  public override string ToString() => value.ToString().ToLowerInvariant();

  public override string ToCedarExpr() => ToString();

  public override bool Equals(object? obj) => obj is PrimBool other && value == other.value;

  public override int GetHashCode() => value.GetHashCode();
}
