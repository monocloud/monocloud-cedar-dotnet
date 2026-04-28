namespace MonoCloud.Cedar.Value.Functions;

public sealed class Offset : Value
{
  public Offset(CedarDateTime dateTime, Duration offsetDuration)
  {
    DateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
    OffsetDuration = offsetDuration ?? throw new ArgumentNullException(nameof(offsetDuration));
  }

  public CedarDateTime DateTime { get; }

  public Duration OffsetDuration { get; }

  public override string ToCedarExpr() => $"{DateTime.ToCedarExpr()}.offset({OffsetDuration.ToCedarExpr()})";

  public override string ToString() => ToCedarExpr();

  public override bool Equals(object? obj) =>
    obj is Offset other && DateTime.Equals(other.DateTime) && OffsetDuration.Equals(other.OffsetDuration);

  public override int GetHashCode() =>
#if NETSTANDARD2_0
    DateTime.GetHashCode() * 31 + OffsetDuration.GetHashCode();
#else
    HashCode.Combine(DateTime, OffsetDuration);
#endif
}
