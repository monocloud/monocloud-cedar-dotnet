namespace MonoCloud.Cedar.Serializer;

public sealed class SchemaJsonConverter : JsonConverter<Schema>
{
  public override Schema? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
    reader.TokenType == JsonTokenType.String
      ? new Schema(reader.GetString()!)
      : new Schema(JsonDocument.ParseValue(ref reader).RootElement.Clone());

  public override void Write(Utf8JsonWriter writer, Schema value, JsonSerializerOptions options)
  {
    if (value.Type == Schema.JsonOrCedar.Cedar)
    {
      writer.WriteStringValue(value.SchemaText);
      return;
    }

    value.SchemaJson!.Value.WriteTo(writer);
  }
}
