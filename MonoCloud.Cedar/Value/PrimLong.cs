namespace MonoCloud.Cedar.Value;

public sealed class PrimLong(long value) : Value
{
  private readonly long value = value;

  public long GetValue() => value;

  public override string ToString() => value.ToString();

  public override string ToCedarExpr() => ToString();

  public override bool Equals(object? obj) => obj is PrimLong other && value == other.value;

  public override int GetHashCode() => value.GetHashCode();
}
