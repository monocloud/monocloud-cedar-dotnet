namespace MonoCloud.Cedar.Model;

public sealed class SourceLabel
{
  public SourceLabel(string? label, int start, int end)
  {
    Label = label;
    Start = start;
    End = end;
  }

  [JsonPropertyName("label")]
  public string? Label { get; }

  [JsonPropertyName("start")]
  public int Start { get; }

  [JsonPropertyName("end")]
  public int End { get; }

  public override string ToString() => $"SourceLabel{{label=\"{Label ?? string.Empty}\", start={Start}, end={End}}}";
}
