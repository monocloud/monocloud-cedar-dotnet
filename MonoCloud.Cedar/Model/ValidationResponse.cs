namespace MonoCloud.Cedar.Model;

public sealed class ValidationResponse
{
  [JsonPropertyName("type")]
  public SuccessOrFailure Type { get; set; }

  public ValidationSuccessResponse? Success =>
    Type == SuccessOrFailure.Success ? new ValidationSuccessResponse(ValidationErrors, ValidationWarnings) : null;

  [JsonPropertyName("validationErrors")]
  public IReadOnlyList<ValidationError>? ValidationErrors { get; set; }

  [JsonPropertyName("validationWarnings")]
  public IReadOnlyList<ValidationError>? ValidationWarnings { get; set; }

  [JsonPropertyName("errors")]
  public IReadOnlyList<DetailedError>? Errors { get; set; }

  [JsonPropertyName("warnings")]
  public IReadOnlyList<DetailedError> Warnings { get; set; } = [];

  public bool ValidationPassed() => Success is not null && Success.ValidationErrors.Count == 0;

  public enum SuccessOrFailure
  {
    [EnumMember(Value = "success")]
    Success,
    [EnumMember(Value = "failure")]
    Failure
  }

  public sealed class ValidationSuccessResponse(IReadOnlyList<ValidationError>? validationErrors, IReadOnlyList<ValidationError>? validationWarnings)
  {
    [JsonPropertyName("validationErrors")]
    public IReadOnlyList<ValidationError> ValidationErrors { get; } = validationErrors ?? [];

    [JsonPropertyName("validationWarnings")]
    public IReadOnlyList<ValidationError> ValidationWarnings { get; } = validationWarnings ?? [];
  }

  public sealed class ValidationError
  {
    [JsonConstructor]
    public ValidationError(string policyId, DetailedError error)
    {
      PolicyId = policyId;
      Error = error;
    }

    [JsonPropertyName("policyId")]
    public string PolicyId { get; }

    [JsonPropertyName("error")]
    public DetailedError Error { get; }

    public string GetPolicyId() => PolicyId;

    public DetailedError GetError() => Error;
  }
}
