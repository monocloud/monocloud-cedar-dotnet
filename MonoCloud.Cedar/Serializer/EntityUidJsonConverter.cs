namespace MonoCloud.Cedar.Serializer;

public sealed class EntityUidJsonConverter : JsonConverter<EntityUID>
{
  public override EntityUID? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var json = JsonSerializer.Deserialize<JsonEUID>(ref reader, options);
    return json is null ? null : EntityUID.ParseFromJson(json);
  }

  public override void Write(Utf8JsonWriter writer, EntityUID value, JsonSerializerOptions options) =>
    JsonSerializer.Serialize(writer, value.AsJson(), options);
}
