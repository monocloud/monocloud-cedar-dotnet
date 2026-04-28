using MonoCloud.Cedar.Value;

namespace MonoCloud.Cedar.UnitTests;

public sealed class EntityIdTests
{
  [Theory]
  [InlineData("")]
  [InlineData("alice")]
  [InlineData("ali\"ce")]
  [InlineData("line\nbreak")]
  public void AnyStringCanBeRepresented(string value)
  {
    var id = new EntityIdentifier(value);
    Assert.True(id.GetRepr().Length >= value.Length);
  }
}
