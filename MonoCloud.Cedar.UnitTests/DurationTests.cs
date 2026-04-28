using MonoCloud.Cedar.Value;

namespace MonoCloud.Cedar.UnitTests;

public sealed class DurationTests
{
  [Theory]
  [InlineData("0ms", 0L)]
  [InlineData("1ms", 1L)]
  [InlineData("1s", 1000L)]
  [InlineData("1m", 60000L)]
  [InlineData("1h", 3600000L)]
  [InlineData("1d", 86400000L)]
  [InlineData("1d2h3m4s5ms", 93784005L)]
  [InlineData("-4s200ms", -4200L)]
  public void ValidDurations(string value, long milliseconds)
  {
    Assert.Equal(milliseconds, new Duration(value).TotalMilliseconds);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("2h1d")]
  [InlineData("2d2d")]
  [InlineData("2x")]
  [InlineData("h")]
  [InlineData("1d-5h")]
  [InlineData("+1d5h")]
  [InlineData("1.23s")]
  [InlineData("9223372036854775808ms")]
  public void InvalidDurations(string value)
  {
    Assert.Throws<ArgumentException>(() => new Duration(value));
  }

  [Fact]
  public void SemanticEqualityAndCompare()
  {
    var oneHour = new Duration("1h");
    var sixtyMinutes = new Duration("60m");

    Assert.Equal(oneHour, sixtyMinutes);
    Assert.Equal(0, oneHour.CompareTo(sixtyMinutes));
    Assert.True(oneHour.CompareTo(new Duration("2h")) < 0);
    Assert.Equal("duration(\"1h\")", oneHour.ToCedarExpr());
  }
}
