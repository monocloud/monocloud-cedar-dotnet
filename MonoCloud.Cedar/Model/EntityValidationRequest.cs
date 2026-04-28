namespace MonoCloud.Cedar.Model;

public sealed class EntityValidationRequest(Schema.Schema schema, IReadOnlyList<Entity.Entity> entities)
{
  [JsonPropertyName("schema")]
  public Schema.Schema Schema { get; } = schema;

  [JsonPropertyName("entities")]
  public IReadOnlyList<Entity.Entity> Entities { get; } = entities;
}
