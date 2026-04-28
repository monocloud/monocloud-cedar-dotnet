using MonoCloud.Cedar.Model.Entity;

namespace MonoCloud.Cedar.UnitTests;

public sealed class EntitiesTests
{
  [Fact]
  public void ParseValidJsonString()
  {
    var entities = Entities.Parse("""
      [
        {"uid":{"type":"User","id":"Alice"},"attrs":{},"parents":[]},
        {"uid":{"type":"Photo","id":"pic01"},"attrs":{},"parents":[{"type":"Album","id":"Vacation"}]}
      ]
      """);

    Assert.Equal(2, entities.GetEntities().Count);
  }

  [Fact]
  public void ParseValidJsonFile()
  {
    var entities = Entities.ParseFile(TestUtil.ResourcePath("valid_entities.json"));
    Assert.NotEmpty(entities.GetEntities());
  }
}
