namespace MonoCloud.Cedar.Value;

public sealed class EntityTypeName
{
  private readonly IReadOnlyList<string> @namespace;
  private readonly Lazy<string> repr;

  private EntityTypeName(IReadOnlyList<string> @namespace, string basename)
  {
    this.@namespace = @namespace;
    BaseName = basename;
    repr = new Lazy<string>(() => CedarJson.NativeAnswer(() => CedarFfi.CedarEntityTypeNameRepr(ToSource())));
  }

  public string BaseName { get; }

  public EntityUID Of(string id) => new(this, id);

  public EntityUID Of(EntityIdentifier id) => new(this, id);

  public IEnumerable<string> GetNamespaceComponents() => @namespace;

  public string GetNamespaceAsString() => string.Join("::", @namespace);

  public string GetBaseName() => BaseName;

  public static EntityTypeName? Parse(string src)
  {
    ArgumentNullException.ThrowIfNull(src);
    var parsed = CedarJson.NativeAnswer(() => CedarFfi.CedarParseEntityTypeName(src));
    return parsed == "null" ? null : FromValidatedSource(parsed);
  }

  public override string ToString() => repr.Value;

  public override bool Equals(object? obj) =>
    obj is EntityTypeName other && BaseName == other.BaseName && @namespace.SequenceEqual(other.@namespace);

  public override int GetHashCode()
  {
    var hash = new HashCode();
    foreach (var part in @namespace)
    {
      hash.Add(part);
    }

    hash.Add(BaseName);
    return hash.ToHashCode();
  }

  internal string ToSource() => @namespace.Count == 0 ? BaseName : $"{string.Join("::", @namespace)}::{BaseName}";

  internal static EntityTypeName FromValidatedSource(string src)
  {
    var parts = src.Split("::", StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0)
    {
      throw new ArgumentException("Entity type name cannot be empty.", nameof(src));
    }

    return new EntityTypeName(parts[..^1], parts[^1]);
  }
}
