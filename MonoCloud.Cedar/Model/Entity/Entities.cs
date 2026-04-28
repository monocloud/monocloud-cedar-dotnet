namespace MonoCloud.Cedar.Model.Entity;

public sealed class Entities
{
  private readonly HashSet<Entity> entities;

  public Entities() => entities = [];

  public Entities(ISet<Entity> entities) => this.entities = [.. entities];

  public ISet<Entity> GetEntities() => new HashSet<Entity>(entities);

  public static Entities Parse(string jsonString)
  {
    using var document = System.Text.Json.JsonDocument.Parse(jsonString);
    var root = document.RootElement;
    var entitiesElement = root.ValueKind == System.Text.Json.JsonValueKind.Object && root.TryGetProperty("entities", out var nested)
      ? nested
      : root;

    return new(System.Text.Json.JsonSerializer.Deserialize<HashSet<Entity>>(entitiesElement.GetRawText(), CedarJson.Options)!);
  }

  public static Entities ParseFile(string filePath) => Parse(File.ReadAllText(filePath));

  public override string ToString() => string.Join(Environment.NewLine, entities.Select(x => x.ToString()));
}
