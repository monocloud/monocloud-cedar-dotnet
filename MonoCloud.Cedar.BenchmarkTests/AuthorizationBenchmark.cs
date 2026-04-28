using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using MonoCloud.Cedar.Model;
using MonoCloud.Cedar.Model.Entity;
using MonoCloud.Cedar.Model.Policy;
using MonoCloud.Cedar.Model.Schema;
using MonoCloud.Cedar.Value;

namespace MonoCloud.Cedar.BenchmarkTests;

[Config(typeof(AuthorizationBenchmarkConfig))]
[BenchmarkCategory("Authorization")]
public class AuthorizationBenchmark
{
  private BasicAuthorizationEngine engine = null!;

  private AuthorizationRequest smallRequest = null!;
  private PolicySet smallPolicySet = null!;
  private ISet<Entity> smallEntities = null!;

  private AuthorizationRequest mediumRequest = null!;
  private PolicySet mediumPolicySet = null!;
  private ISet<Entity> mediumEntities = null!;

  private ValidationRequest validationRequest = null!;

  [GlobalSetup]
  public void SetUp()
  {
    engine = new BasicAuthorizationEngine();

    SetUpSmallScenario();
    SetUpMediumScenario();
    SetUpValidationScenario();
  }

  [Benchmark]
  public AuthorizationResponse IsAuthorizedSmall() =>
    engine.IsAuthorized(smallRequest, smallPolicySet, smallEntities);

  [Benchmark]
  public AuthorizationResponse IsAuthorizedMedium() =>
    engine.IsAuthorized(mediumRequest, mediumPolicySet, mediumEntities);

  [Benchmark]
  public ValidationResponse ValidateSmall() =>
    engine.Validate(validationRequest);

  private void SetUpSmallScenario()
  {
    var userType = EntityTypeName.Parse("User")!;
    var actionType = EntityTypeName.Parse("Action")!;
    var resourceType = EntityTypeName.Parse("Resource")!;

    smallRequest = new AuthorizationRequest(
      userType.Of("alice"),
      actionType.Of("view"),
      resourceType.Of("doc1"),
      new Dictionary<string, Value.Value>());

    smallPolicySet = PolicySet.ParsePolicies(ReadResource("small_policies.cedar"));
    smallEntities = Entities.Parse(ReadResource("small_entities.json")).GetEntities();
  }

  private void SetUpMediumScenario()
  {
    var userType = EntityTypeName.Parse("User")!;
    var actionType = EntityTypeName.Parse("Action")!;
    var photoType = EntityTypeName.Parse("Photo")!;

    mediumRequest = new AuthorizationRequest(
      userType.Of("alice"),
      actionType.Of("View_Photo"),
      photoType.Of("pic01"),
      new Dictionary<string, Value.Value>());

    mediumPolicySet = PolicySet.ParsePolicies(ReadResource("medium_policies.cedar"));
    mediumEntities = Entities.Parse(ReadResource("medium_entities.json")).GetEntities();
  }

  private void SetUpValidationScenario()
  {
    var schema = Schema.Parse(Schema.JsonOrCedar.Json, ReadResource("photoflash_schema.json"));
    var policySet = PolicySet.ParsePolicies(
      "permit(principal == User::\"alice\", action == Action::\"View_Photo\", resource);");

    validationRequest = new ValidationRequest(schema, policySet);
  }

  private static string ReadResource(string resourceName) =>
    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Resources", resourceName));
}

public sealed class AuthorizationBenchmarkConfig : ManualConfig
{
  public AuthorizationBenchmarkConfig()
  {
    AddJob(Job.Default
      .WithToolchain(InProcessNoEmitToolchain.Instance)
      .WithLaunchCount(1)
      .WithWarmupCount(3)
      .WithIterationCount(5));
    AddDiagnoser(MemoryDiagnoser.Default);
  }
}
