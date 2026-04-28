namespace MonoCloud.Cedar.Value;

public sealed class EntityUID : Value
{
  private readonly Lazy<string> repr;

  public EntityUID(EntityTypeName type, EntityIdentifier id)
  {
    Type = type;
    Id = id;
    repr = new Lazy<string>(() => CedarJson.NativeAnswer(() => CedarFfi.CedarEuidRepr(type.ToSource(), id.ToString())));
  }

  public EntityUID(EntityTypeName type, string id) : this(type, new EntityIdentifier(id))
  {
  }

  [JsonIgnore]
  public EntityTypeName Type { get; }

  [JsonIgnore]
  public EntityIdentifier Id { get; }

  public EntityTypeName GetTypeName() => Type;

  public EntityIdentifier GetId() => Id;

  public JsonEUID AsJson() => new(Type.ToString(), Id.ToString());

  public static EntityUID? Parse(string src)
  {
    ArgumentNullException.ThrowIfNull(src);
    var parsed = CedarJson.NativeAnswer(() => CedarFfi.CedarParseEntityUid(src));
    if (parsed == "null")
    {
      return null;
    }

    var separator = parsed.IndexOf("::\"", StringComparison.Ordinal);
    if (separator < 0 || !parsed.EndsWith('"'))
    {
      return null;
    }

    var type = EntityTypeName.FromValidatedSource(parsed[..separator]);
    var id = parsed[(separator + 3)..^1];
    return new EntityUID(type, id);
  }

  public static EntityUID? ParseFromJson(JsonEUID euid) =>
    EntityTypeName.Parse(euid.Type) is { } type ? new EntityUID(type, euid.Id) : null;

  public override string ToString() => repr.Value;

  public override string ToCedarExpr() => ToString();

  public override bool Equals(object? obj) => obj is EntityUID other && Type.Equals(other.Type) && Id.Equals(other.Id);

  public override int GetHashCode() => HashCode.Combine(Type, Id);
}

public sealed record JsonEUID(
  [property: JsonPropertyName("type")] string Type,
  [property: JsonPropertyName("id")] string Id);
