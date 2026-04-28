using MonoCloud.Cedar.Value;

namespace MonoCloud.Cedar.UnitTests;

public sealed class EntityUIDTests
{
  [Fact]
  public void Simple()
  {
    var euid = EntityUID.Parse("Foo::\"alice\"")!;

    Assert.Equal(new EntityIdentifier("alice"), euid.GetId());
    Assert.Equal(EntityTypeName.Parse("Foo"), euid.GetTypeName());
  }

  [Fact]
  public void SimpleNested()
  {
    var euid = EntityUID.Parse("Foo::Bar::\"alice\"")!;

    Assert.Equal(new EntityIdentifier("alice"), euid.GetId());
    Assert.Equal(EntityTypeName.Parse("Foo::Bar"), euid.GetTypeName());
  }

  [Theory]
  [InlineData("Foo:Bar::\"alice\"")]
  [InlineData("Foo::bar::\"alice")]
  [InlineData("Foo::Bar::Baz")]
  [InlineData("")]
  [InlineData("\"test\"")]
  public void BadValuesReturnNull(string value)
  {
    Assert.Null(EntityUID.Parse(value));
  }

  [Fact]
  public void EmptyIdRoundTrips()
  {
    var euid = new EntityUID(EntityTypeName.Parse("Foo")!, "");
    Assert.Equal(euid, EntityUID.Parse(euid.ToString()));
  }
}
