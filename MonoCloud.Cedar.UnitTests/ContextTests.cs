using MonoCloud.Cedar.Model;
using MonoCloud.Cedar.Value;

namespace MonoCloud.Cedar.UnitTests;

public sealed class ContextTests
{
  private static Dictionary<string, Value.Value> ValidMap() => new()
  {
    ["key1"] = new PrimString("value1"),
    ["key2"] = new PrimLong(999),
    ["key3"] = new PrimBool(true)
  };

  private static Dictionary<string, Value.Value> MergedMap() => new(ValidMap())
  {
    ["key4"] = new PrimString("value1"),
    ["key5"] = new PrimLong(999)
  };

  [Fact]
  public void GivenValidIterableConstructorConstructs()
  {
    var context = new Context(ValidMap());
    Assert.Equal(ValidMap(), context.GetContext());
  }

  [Fact]
  public void GivenDuplicateKeyIterableConstructorThrows()
  {
    var entries = new[]
    {
      KeyValuePair.Create<string, Value.Value>("key1", new PrimString("value1")),
      KeyValuePair.Create<string, Value.Value>("key1", new PrimLong(999))
    };

    Assert.Throws<ArgumentException>(() => entries.ToDictionary());
  }

  [Fact]
  public void GivenValidIterableMergeMerges()
  {
    var context = new Context(ValidMap());
    context.Merge(new Dictionary<string, Value.Value>
    {
      ["key4"] = new PrimString("value1"),
      ["key5"] = new PrimLong(999)
    });

    Assert.Equal(MergedMap(), context.GetContext());
  }

  [Fact]
  public void GivenExistingKeyMergeThrows()
  {
    var context = new Context(ValidMap());

    Assert.Throws<InvalidOperationException>(() => context.Merge(new Dictionary<string, Value.Value>
    {
      ["key3"] = new PrimString("value1"),
      ["key5"] = new PrimLong(999)
    }));
  }

  [Fact]
  public void GetReturnsValueOrNull()
  {
    var context = new Context(ValidMap());

    Assert.Equal(new PrimString("value1"), context.Get("key1"));
    Assert.Null(context.Get("invalidKey"));
  }
}
