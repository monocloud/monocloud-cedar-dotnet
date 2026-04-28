using CedarDecimal = MonoCloud.Cedar.Value.Decimal;

namespace MonoCloud.Cedar.UnitTests;

public sealed class DecimalTests
{
  [Theory]
  [InlineData("1.0")]
  [InlineData("1.0000")]
  [InlineData("-1.0")]
  [InlineData("0.0")]
  [InlineData("123.456")]
  [InlineData("-000000922337203685477.5808")]
  [InlineData("000000922337203685477.5807")]
  public void ValidDecimals(string value)
  {
    _ = new CedarDecimal(value);
  }

  [Theory]
  [InlineData("")]
  [InlineData("abc")]
  [InlineData("1")]
  [InlineData("1.00000")]
  [InlineData("922337203685477.5808")]
  [InlineData("-922337203685477.5809")]
  [InlineData(".1")]
  [InlineData("1.")]
  [InlineData("+1.0")]
  [InlineData("1.0e2")]
  [InlineData(" 1.0")]
  public void InvalidDecimals(string value)
  {
    Assert.Throws<ArgumentException>(() => new CedarDecimal(value));
  }

  [Fact]
  public void ToCedarExprAndEquality()
  {
    var decimalValue = new CedarDecimal("1.0000");

    Assert.Equal("decimal(\"1.0000\")", decimalValue.ToCedarExpr());
    Assert.Equal(decimalValue, new CedarDecimal("1.0000"));
  }
}
