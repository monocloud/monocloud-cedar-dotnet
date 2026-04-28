namespace MonoCloud.Cedar.Value;

[Experimental(ExperimentalFeature.PartialEvaluation)]
public sealed class Unknown(string arg) : Value
{
  private readonly string arg = arg;

  public override string ToCedarExpr() => $"Unknown(\"{arg}\")";

  public override string ToString() => arg;

  public override bool Equals(object? obj) => obj is Unknown other && arg == other.arg;

  public override int GetHashCode() => arg.GetHashCode();
}
