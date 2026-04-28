namespace MonoCloud.Cedar.Value;

public sealed class EntityIdentifier(string id)
{
  private readonly string id = id;

  public string GetRepr() => CedarJson.NativeAnswer(() => CedarFfi.CedarEntityIdentifierRepr(id));

  public override string ToString() => id;

  public override bool Equals(object? obj) => obj is EntityIdentifier other && id == other.id;

  public override int GetHashCode() => id.GetHashCode();
}
