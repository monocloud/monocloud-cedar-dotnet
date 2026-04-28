namespace MonoCloud.Cedar.Model;

public sealed class DetailedError
{
  public DetailedError(
    string message,
    string? help = null,
    string? code = null,
    string? url = null,
    Severity? severity = null,
    IReadOnlyList<SourceLabel>? sourceLocations = null,
    IReadOnlyList<DetailedError>? related = null)
  {
    Message = message;
    Help = help;
    Code = code;
    Url = url;
    Severity = severity;
    SourceLocations = sourceLocations ?? [];
    Related = related ?? [];
  }

  [JsonPropertyName("message")]
  public string Message { get; }

  [JsonPropertyName("help")]
  public string? Help { get; }

  [JsonPropertyName("code")]
  public string? Code { get; }

  [JsonPropertyName("url")]
  public string? Url { get; }

  [JsonPropertyName("severity")]
  public Severity? Severity { get; }

  [JsonPropertyName("sourceLocations")]
  public IReadOnlyList<SourceLabel> SourceLocations { get; }

  [JsonPropertyName("related")]
  public IReadOnlyList<DetailedError> Related { get; }

  public override string ToString() => Message;
}
