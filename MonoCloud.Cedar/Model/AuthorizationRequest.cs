namespace MonoCloud.Cedar.Model;

public class AuthorizationRequest
{
  public AuthorizationRequest(
    EntityUID principalEUID,
    EntityUID actionEUID,
    EntityUID resourceEUID,
    IDictionary<string, Value.Value>? context,
    Schema.Schema? schema,
    bool enableRequestValidation)
  {
    PrincipalEUID = principalEUID;
    ActionEUID = actionEUID;
    ResourceEUID = resourceEUID;
    Context = context is null ? null : new Dictionary<string, Value.Value>(context);
    Schema = schema;
    EnableRequestValidation = enableRequestValidation;
  }

  public AuthorizationRequest(
    EntityUID principalEUID,
    EntityUID actionEUID,
    EntityUID resourceEUID,
    Context context,
    Schema.Schema? schema,
    bool enableRequestValidation)
    : this(principalEUID, actionEUID, resourceEUID, context.GetContext(), schema, enableRequestValidation)
  {
  }

  public AuthorizationRequest(EntityUID principalEUID, EntityUID actionEUID, EntityUID resourceEUID, IDictionary<string, Value.Value> context)
    : this(principalEUID, actionEUID, resourceEUID, context, null, false)
  {
  }

  public AuthorizationRequest(EntityUID principalEUID, EntityUID actionEUID, EntityUID resourceEUID, Context context)
    : this(principalEUID, actionEUID, resourceEUID, context, null, false)
  {
  }

  public AuthorizationRequest(Entity.Entity principal, Entity.Entity action, Entity.Entity resource, IDictionary<string, Value.Value> context)
    : this(principal.GetEUID(), action.GetEUID(), resource.GetEUID(), context)
  {
  }

  public AuthorizationRequest(Entity.Entity principal, Entity.Entity action, Entity.Entity resource, Context context)
    : this(principal.GetEUID(), action.GetEUID(), resource.GetEUID(), context)
  {
  }

  [JsonPropertyName("principal")]
  public EntityUID PrincipalEUID { get; }

  [JsonPropertyName("action")]
  public EntityUID ActionEUID { get; }

  [JsonPropertyName("resource")]
  public EntityUID ResourceEUID { get; }

  [JsonPropertyName("context")]
  public IReadOnlyDictionary<string, Value.Value>? Context { get; }

  [JsonPropertyName("schema")]
  public Schema.Schema? Schema { get; }

  [JsonPropertyName("validateRequest")]
  public bool EnableRequestValidation { get; }

  public override string ToString() => $"Request({PrincipalEUID}, {ActionEUID}, {ResourceEUID}, {Context})";
}
