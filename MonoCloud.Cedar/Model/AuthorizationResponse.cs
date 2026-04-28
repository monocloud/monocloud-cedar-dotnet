namespace MonoCloud.Cedar.Model;

public sealed class AuthorizationResponse
{
  [JsonConstructor]
  public AuthorizationResponse(SuccessOrFailure type, AuthorizationSuccessResponse? success, IReadOnlyList<DetailedError>? errors, IReadOnlyList<string>? warnings)
  {
    Type = type;
    Success = success;
    Errors = errors;
    Warnings = warnings ?? [];
  }

  [JsonPropertyName("type")]
  public SuccessOrFailure Type { get; }

  [JsonPropertyName("response")]
  public AuthorizationSuccessResponse? Success { get; }

  [JsonPropertyName("errors")]
  public IReadOnlyList<DetailedError>? Errors { get; }

  [JsonPropertyName("warnings")]
  public IReadOnlyList<string> Warnings { get; }

  public override string ToString() =>
    Type == SuccessOrFailure.Success ? $"SUCCESS: {Success}" : $"FAILURE: {string.Join(", ", Errors ?? [])}";

  public enum SuccessOrFailure
  {
    [EnumMember(Value = "success")]
    Success,
    [EnumMember(Value = "failure")]
    Failure
  }
}
