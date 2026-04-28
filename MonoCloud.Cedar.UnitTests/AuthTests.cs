using MonoCloud.Cedar.Model;
using MonoCloud.Cedar.Model.Entity;
using MonoCloud.Cedar.Model.Policy;
using MonoCloud.Cedar.Value;

namespace MonoCloud.Cedar.UnitTests;

public sealed class AuthTests
{
  private static void AssertAllowed(AuthorizationRequest request, PolicySet policySet, ISet<Entity> entities)
  {
    var engine = new BasicAuthorizationEngine();

    var responseWithEntities = engine.IsAuthorized(request, policySet, new Entities(entities));
    Assert.Equal(AuthorizationResponse.SuccessOrFailure.Success, responseWithEntities.Type);
    Assert.True(responseWithEntities.Success!.IsAllowed());

    var response = engine.IsAuthorized(request, policySet, entities);
    Assert.Equal(AuthorizationResponse.SuccessOrFailure.Success, response.Type);
    Assert.True(response.Success!.IsAllowed());
  }

  [Fact]
  public void Simple()
  {
    var alice = EntityTypeName.Parse("User")!.Of("alice");
    var view = EntityTypeName.Parse("Action")!.Of("view");
    var request = new AuthorizationRequest(alice, view, alice, new Dictionary<string, Value.Value>());
    var policySet = new PolicySet(new HashSet<Policy> { new("permit(principal,action,resource);", "p0") });

    AssertAllowed(request, policySet, new HashSet<Entity>());
  }

  [Fact]
  public void AuthWithMapContext()
  {
    var principal = EntityTypeName.Parse("User")!.Of("Alice");
    var action = EntityTypeName.Parse("Action")!.Of("View_Photo");
    var resource = EntityTypeName.Parse("Photo")!.Of("pic01");
    var context = new Dictionary<string, Value.Value> { ["authenticated"] = new PrimBool(true) };

    AssertAllowed(
      new AuthorizationRequest(principal, action, resource, context),
      BuildContextPolicySet(),
      BuildContextEntities());
  }

  [Fact]
  public void AuthWithContextObject()
  {
    var principal = EntityTypeName.Parse("User")!.Of("Alice");
    var action = EntityTypeName.Parse("Action")!.Of("View_Photo");
    var resource = EntityTypeName.Parse("Photo")!.Of("pic01");
    var context = new Context(new Dictionary<string, Value.Value> { ["authenticated"] = new PrimBool(true) });

    AssertAllowed(
      new AuthorizationRequest(principal, action, resource, context),
      BuildContextPolicySet(),
      BuildContextEntities());
  }

  [Fact]
  public void PartialAuthorizationConcrete()
  {
    var alice = EntityTypeName.Parse("User")!.Of("alice");
    var view = EntityTypeName.Parse("Action")!.Of("view");
    var request = PartialAuthorizationRequest.NewBuilder()
      .Principal(alice)
      .Action(view)
      .Resource(alice)
      .Context(new Dictionary<string, Value.Value>())
      .Build();

    var policySet = new PolicySet(new HashSet<Policy> { new("permit(principal == User::\"alice\",action,resource);", "p0") });
    var response = new BasicAuthorizationEngine().IsAuthorizedPartial(request, policySet, new HashSet<Entity>());

    Assert.Equal(AuthorizationSuccessResponse.Decision.Allow, response.Success!.GetDecision());
  }

  private static ISet<Entity> BuildContextEntities()
  {
    var principalType = EntityTypeName.Parse("User")!;
    var actionType = EntityTypeName.Parse("Action")!;
    var albumType = EntityTypeName.Parse("Album")!;
    var photoType = EntityTypeName.Parse("Photo")!;

    var album = new Entity(albumType.Of("Vacation"), new Dictionary<string, Value.Value>(), new HashSet<EntityUID>());
    var photo = new Entity(photoType.Of("pic01"), new Dictionary<string, Value.Value>(), new HashSet<EntityUID> { album.GetEUID() });

    return new HashSet<Entity>
    {
      photo,
      album,
      new(principalType.Of("Alice"), new Dictionary<string, Value.Value>(), new HashSet<EntityUID>()),
      new(actionType.Of("View_Photo"), new Dictionary<string, Value.Value>(), new HashSet<EntityUID>())
    };
  }

  private static PolicySet BuildContextPolicySet() =>
    new(new HashSet<Policy>
    {
      new("permit(principal == User::\"Alice\", action == Action::\"View_Photo\", resource in Album::\"Vacation\") when {context.authenticated == true};", "p1")
    });
}
