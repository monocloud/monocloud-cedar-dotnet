using MonoCloud.Cedar.Value;

namespace MonoCloud.Cedar.UnitTests;

public sealed class EntityTypeNameTests
{
  private static readonly string[] Keywords = ["true", "false", "if", "then", "else", "in", "like", "has", "is"];

  [Fact]
  public void SimpleExample()
  {
    var value = EntityTypeName.Parse("hello");
    Assert.NotNull(value);
    Assert.Equal("hello", value.GetBaseName());
  }

  [Fact]
  public void SimpleWithNestedNamespace()
  {
    var value = EntityTypeName.Parse("com::cedarpolicy::value::EntityTypeName")!;

    Assert.Equal("EntityTypeName", value.GetBaseName());
    Assert.Equal(["com", "cedarpolicy", "value"], value.GetNamespaceComponents());
  }

  [Fact]
  public void InvalidValuesReturnNull()
  {
    Assert.Null(EntityTypeName.Parse("[]"));
    Assert.Null(EntityTypeName.Parse("foo::bar::bad!#f::another"));
    Assert.Null(EntityTypeName.Parse(""));
  }

  [Fact]
  public void RejectsKeywords()
  {
    foreach (var keyword in Keywords)
    {
      Assert.Null(EntityTypeName.Parse($"Foo::{keyword}::Bar"));
    }
  }

  [Fact]
  public void RoundTrip()
  {
    var name = EntityTypeName.Parse("Foo::Bar::Baz")!;
    var reparsed = EntityTypeName.Parse(name.ToString());

    Assert.Equal(name, reparsed);
    Assert.Equal(name.GetHashCode(), reparsed!.GetHashCode());
  }
}
