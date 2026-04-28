namespace MonoCloud.Cedar.Model;

public sealed class ValidationRequest(Schema.Schema schema, PolicySet policies)
{
  public Schema.Schema GetSchema() => schema;

  [JsonPropertyName("schema")]
  public Schema.Schema Schema => schema;

  [JsonPropertyName("policies")]
  public PolicySet Policies => policies;

  public PolicySet GetPolicySet() => policies;
}
