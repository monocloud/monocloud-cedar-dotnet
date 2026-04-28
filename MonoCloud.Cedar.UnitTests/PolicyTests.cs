using MonoCloud.Cedar.Model;
using MonoCloud.Cedar.Model.Exception;
using MonoCloud.Cedar.Model.Policy;

namespace MonoCloud.Cedar.UnitTests;

public sealed class PolicyTests
{
  [Fact]
  public void ParseStaticPolicyTests()
  {
    var policy1 = Policy.ParseStaticPolicy("permit(principal, action, resource);");
    var policy2 = Policy.ParseStaticPolicy("permit(principal, action, resource) when { principal has x && principal.x == 5};");

    Assert.NotEqual(policy1.PolicyID, policy2.PolicyID);
    Assert.Throws<InternalException>(() => Policy.ParseStaticPolicy("permit();"));
    Assert.Throws<ArgumentNullException>(() => Policy.ParseStaticPolicy(null!));
  }

  [Fact]
  public void ParsePolicyTemplateTests()
  {
    const string templateBody = "permit(principal == ?principal, action, resource in ?resource);";
    var template = Policy.ParsePolicyTemplate(templateBody);

    Assert.Equal(templateBody, template.PolicySrc);
    Assert.Throws<InternalException>(() => Policy.ParsePolicyTemplate("permit(principal in ?resource, action, resource);"));
    Assert.Throws<InternalException>(() => Policy.ParsePolicyTemplate("permit(principal, action, resource);"));
  }

  [Fact]
  public void StaticPolicyToJsonTests()
  {
    var policy = Policy.ParseStaticPolicy("permit(principal, action, resource);");
    const string expected = "{\"effect\":\"permit\",\"principal\":{\"op\":\"All\"},\"action\":{\"op\":\"All\"},\"resource\":{\"op\":\"All\"},\"conditions\":[]}";

    Assert.Equal(expected, policy.ToJson());
    Assert.Throws<InternalException>(() => new Policy("permit();").ToJson());
  }

  [Fact]
  public void PolicyFromJsonTest()
  {
    const string json = "{\"effect\":\"permit\",\"principal\":{\"op\":\"All\"},\"action\":{\"op\":\"All\"},\"resource\":{\"op\":\"All\"},\"conditions\":[]}";

    var policy = Policy.FromJson(null!, json);

    Assert.Equal(json, policy.ToJson());
    Assert.Throws<InternalException>(() => Policy.FromJson(null!, "effect\":\"permit\""));
  }

  [Fact]
  public void PolicyEffectTest()
  {
    Assert.Equal(Effect.Permit, new Policy("permit(principal, action, resource);").Effect());
    Assert.Equal(Effect.Forbid, new Policy("forbid(principal, action, resource);").Effect());
    Assert.Equal(Effect.Permit, new Policy("permit(principal == ?principal, action, resource == ?resource);").Effect());
    Assert.Throws<InternalException>(() => new Policy("perm(principal == ?principal, action, resource in ?resource);").Effect());
  }

  [Fact]
  public void StaticPolicyAnnotations()
  {
    var policy = Policy.ParseStaticPolicy("""
      @id("policyID1")
      @annotation("myAnnotation")
      @emptyAnnotation
      permit(principal, action, resource);
      """);

    Assert.Equal(new Dictionary<string, string>
    {
      ["id"] = "policyID1",
      ["annotation"] = "myAnnotation",
      ["emptyAnnotation"] = ""
    }, policy.GetAnnotations());
    Assert.Equal("myAnnotation", policy.GetAnnotation("annotation"));
    Assert.Null(Policy.ParseStaticPolicy("permit(principal, action, resource);").GetAnnotation("missing"));
  }

  [Fact]
  public void TemplatePolicyAnnotations()
  {
    var policy = Policy.ParsePolicyTemplate("""
      @id("policyID1")
      @annotation("myAnnotation")
      @emptyAnnotation
      permit(principal == ?principal, action, resource);
      """);

    Assert.Equal("myAnnotation", policy.GetAnnotation("annotation"));
    Assert.Equal("", policy.GetAnnotation("emptyAnnotation"));
  }
}
