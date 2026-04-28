namespace MonoCloud.Cedar.Serializer;

public sealed class ValueJsonConverter : JsonConverter<Value.Value>
{
  public override Value.Value? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
    reader.TokenType switch
    {
      JsonTokenType.True => new PrimBool(true),
      JsonTokenType.False => new PrimBool(false),
      JsonTokenType.String => new PrimString(reader.GetString()!),
      JsonTokenType.Number => new PrimLong(reader.GetInt64()),
      JsonTokenType.StartArray => ReadList(ref reader, options),
      JsonTokenType.StartObject => ReadObject(ref reader, options),
      _ => throw new JsonException($"Unexpected token {reader.TokenType} when parsing Cedar value.")
    };

  public override void Write(Utf8JsonWriter writer, Value.Value value, JsonSerializerOptions options)
  {
    switch (value)
    {
      case PrimBool boolValue:
        writer.WriteBooleanValue(boolValue.GetValue());
        break;
      case PrimLong longValue:
        writer.WriteNumberValue(longValue.GetValue());
        break;
      case PrimString stringValue:
        writer.WriteStringValue(stringValue.GetValue());
        break;
      case CedarList list:
        writer.WriteStartArray();
        foreach (var item in list)
        {
          Write(writer, item, options);
        }

        writer.WriteEndArray();
        break;
      case CedarMap map:
        writer.WriteStartObject();
        foreach (var item in map)
        {
          writer.WritePropertyName(item.Key);
          Write(writer, item.Value, options);
        }

        writer.WriteEndObject();
        break;
      case EntityUID entityUid:
        WriteEntity(writer, entityUid.AsJson());
        break;
      case CedarDecimal decimalValue:
        WriteExtension(writer, "decimal", decimalValue.ToString());
        break;
      case CedarDateTime dateTimeValue:
        WriteExtension(writer, "datetime", dateTimeValue.ToString());
        break;
      case Duration durationValue:
        WriteExtension(writer, "duration", durationValue.ToString());
        break;
      case IpAddress ipAddressValue:
        WriteExtension(writer, "ip", ipAddressValue.ToString());
        break;
      case Unknown unknown:
        WriteExtension(writer, "unknown", unknown.ToString());
        break;
      case Offset offset:
        WriteOffset(writer, offset, options);
        break;
      default:
        throw new NotSupportedException($"Unsupported Cedar value type: {value.GetType()}");
    }
  }

  private CedarList ReadList(ref Utf8JsonReader reader, JsonSerializerOptions options)
  {
    var result = new CedarList();
    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
    {
      result.Add(Read(ref reader, typeof(Value.Value), options)!);
    }

    return result;
  }

  private Value.Value ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
  {
    using var document = JsonDocument.ParseValue(ref reader);
    var root = document.RootElement;

    if (root.TryGetProperty("__entity", out var entity))
    {
      return EntityUID.ParseFromJson(entity.Deserialize<JsonEUID>(options)!)!;
    }

    if (root.TryGetProperty("__extn", out var extension))
    {
      var fn = extension.GetProperty("fn").GetString();
      if (fn == "offset")
      {
        return ReadOffset(extension, options);
      }

      var arg = extension.GetProperty("arg").GetString()!;
      return fn switch
      {
        "decimal" => new CedarDecimal(arg),
        "datetime" => new CedarDateTime(arg),
        "duration" => new Duration(arg),
        "ip" => new IpAddress(arg),
        "unknown" => new Unknown(arg),
        _ => throw new NotSupportedException($"Unsupported Cedar extension value: {fn}")
      };
    }

    var result = new CedarMap();
    foreach (var property in root.EnumerateObject())
    {
      var valueReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(property.Value.GetRawText()));
      valueReader.Read();
      result.Add(property.Name, Read(ref valueReader, typeof(Value.Value), options)!);
    }

    return result;
  }

  private static void WriteEntity(Utf8JsonWriter writer, JsonEUID value)
  {
    writer.WriteStartObject();
    writer.WritePropertyName("__entity");
    JsonSerializer.Serialize(writer, value, CedarJson.Options);
    writer.WriteEndObject();
  }

  private static void WriteExtension(Utf8JsonWriter writer, string fn, string arg)
  {
    writer.WriteStartObject();
    writer.WriteStartObject("__extn");
    writer.WriteString("fn", fn);
    writer.WriteString("arg", arg);
    writer.WriteEndObject();
    writer.WriteEndObject();
  }

  private void WriteOffset(Utf8JsonWriter writer, Offset offset, JsonSerializerOptions options)
  {
    writer.WriteStartObject();
    writer.WriteStartObject("__extn");
    writer.WriteString("fn", "offset");
    writer.WriteStartArray("args");
    Write(writer, offset.DateTime, options);
    Write(writer, offset.OffsetDuration, options);
    writer.WriteEndArray();
    writer.WriteEndObject();
    writer.WriteEndObject();
  }

  private Offset ReadOffset(JsonElement extension, JsonSerializerOptions options)
  {
    if (!extension.TryGetProperty("args", out var args) || args.ValueKind != JsonValueKind.Array)
    {
      throw new JsonException("offset extension requires an 'args' array.");
    }

    if (args.GetArrayLength() != 2)
    {
      throw new JsonException($"offset extension requires exactly two arguments but got {args.GetArrayLength()}.");
    }

    var dateTime = args[0].Deserialize<Value.Value>(options) as CedarDateTime
      ?? throw new JsonException("offset first argument must be a datetime.");
    var duration = args[1].Deserialize<Value.Value>(options) as Duration
      ?? throw new JsonException("offset second argument must be a duration.");

    return new Offset(dateTime, duration);
  }
}
