using MonoCloud.Cedar.Model;
using MonoCloud.Cedar.Model.Entity;
using MonoCloud.Cedar.Model.Exception;
using MonoCloud.Cedar.Value;

namespace MonoCloud.Cedar.UnitTests;

public sealed class EntityValidationTests
{
  [Fact]
  public void ValidEntitiesPass()
  {
    var userType = EntityTypeName.Parse("User")!;
    var taskType = EntityTypeName.Parse("Task")!;
    var colorType = EntityTypeName.Parse("Color")!;
    var user = new Entity(userType.Of("alice"), new Dictionary<string, Value.Value>
    {
      ["name"] = new PrimString("Alice")
    }, new HashSet<EntityUID>());
    var task = new Entity(taskType.Of("task1"), new Dictionary<string, Value.Value>
    {
      ["owner"] = user.GetEUID(),
      ["name"] = new PrimString("Complete project"),
      ["status"] = colorType.Of("Red")
    }, new HashSet<EntityUID>());

    new BasicAuthorizationEngine().ValidateEntities(new EntityValidationRequest(TestUtil.LoadJsonSchemaResource("enum_schema.json"), [user, task]));
  }

  [Fact]
  public void InvalidEnumEntitiesThrowBadRequest()
  {
    var userType = EntityTypeName.Parse("User")!;
    var taskType = EntityTypeName.Parse("Task")!;
    var colorType = EntityTypeName.Parse("Color")!;
    var user = new Entity(userType.Of("alice"), new Dictionary<string, Value.Value>
    {
      ["name"] = new PrimString("Alice")
    }, new HashSet<EntityUID>());
    var task = new Entity(taskType.Of("task1"), new Dictionary<string, Value.Value>
    {
      ["owner"] = user.GetEUID(),
      ["name"] = new PrimString("Complete project"),
      ["status"] = colorType.Of("Purple")
    }, new HashSet<EntityUID>());

    Assert.Throws<BadRequestException>(() =>
      new BasicAuthorizationEngine().ValidateEntities(new EntityValidationRequest(TestUtil.LoadJsonSchemaResource("enum_schema.json"), [user, task])));
  }
}
