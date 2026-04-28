namespace MonoCloud.Cedar.Model;

[Experimental(ExperimentalFeature.PartialEvaluation)]
public sealed class PartialAuthorizationResponse
{
  [JsonConstructor]
  public PartialAuthorizationResponse(SuccessOrFailure type, PartialAuthorizationSuccessResponse? success, IReadOnlyList<DetailedError>? errors, IReadOnlyList<string>? warnings)
  {
    Type = type;
    Success = success;
    Errors = errors;
    Warnings = warnings ?? [];
  }

  [JsonPropertyName("type")]
  public SuccessOrFailure Type { get; }

  [JsonPropertyName("response")]
  public PartialAuthorizationSuccessResponse? Success { get; }

  [JsonPropertyName("errors")]
  public IReadOnlyList<DetailedError>? Errors { get; }

  [JsonPropertyName("warnings")]
  public IReadOnlyList<string> Warnings { get; }

  public enum SuccessOrFailure
  {
    [EnumMember(Value = "residuals")]
    Success,
    [EnumMember(Value = "failure")]
    Failure
  }
}
