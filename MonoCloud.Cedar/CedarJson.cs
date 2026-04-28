namespace MonoCloud.Cedar;

public static class CedarJson
{
  public static readonly JsonSerializerOptions Options = CreateOptions();

  private static JsonSerializerOptions CreateOptions()
  {
    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = false
    };

    options.Converters.Add(new ValueJsonConverter());
    options.Converters.Add(new EntityUidJsonConverter());
    options.Converters.Add(new EntityJsonConverter());
    options.Converters.Add(new SchemaJsonConverter());
    options.Converters.Add(new EnumMemberJsonStringEnumConverter());
    return options;
  }

  public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

  public static T Deserialize<T>(string json) =>
    JsonSerializer.Deserialize<T>(json, Options) ?? throw new JsonException($"Could not deserialize {typeof(T).Name}.");

  public static string NativeString(Func<IntPtr> call)
  {
    var ptr = call();
    try
    {
      return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
    }
    finally
    {
      CedarFfi.FreeString(ptr);
    }
  }

  public static string NativeAnswer(Func<IntPtr> call)
  {
    var answer = Deserialize<CedarStringAnswer>(NativeString(call));
    if (answer.Success == "true")
    {
      return answer.Result ?? string.Empty;
    }

    throw new Model.Exception.InternalException(answer.Errors ?? []);
  }

  public sealed class CedarStringAnswer
  {
    public string? Success { get; set; }
    public string? Result { get; set; }
    public IReadOnlyList<string>? Errors { get; set; }
  }
}
