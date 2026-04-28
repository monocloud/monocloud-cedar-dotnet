using MonoCloud.Cedar.Model.Exception;
using MonoCloud.Cedar.Model.Schema;
using System.Text.Json;

namespace MonoCloud.Cedar.UnitTests;

public sealed class SchemaTests
{
  [Fact]
  public void ParseJsonSchema()
  {
    _ = Schema.Parse(Schema.JsonOrCedar.Json, "{}");
    _ = Schema.Parse(Schema.JsonOrCedar.Json, """
      {
        "Foo::Bar": {
          "entityTypes": {},
          "actions": {}
        }
      }
      """);

    Assert.Throws<InternalException>(() => Schema.Parse(Schema.JsonOrCedar.Json, "{\"foo\": \"bar\"}"));
  }

  [Fact]
  public void ParseCedarSchema()
  {
    _ = Schema.Parse(Schema.JsonOrCedar.Cedar, "");
    _ = Schema.Parse(Schema.JsonOrCedar.Cedar, "namespace Foo::Bar {}");

    Assert.Throws<InternalException>(() => Schema.Parse(Schema.JsonOrCedar.Cedar, "namspace Foo::Bar;"));
  }

  [Fact]
  public void ToCedarFormat()
  {
    var schema = new Schema("entity User;");
    Assert.Equal("entity User;", schema.ToCedarFormat());

    var jsonSchema = Schema.Parse(Schema.JsonOrCedar.Json, """
      {
        "": {
          "entityTypes": {
            "User": {}
          },
          "actions": {}
        }
      }
      """);

    Assert.Equal("entity User;", jsonSchema.ToCedarFormat().Trim());
  }

  [Fact]
  public void ToJsonFormat()
  {
    var schema = new Schema("entity User;");
    var result = schema.ToJsonFormat();
    var expected = JsonDocument.Parse("{\"\":{\"entityTypes\":{\"User\":{}},\"actions\":{}}}").RootElement;

    Assert.Equal(
      JsonSerializer.Serialize(expected, CedarJson.Options),
      JsonSerializer.Serialize(result, CedarJson.Options));
  }

  [Fact]
  public void EnumSchemasParse()
  {
    Assert.NotNull(TestUtil.LoadJsonSchemaResource("enum_schema.json"));
    Assert.NotNull(TestUtil.LoadCedarSchemaResource("enum_schema.cedarschema"));
    Assert.Throws<InternalException>(() => Schema.Parse(Schema.JsonOrCedar.Cedar, "entity Color enum [];"));
  }
}
