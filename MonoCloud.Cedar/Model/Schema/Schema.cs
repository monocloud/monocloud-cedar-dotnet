namespace MonoCloud.Cedar.Model.Schema;

[JsonConverter(typeof(SchemaJsonConverter))]
public sealed class Schema
{
  public Schema(JsonElement schemaJson)
  {
    Type = JsonOrCedar.Json;
    SchemaJson = schemaJson;
  }

  public Schema(string schemaText)
  {
    Type = JsonOrCedar.Cedar;
    SchemaText = schemaText;
  }

  public JsonOrCedar Type { get; }

  public JsonElement? SchemaJson { get; }

  public string? SchemaText { get; }

  public static Schema Parse(JsonOrCedar type, string str)
  {
    ArgumentNullException.ThrowIfNull(str);
    if (type == JsonOrCedar.Json)
    {
      CedarJson.NativeAnswer(() => CedarFfi.CedarParseJsonSchema(str));
      return new Schema(JsonDocument.Parse(str).RootElement.Clone());
    }

    CedarJson.NativeAnswer(() => CedarFfi.CedarParseCedarSchema(str));
    return new Schema(str);
  }

  public string ToCedarFormat() =>
    Type == JsonOrCedar.Cedar
      ? SchemaText!
      : CedarJson.NativeAnswer(() => CedarFfi.CedarJsonToCedarSchema(SchemaJson!.Value.GetRawText()));

  public JsonElement ToJsonFormat()
  {
    if (Type == JsonOrCedar.Json)
    {
      return SchemaJson!.Value;
    }

    var json = CedarJson.NativeAnswer(() => CedarFfi.CedarCedarToJsonSchema(SchemaText!));
    return JsonDocument.Parse(json).RootElement.Clone();
  }

  public override string ToString() =>
    Type == JsonOrCedar.Json ? $"Schema(schemaJson={SchemaJson})" : $"Schema(schemaText={SchemaText})";

  public enum JsonOrCedar
  {
    Json,
    Cedar
  }
}
