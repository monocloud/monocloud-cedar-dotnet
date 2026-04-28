namespace MonoCloud.Cedar.Model;

[Experimental(ExperimentalFeature.PartialEvaluation)]
public sealed class PartialAuthorizationSuccessResponse
{
  [JsonPropertyName("decision")]
  public AuthorizationSuccessResponse.Decision? DecisionValue { get; set; }

  [JsonPropertyName("satisfied")]
  public ISet<string> Satisfied { get; set; } = new HashSet<string>();

  [JsonPropertyName("errored")]
  public ISet<string> Errored { get; set; } = new HashSet<string>();

  [JsonPropertyName("mayBeDetermining")]
  public ISet<string> MayBeDetermining { get; set; } = new HashSet<string>();

  [JsonPropertyName("mustBeDetermining")]
  public ISet<string> MustBeDetermining { get; set; } = new HashSet<string>();

  [JsonPropertyName("residuals")]
  public IReadOnlyDictionary<string, JsonElement> Residuals { get; set; } = new Dictionary<string, JsonElement>();

  [JsonPropertyName("nontrivialResiduals")]
  public ISet<string> NontrivialResiduals { get; set; } = new HashSet<string>();

  [JsonPropertyName("warnings")]
  public ISet<string> Warnings { get; set; } = new HashSet<string>();

  public AuthorizationSuccessResponse.Decision? GetDecision() => DecisionValue;

  public IReadOnlyDictionary<string, JsonElement> GetResiduals() => Residuals;

  public ISet<string> GetSatisfied() => new HashSet<string>(Satisfied);

  public ISet<string> GetErrored() => new HashSet<string>(Errored);

  public ISet<string> GetMayBeDetermining() => new HashSet<string>(MayBeDetermining);

  public ISet<string> GetMustBeDetermining() => new HashSet<string>(MustBeDetermining);

  public ISet<string> GetNontrivialResiduals() => new HashSet<string>(NontrivialResiduals);
}
