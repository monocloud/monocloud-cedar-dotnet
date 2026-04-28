namespace MonoCloud.Cedar.Serializer;

public sealed class EntityJsonConverter : JsonConverter<Entity>
{
  public override Entity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    using var document = JsonDocument.ParseValue(ref reader);
    var root = document.RootElement;

    if (!root.TryGetProperty("uid", out var uid) ||
        !root.TryGetProperty("attrs", out var attrs) ||
        !root.TryGetProperty("parents", out var parents))
    {
      throw new JsonException("Entity JSON must contain uid, attrs, and parents.");
    }

    var tags = root.TryGetProperty("tags", out var tagElement)
      ? tagElement.Deserialize<Dictionary<string, Value.Value>>(options)
      : new Dictionary<string, Value.Value>();

    return new Entity(
      uid.Deserialize<EntityUID>(options)!,
      attrs.Deserialize<Dictionary<string, Value.Value>>(options)!,
      parents.Deserialize<HashSet<EntityUID>>(options)!,
      tags);
  }

  public override void Write(Utf8JsonWriter writer, Entity value, JsonSerializerOptions options)
  {
    writer.WriteStartObject();
    writer.WritePropertyName("uid");
    JsonSerializer.Serialize(writer, value.GetEUID(), options);
    writer.WritePropertyName("attrs");
    JsonSerializer.Serialize(writer, value.Attrs, options);
    writer.WritePropertyName("parents");
    JsonSerializer.Serialize(writer, value.GetParents(), options);
    writer.WritePropertyName("tags");
    JsonSerializer.Serialize(writer, value.GetTags(), options);
    writer.WriteEndObject();
  }
}
