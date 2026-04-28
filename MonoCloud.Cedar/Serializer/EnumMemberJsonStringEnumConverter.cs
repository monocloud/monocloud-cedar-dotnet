namespace MonoCloud.Cedar.Serializer;

internal sealed class EnumMemberJsonStringEnumConverter : JsonConverterFactory
{
  public override bool CanConvert(Type typeToConvert) =>
    typeToConvert.IsEnum;

  public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
  {
    var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
    var converterType = typeof(EnumMemberConverter<>).MakeGenericType(enumType);
    return (JsonConverter)Activator.CreateInstance(converterType)!;
  }

  private sealed class EnumMemberConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
  {
    private static readonly Dictionary<string, TEnum> FromWire = BuildFromWire();
    private static readonly Dictionary<TEnum, string> ToWire = FromWire.ToDictionary(x => x.Value, x => x.Key);

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var value = reader.GetString();
      if (value is not null && FromWire.TryGetValue(value, out var parsed))
      {
        return parsed;
      }

      throw new JsonException($"Unknown {typeof(TEnum).Name} value '{value}'.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
      writer.WriteStringValue(ToWire.TryGetValue(value, out var wire) ? wire : value.ToString());

    private static Dictionary<string, TEnum> BuildFromWire()
    {
      var result = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);
      foreach (var name in Enum.GetNames<TEnum>())
      {
        var member = typeof(TEnum).GetMember(name).Single();
        var wire = member.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? JsonNamingPolicy.CamelCase.ConvertName(name);
        result[wire] = Enum.Parse<TEnum>(name);
      }

      return result;
    }
  }
}
