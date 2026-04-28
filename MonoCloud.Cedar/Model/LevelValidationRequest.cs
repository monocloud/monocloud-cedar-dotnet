namespace MonoCloud.Cedar.Model;

public sealed class LevelValidationRequest
{
  public LevelValidationRequest(Schema.Schema schema, PolicySet policies, long maxDerefLevel)
  {
    if (maxDerefLevel < 0)
    {
      throw new ArgumentException("maxDerefLevel must be non-negative", nameof(maxDerefLevel));
    }

    Schema = schema;
    Policies = policies;
    MaxDerefLevel = maxDerefLevel;
  }

  [JsonPropertyName("schema")]
  public Schema.Schema Schema { get; }

  [JsonPropertyName("policies")]
  public PolicySet Policies { get; }

  [JsonPropertyName("maxDerefLevel")]
  public long MaxDerefLevel { get; }

  public Schema.Schema GetSchema() => Schema;

  public PolicySet GetPolicySet() => Policies;

  public long GetMaxDerefLevel() => MaxDerefLevel;
}
