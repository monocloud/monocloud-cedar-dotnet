namespace MonoCloud.Cedar.Model;

[Experimental(ExperimentalFeature.PartialEvaluation)]
public class PartialAuthorizationRequest
{
  public PartialAuthorizationRequest(
    EntityUID? principal,
    EntityUID? action,
    EntityUID? resource,
    IDictionary<string, Value.Value>? context,
    Schema.Schema? schema,
    bool enableRequestValidation)
  {
    Principal = principal;
    Action = action;
    Resource = resource;
    Context = context is null ? null : new Dictionary<string, Value.Value>(context);
    Schema = schema;
    EnableRequestValidation = enableRequestValidation;
  }

  [JsonPropertyName("principal")]
  public EntityUID? Principal { get; }

  [JsonPropertyName("action")]
  public EntityUID? Action { get; }

  [JsonPropertyName("resource")]
  public EntityUID? Resource { get; }

  [JsonPropertyName("context")]
  public IReadOnlyDictionary<string, Value.Value>? Context { get; }

  [JsonPropertyName("schema")]
  public Schema.Schema? Schema { get; }

  [JsonPropertyName("validateRequest")]
  public bool EnableRequestValidation { get; }

  public static Builder NewBuilder() => new();

  public sealed class Builder
  {
    private EntityUID? principal;
    private EntityUID? action;
    private EntityUID? resource;
    private IDictionary<string, Value.Value>? context;
    private Schema.Schema? schema;
    private bool enableRequestValidation;

    public Builder Principal(EntityUID principalEUID)
    {
      principal = principalEUID;
      return this;
    }

    public Builder Action(EntityUID actionEUID)
    {
      action = actionEUID;
      return this;
    }

    public Builder Resource(EntityUID resourceEUID)
    {
      resource = resourceEUID;
      return this;
    }

    public Builder Context(IDictionary<string, Value.Value> context)
    {
      this.context = new Dictionary<string, Value.Value>(context);
      return this;
    }

    public Builder Context(Model.Context context)
    {
      this.context = context.GetContext();
      return this;
    }

    public Builder EmptyContext()
    {
      context = new Dictionary<string, Value.Value>();
      return this;
    }

    public Builder Schema(Schema.Schema schema)
    {
      this.schema = schema;
      return this;
    }

    public Builder EnableRequestValidation()
    {
      enableRequestValidation = true;
      return this;
    }

    public PartialAuthorizationRequest Build() => new(principal, action, resource, context, schema, enableRequestValidation);
  }
}
