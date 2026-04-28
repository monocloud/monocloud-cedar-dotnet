namespace MonoCloud.Cedar.Value;

public sealed partial class DateTime : Value
{
  // 0000-01-01T00:00:00+2359 — earliest representable instant.
  private const long MinInstantMs = -62167305540000L;
  // 9999-12-31T23:59:59-2359 — latest representable instant.
  private const long MaxInstantMs = 253402387139000L;

  private readonly string dateTime;
  private readonly long epochMillis;

  public DateTime(string dateTime)
  {
    if (dateTime is null || !TryParse(dateTime, out var ms))
    {
      throw new ArgumentException($"Input string is not a supported DateTime format: {dateTime}", nameof(dateTime));
    }

    this.dateTime = dateTime;
    epochMillis = ms;
  }

  public override string ToCedarExpr() => $"datetime(\"{dateTime}\")";

  public override string ToString() => dateTime;

  public long ToEpochMilli() => epochMillis;

  public override bool Equals(object? obj) => obj is DateTime other && epochMillis == other.epochMillis;

  public override int GetHashCode() => epochMillis.GetHashCode();

  private static bool TryParse(string text, out long result)
  {
    result = 0;
    var match = DateTimeRegex().Match(text);
    if (!match.Success)
    {
      return false;
    }

    var year = ParseInt(match.Groups["year"]);
    var month = ParseInt(match.Groups["month"]);
    var day = ParseInt(match.Groups["day"]);
    if (month < 1 || month > 12 || day < 1 || day > DaysInMonth(year, month))
    {
      return false;
    }

    var hour = 0;
    var minute = 0;
    var second = 0;
    var millisecond = 0;
    var offsetMillis = 0L;

    if (match.Groups["hour"].Success)
    {
      hour = ParseInt(match.Groups["hour"]);
      minute = ParseInt(match.Groups["minute"]);
      second = ParseInt(match.Groups["second"]);
      if (hour > 23 || minute > 59 || second > 59)
      {
        return false;
      }

      if (match.Groups["ms"].Success)
      {
        millisecond = ParseInt(match.Groups["ms"]);
      }

      if (match.Groups["sign"].Success)
      {
        var offsetHours = ParseInt(match.Groups["offh"]);
        var offsetMinutes = ParseInt(match.Groups["offm"]);
        if (offsetHours > 23 || offsetMinutes > 59)
        {
          return false;
        }

        offsetMillis = (offsetHours * 60L + offsetMinutes) * 60_000L;
        if (match.Groups["sign"].Value == "-")
        {
          offsetMillis = -offsetMillis;
        }
      }
    }

    var days = DaysFromCivil(year, month, day);
    var ms = checked((days * 86_400L + hour * 3_600L + minute * 60L + second) * 1_000L + millisecond - offsetMillis);
    if (ms < MinInstantMs || ms > MaxInstantMs)
    {
      return false;
    }

    result = ms;
    return true;
  }

  private static int DaysInMonth(int year, int month) => month switch
  {
    1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
    4 or 6 or 9 or 11 => 30,
    2 => IsLeap(year) ? 29 : 28,
    _ => 0
  };

  private static bool IsLeap(int year) => (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;

  // Howard Hinnant's days_from_civil — proleptic Gregorian, supports year 0 and earlier.
  private static long DaysFromCivil(int year, int month, int day)
  {
    var y = year - (month <= 2 ? 1 : 0);
    var era = (y >= 0 ? y : y - 399) / 400;
    var yoe = y - era * 400;
    var doy = (153 * (month > 2 ? month - 3 : month + 9) + 2) / 5 + day - 1;
    var doe = yoe * 365 + yoe / 4 - yoe / 100 + doy;
    return era * 146097L + doe - 719468L;
  }

  private static int ParseInt(Group group) =>
#if NETSTANDARD2_0
    int.Parse(group.Value);
#else
    int.Parse(group.ValueSpan);
#endif

#if NETSTANDARD2_0
  private static readonly Regex DateTimeRegexInstance =
    new(@"^(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})(?:T(?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2})(?:\.(?<ms>\d{3}))?(?:Z|(?<sign>[+-])(?<offh>\d{2})(?<offm>\d{2})))?$", RegexOptions.Compiled);

  private static Regex DateTimeRegex() => DateTimeRegexInstance;
#else
  [GeneratedRegex(@"^(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})(?:T(?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2})(?:\.(?<ms>\d{3}))?(?:Z|(?<sign>[+-])(?<offh>\d{2})(?<offm>\d{2})))?$")]
  private static partial Regex DateTimeRegex();
#endif
}
