namespace MonoCloud.Cedar.Value;

public sealed partial class Duration : Value, IComparable<Duration>
{
  private readonly string durationString;

  public Duration(string duration)
  {
    durationString = duration ?? throw new ArgumentNullException(nameof(duration));
    TotalMilliseconds = ParseToMilliseconds(duration);
  }

  public long TotalMilliseconds { get; }

  public int CompareTo(Duration? other) => other is null ? 1 : TotalMilliseconds.CompareTo(other.TotalMilliseconds);

  public override string ToCedarExpr() => $"duration(\"{durationString}\")";

  public override string ToString() => durationString;

  private static long ParseToMilliseconds(string text)
  {
    var match = DurationRegex().Match(text);
    if (!match.Success || string.IsNullOrWhiteSpace(text))
    {
      throw new ArgumentException($"Input string is not a supported Duration format: {text}", nameof(text));
    }

    var sign = match.Groups["sign"].Value == "-" ? -1 : 1;
    var total = 0L;
    try
    {
      total = checked(total + Read(match, "days") * 86_400_000L);
      total = checked(total + Read(match, "hours") * 3_600_000L);
      total = checked(total + Read(match, "minutes") * 60_000L);
      total = checked(total + Read(match, "seconds") * 1_000L);
      total = checked(total + Read(match, "milliseconds"));
    }
    catch (OverflowException ex)
    {
      throw new ArgumentException($"Input string is not a supported Duration format: {text}", nameof(text), ex);
    }

    if (!match.Groups.Values.Skip(1).Any(x => x.Success && x.Name != "sign"))
    {
      throw new ArgumentException($"Input string is not a supported Duration format: {text}", nameof(text));
    }

    return sign * total;
  }

  private static long Read(Match match, string name) =>
    match.Groups[name].Success ? long.Parse(match.Groups[name].Value) : 0;

  [GeneratedRegex("^(?<sign>-?)(?:(?:(?<days>\\d+)d)?(?:(?<hours>\\d+)h)?(?:(?<minutes>\\d+)m(?!s))?(?:(?<seconds>\\d+)s)?(?:(?<milliseconds>\\d+)ms)?)$")]
  private static partial Regex DurationRegex();

  public override bool Equals(object? obj) => obj is Duration other && TotalMilliseconds == other.TotalMilliseconds;

  public override int GetHashCode() => TotalMilliseconds.GetHashCode();
}
