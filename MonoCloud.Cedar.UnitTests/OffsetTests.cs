using MonoCloud.Cedar.Value;
using MonoCloud.Cedar.Value.Functions;
using CedarDateTime = MonoCloud.Cedar.Value.DateTime;

namespace MonoCloud.Cedar.UnitTests;

public sealed class OffsetTests
{
  [Fact]
  public void ValidJsonSerialization()
  {
    var dateTime = new CedarDateTime("2023-12-25T10:30:45Z");
    var duration = new Duration("2h30m");
    var offset = new Offset(dateTime, duration);

    var json = CedarJson.Serialize<Value.Value>(offset);

    const string expected = "{\"__extn\":{\"fn\":\"offset\",\"args\":["
                            + "{\"__extn\":{\"fn\":\"datetime\",\"arg\":\"2023-12-25T10:30:45Z\"}},"
                            + "{\"__extn\":{\"fn\":\"duration\",\"arg\":\"2h30m\"}}]}}";
    Assert.Equal(expected, json);
  }

  [Fact]
  public void ValidJsonDeserialization()
  {
    const string json = "{\"__extn\":{\"fn\":\"offset\",\"args\":["
                        + "{\"__extn\":{\"fn\":\"datetime\",\"arg\":\"2023-01-01T00:00:00Z\"}},"
                        + "{\"__extn\":{\"fn\":\"duration\",\"arg\":\"1d5h\"}}]}}";

    var offset = (Offset)CedarJson.Deserialize<Value.Value>(json);

    Assert.Equal("2023-01-01T00:00:00Z", offset.DateTime.ToString());
    Assert.Equal("1d5h", offset.OffsetDuration.ToString());
    Assert.Equal("datetime(\"2023-01-01T00:00:00Z\").offset(duration(\"1d5h\"))", offset.ToCedarExpr());
  }

  [Fact]
  public void InvalidJsonDeserialization()
  {
    const string invalidJson = "{\"__extn\":{\"fn\":\"offset\",\"args\":\"invalid-offset\"}}";
    Assert.ThrowsAny<Exception>(() => CedarJson.Deserialize<Value.Value>(invalidJson));

    const string invalidDateTimeJson = "{\"__extn\":{\"fn\":\"offset\",\"args\":["
                                       + "{\"__extn\":{\"fn\":\"datetime\",\"arg\":\"invalid-date\"}},"
                                       + "{\"__extn\":{\"fn\":\"duration\",\"arg\":\"1h\"}}]}}";
    Assert.ThrowsAny<Exception>(() => CedarJson.Deserialize<Value.Value>(invalidDateTimeJson));

    const string invalidDurationJson = "{\"__extn\":{\"fn\":\"offset\",\"args\":["
                                       + "{\"__extn\":{\"fn\":\"datetime\",\"arg\":\"2023-01-01T00:00:00Z\"}},"
                                       + "{\"__extn\":{\"fn\":\"duration\",\"arg\":\"invalid-duration\"}}]}}";
    Assert.ThrowsAny<Exception>(() => CedarJson.Deserialize<Value.Value>(invalidDurationJson));
  }

  [Theory]
  [InlineData("2023-12-25T10:30:45Z", "2h30m")]
  [InlineData("2023-01-01T00:00:00Z", "1d")]
  [InlineData("2023-06-15T14:22:33.123Z", "5m30s")]
  [InlineData("2023-03-10T08:45:12+0500", "1h15m45s")]
  [InlineData("2023-11-30T23:59:59-0800", "-30m")]
  [InlineData("2023-02-28", "24h")]
  [InlineData("2023-07-04T12:00:00.999Z", "1ms")]
  public void JsonRoundTrip(string dateTimeString, string durationString)
  {
    var original = new Offset(new CedarDateTime(dateTimeString), new Duration(durationString));

    var json = CedarJson.Serialize<Value.Value>(original);
    var deserialized = (Offset)CedarJson.Deserialize<Value.Value>(json);

    Assert.Equal(original.DateTime.ToString(), deserialized.DateTime.ToString());
    Assert.Equal(original.OffsetDuration.ToString(), deserialized.OffsetDuration.ToString());
    Assert.Equal(original.ToCedarExpr(), deserialized.ToCedarExpr());
  }
}
