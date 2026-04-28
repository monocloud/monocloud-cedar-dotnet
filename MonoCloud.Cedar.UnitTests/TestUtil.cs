using MonoCloud.Cedar.Model.Policy;
using MonoCloud.Cedar.Model.Schema;
using MonoCloud.Cedar.Value;
using System.Text.Json;

namespace MonoCloud.Cedar.UnitTests;

internal static class TestUtil
{
  public static string ResourcePath(string name) =>
    Path.Combine(AppContext.BaseDirectory, "Resources", name);

  public static string ReadResource(string name) => File.ReadAllText(ResourcePath(name));

  public static Schema LoadJsonSchemaResource(string name) =>
    Schema.Parse(Schema.JsonOrCedar.Json, ReadResource(name));

  public static Schema LoadCedarSchemaResource(string name) =>
    Schema.Parse(Schema.JsonOrCedar.Cedar, ReadResource(name));

  public static PolicySet BuildValidPolicySet()
  {
    var policies = new HashSet<Policy>
    {
      new("permit(principal == User::\"Bob\", action == Action::\"View_Photo\", resource in Album::\"Vacation\");", "p1")
    };

    var templates = new HashSet<Policy>
    {
      new("permit(principal == ?principal, action == Action::\"View_Photo\", resource in Album::\"Vacation\");", "t0")
    };

    var templateLinks = new[]
    {
      new TemplateLink("t0", "tl0", [new LinkValue("?principal", EntityUID.Parse("User::\"Alice\"")!)])
    };

    return new PolicySet(policies, templates, templateLinks);
  }

  public static PolicySet BuildInvalidPolicySet() =>
    new(new HashSet<Policy> { new("permit();", "p0") });

  public static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();
}
