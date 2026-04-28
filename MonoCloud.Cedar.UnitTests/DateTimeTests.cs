using CedarDateTime = MonoCloud.Cedar.Value.DateTime;

namespace MonoCloud.Cedar.UnitTests;

public sealed class DateTimeTests
{
  [Theory]
  [InlineData("2024-02-29", 1709164800000L)]
  [InlineData("2023-12-31T23:59:59Z", 1704067199000L)]
  [InlineData("2024-10-15T11:38:02+1134", 1728950642000L)]
  [InlineData("1969-12-31T23:59:59.999Z", -1L)]
  public void ValidDateTimes(string value, long epochMilliseconds)
  {
    var dateTime = new CedarDateTime(value);

    Assert.Equal(value, dateTime.ToString());
    Assert.Equal(epochMilliseconds, dateTime.ToEpochMilli());
  }

  [Theory]
  [InlineData("")]
  [InlineData("a")]
  [InlineData(" 2022-10-10")]
  [InlineData("2022-10-10 ")]
  [InlineData("2024-01-01T00:00:00")]
  [InlineData("2024-01-01T01:02")]
  [InlineData("2024-01-01T31:02:03Z")]
  [InlineData("2016-01-01T00:00:00+2400")]
  public void InvalidDateTimes(string value)
  {
    Assert.Throws<ArgumentException>(() => new CedarDateTime(value));
  }

  [Fact]
  public void SemanticEquality()
  {
    var date = new CedarDateTime("2023-12-25");
    var midnight = new CedarDateTime("2023-12-25T00:00:00Z");
    var eastern = new CedarDateTime("2023-12-25T07:00:00-0500");
    var utc = new CedarDateTime("2023-12-25T12:00:00Z");

    Assert.Equal(date, midnight);
    Assert.Equal(utc, eastern);
    Assert.Equal("datetime(\"2023-12-25\")", date.ToCedarExpr());
  }
}
