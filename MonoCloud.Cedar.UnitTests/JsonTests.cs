using MonoCloud.Cedar.Model;
using MonoCloud.Cedar.Value;
using System.Text.Json;

namespace MonoCloud.Cedar.UnitTests;

public sealed class JsonTests
{
  [Fact]
  public void TestRequest()
  {
    var gandalf = EntityTypeName.Parse("Wizard")!.Of("gandalf");
    var opens = EntityTypeName.Parse("Action")!.Of("opens");
    var moria = EntityTypeName.Parse("Mines")!.Of("moria");
    var request = new AuthorizationRequest(gandalf, opens, moria, new Dictionary<string, Value.Value>());

    var json = JsonSerializer.Serialize(request, CedarJson.Options);

    Assert.Contains("\"principal\":{\"type\":\"Wizard\",\"id\":\"gandalf\"}", json);
    Assert.Contains("\"action\":{\"type\":\"Action\",\"id\":\"opens\"}", json);
    Assert.Contains("\"resource\":{\"type\":\"Mines\",\"id\":\"moria\"}", json);
    Assert.Contains("\"validateRequest\":false", json);
  }

  [Fact]
  public void TestPrimitiveValues()
  {
    Assert.Equal("3000000000", JsonSerializer.Serialize<Value.Value>(new PrimLong(3000000000L), CedarJson.Options));
    Assert.Equal("true", JsonSerializer.Serialize<Value.Value>(new PrimBool(true), CedarJson.Options));
    Assert.Equal("\"hello world\"", JsonSerializer.Serialize<Value.Value>(new PrimString("hello world"), CedarJson.Options));
  }

  [Fact]
  public void TestEntityUidAsValue()
  {
    var uid = EntityUID.Parse("silver::\"jakob\"")!;
    var json = JsonSerializer.Serialize<Value.Value>(uid, CedarJson.Options);

    Assert.Equal("{\"__entity\":{\"type\":\"silver\",\"id\":\"jakob\"}}", json);
  }

  [Fact]
  public void TestListAndMap()
  {
    var list = new CedarList([new PrimLong(5), new PrimBool(false)]);
    var map = new CedarMap(new Dictionary<string, Value.Value> { ["name"] = new PrimString("alice") });

    Assert.Equal("[5,false]", JsonSerializer.Serialize<Value.Value>(list, CedarJson.Options));
    Assert.Equal("{\"name\":\"alice\"}", JsonSerializer.Serialize<Value.Value>(map, CedarJson.Options));
  }

  [Fact]
  public void TestUnknown()
  {
    var json = JsonSerializer.Serialize<Value.Value>(new Unknown("test"), CedarJson.Options);
    Assert.Equal("{\"__extn\":{\"fn\":\"unknown\",\"arg\":\"test\"}}", json);
  }
}
