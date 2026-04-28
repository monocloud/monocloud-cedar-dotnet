namespace MonoCloud.Cedar;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface)]
public sealed class ExperimentalAttribute(ExperimentalFeature feature) : Attribute
{
  public ExperimentalFeature Feature { get; } = feature;
}
