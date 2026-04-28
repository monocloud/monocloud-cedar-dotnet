using MonoCloud.Cedar.Model;
using MonoCloud.Cedar.Model.Policy;
using MonoCloud.Cedar.Model.Schema;
using MonoCloud.Cedar.Value;

namespace MonoCloud.Cedar.UnitTests;

public sealed class ValidationTests
{
  private readonly AuthorizationEngine engine = new BasicAuthorizationEngine();

  [Fact]
  public void GivenEmptySchemaAndNoPolicyReturnsValid()
  {
    var response = engine.Validate(new ValidationRequest(TestUtil.LoadJsonSchemaResource("empty_schema.json"), new PolicySet()));
    Assert.True(response.ValidationPassed());

    var levelResponse = engine.ValidateWithLevel(new LevelValidationRequest(TestUtil.LoadJsonSchemaResource("empty_schema.json"), new PolicySet(), 1));
    Assert.True(levelResponse.ValidationPassed());
  }

  [Fact]
  public void GivenExampleSchemaAndCorrectPolicyReturnsValid()
  {
    var schema = TestUtil.LoadJsonSchemaResource("photoflash_schema.json");
    var policies = new PolicySet(new HashSet<Policy>
    {
      new("permit(principal == User::\"alice\", action == Action::\"viewPhoto\", resource == Photo::\"VacationPhoto94.jpg\");", "policy0")
    });

    Assert.True(engine.Validate(new ValidationRequest(schema, policies)).ValidationPassed());
  }

  [Fact]
  public void GivenExampleSchemaAndIncorrectPolicyReturnsInvalid()
  {
    var schema = TestUtil.LoadJsonSchemaResource("photoflash_schema.json");
    var policies = new PolicySet(new HashSet<Policy>
    {
      new("permit(principal == User::\"alice\", action == Action::\"viewPhoto\", resource == User::\"bob\");", "policy0")
    });

    Assert.False(engine.Validate(new ValidationRequest(schema, policies)).ValidationPassed());
  }

  [Fact]
  public void ValidateTemplateLinkedPolicySuccess()
  {
    var schema = TestUtil.LoadJsonSchemaResource("library_schema.json");
    var templates = new HashSet<Policy>
    {
      new("permit(principal == ?principal, action, resource in ?resource);", "template0")
    };
    var links = new[]
    {
      new TemplateLink("template0", "policy0",
      [
        new LinkValue("?principal", EntityUID.Parse("Library::User::\"Victor\"")!),
        new LinkValue("?resource", EntityUID.Parse("Library::Book::\"The black Swan\"")!)
      ])
    };
    var policies = new PolicySet(new HashSet<Policy>(), templates, links);

    Assert.True(engine.Validate(new ValidationRequest(schema, policies)).ValidationPassed());
  }

  [Fact]
  public void ValidateLevelPolicyFailsWhenExpected()
  {
    var schema = TestUtil.LoadJsonSchemaResource("level_schema.json");
    var policies = new PolicySet(new HashSet<Policy>
    {
      new("""
        permit(
            principal in UserGroup::"alice_friends",
            action == Action::"viewPhoto",
            resource
        ) when {principal in resource.owner.friend};
        """, "policy0")
    });

    Assert.True(engine.Validate(new ValidationRequest(schema, policies)).ValidationPassed());
    Assert.False(engine.ValidateWithLevel(new LevelValidationRequest(schema, policies, 1)).ValidationPassed());
  }
}
