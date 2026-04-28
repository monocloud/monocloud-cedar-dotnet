namespace MonoCloud.Cedar.Model;

public sealed class AuthorizationSuccessResponse
{
  public AuthorizationSuccessResponse()
  {
    DecisionValue = Decision.Deny;
    DiagnosticsValue = new Diagnostics();
  }

  public AuthorizationSuccessResponse(Decision decision, Diagnostics diagnostics)
  {
    DecisionValue = decision;
    DiagnosticsValue = diagnostics;
  }

  [JsonPropertyName("decision")]
  public Decision DecisionValue { get; set; }

  [JsonPropertyName("diagnostics")]
  public Diagnostics DiagnosticsValue { get; set; }

  public Decision GetDecision() => DecisionValue;

  public ISet<string> GetReason() => new HashSet<string>(DiagnosticsValue.Reason);

  public IReadOnlyList<AuthorizationError> GetErrors() => DiagnosticsValue.Errors;

  public bool IsAllowed() => DecisionValue == Decision.Allow;

  public override string ToString() => $"{DecisionValue}, reason [{string.Join(", ", DiagnosticsValue.Reason)}], errors [{string.Join(", ", DiagnosticsValue.Errors)}]";

  public enum Decision
  {
    [EnumMember(Value = "allow")]
    Allow,
    [EnumMember(Value = "deny")]
    Deny
  }

  public sealed class Diagnostics
  {
    public Diagnostics()
    {
      Reason = new HashSet<string>();
      Errors = [];
    }

    [JsonConstructor]
    public Diagnostics(ISet<string>? reason, IReadOnlyList<AuthorizationError>? errors)
    {
      Reason = reason ?? new HashSet<string>();
      Errors = errors ?? [];
    }

    [JsonPropertyName("reason")]
    public ISet<string> Reason { get; }

    [JsonPropertyName("errors")]
    public IReadOnlyList<AuthorizationError> Errors { get; }

    public ISet<string> GetReasons() => new HashSet<string>(Reason);

    public IReadOnlyList<AuthorizationError> GetErrors() => Errors;
  }

  public sealed class AuthorizationError
  {
    [JsonConstructor]
    public AuthorizationError(string policyId, DetailedError error)
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

    public override string ToString() => $"AuthorizationError{{policyId={PolicyId}, error={Error}}}";
  }
}
