namespace MonoCloud.Cedar.Model.Exception;

public sealed class MissingExperimentalFeatureException(ExperimentalFeature feature)
  : InternalException($"The Cedar native runtime was not built with {feature}.")
{
  public ExperimentalFeature Feature { get; } = feature;
}
