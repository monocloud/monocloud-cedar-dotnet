namespace MonoCloud.Cedar.Model.Entity;

public sealed class Entity
{
  public Entity(EntityUID uid) : this(uid, new Dictionary<string, Value.Value>(), new HashSet<EntityUID>(), new Dictionary<string, Value.Value>())
  {
  }

  public Entity(EntityUID uid, ISet<EntityUID> parentsEUIDs) : this(uid, new Dictionary<string, Value.Value>(), parentsEUIDs, new Dictionary<string, Value.Value>())
  {
  }

  public Entity(EntityUID uid, IDictionary<string, Value.Value> attributes, ISet<EntityUID> parentsEUIDs) : this(uid, attributes, parentsEUIDs, new Dictionary<string, Value.Value>())
  {
  }

  [JsonConstructor]
  public Entity(EntityUID uid, IDictionary<string, Value.Value> attrs, ISet<EntityUID> parents, IDictionary<string, Value.Value>? tags = null)
  {
    EUID = uid;
    Attrs = new Dictionary<string, Value.Value>(attrs);
    ParentsEUIDs = new HashSet<EntityUID>(parents);
    Tags = new Dictionary<string, Value.Value>(tags ?? new Dictionary<string, Value.Value>());
  }

  [JsonPropertyName("uid")]
  public EntityUID EUID { get; }

  [JsonPropertyName("attrs")]
  public IReadOnlyDictionary<string, Value.Value> Attrs { get; }

  [JsonPropertyName("parents")]
  public IReadOnlySet<EntityUID> ParentsEUIDs { get; }

  [JsonPropertyName("tags")]
  public IReadOnlyDictionary<string, Value.Value> Tags { get; }

  public Value.Value? GetAttr(string attribute) => Attrs.GetValueOrDefault(attribute);

  public EntityUID GetEUID() => EUID;

  public ISet<EntityUID> GetParents() => new HashSet<EntityUID>(ParentsEUIDs);

  public IDictionary<string, Value.Value> GetTags() => new Dictionary<string, Value.Value>(Tags);

  public static Entity Parse(string jsonString) => CedarJson.Deserialize<Entity>(jsonString);

  public static Entity ParseFile(string filePath) => Parse(File.ReadAllText(filePath));

  public override string ToString() => EUID.ToString();
}
