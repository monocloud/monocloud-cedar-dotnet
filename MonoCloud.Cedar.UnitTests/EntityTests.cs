using MonoCloud.Cedar.Model.Entity;
using MonoCloud.Cedar.Value;
using System.Text.Json;

namespace MonoCloud.Cedar.UnitTests;

public sealed class EntityTests
{
  [Fact]
  public void GetAttrTests()
  {
    var attr = new PrimString("stringAttrValue");
    var principalType = EntityTypeName.Parse("User")!;
    var principal = new Entity(
      principalType.Of("Alice"),
      new Dictionary<string, Value.Value> { ["stringAttr"] = attr },
      new HashSet<EntityUID>());

    Assert.Equal(attr, principal.GetAttr("stringAttr"));
    Assert.Null(principal.GetAttr("decimalAttr"));
  }

  [Fact]
  public void NewWithEntityUidTests()
  {
    var principalType = EntityTypeName.Parse("User")!;
    var principal = new Entity(principalType.Of("Alice"));

    Assert.Equal(principalType.Of("Alice"), principal.GetEUID());
    Assert.Null(principal.GetAttr("stringAttr"));
    Assert.Empty(principal.GetParents());
  }

  [Fact]
  public void NewWithoutAttributesTests()
  {
    var principalType = EntityTypeName.Parse("User")!;
    var parents = new HashSet<EntityUID> { principalType.Of("Bob") };
    var principal = new Entity(principalType.Of("Alice"), parents);

    Assert.Equal(principalType.Of("Alice"), principal.GetEUID());
    Assert.Equal(parents, principal.GetParents());
  }

  [Fact]
  public void GivenValidJsonStringParseReturns()
  {
    var entity = Entity.Parse("""
      {"uid":{"type":"Photo","id":"pic01"},
       "attrs":{"dummyIP":{"__extn":{"fn":"ip","arg":"192.168.1.100"}}},
       "parents":[{"type":"Photo","id":"pic01"}],
       "tags":{"dummyTagUser":{"__entity":{"type":"User::Tag","id":"Alice"}}}}
      """);

    Assert.Equal(EntityTypeName.Parse("Photo")!.Of("pic01"), entity.GetEUID());
    Assert.Single(entity.Attrs);
    Assert.Single(entity.GetParents());
    Assert.Single(entity.GetTags());
  }

  [Fact]
  public void GivenInvalidJsonStringParseThrows()
  {
    Assert.Throws<JsonException>(() => Entity.Parse("""{"uid":{"type":"Photo","id":"pic01"}}"""));
    Assert.Throws<JsonException>(() => Entity.Parse("""{"uid":{"type":"Photo","id":"pic01"},"parents":{},"attrs":[]}"""));
  }

  [Fact]
  public void GivenJsonFileParseReturns()
  {
    var entity = Entity.ParseFile(TestUtil.ResourcePath("valid_entity.json"));
    Assert.Equal(EntityTypeName.Parse("Photo")!.Of("pic01"), entity.GetEUID());
  }
}
