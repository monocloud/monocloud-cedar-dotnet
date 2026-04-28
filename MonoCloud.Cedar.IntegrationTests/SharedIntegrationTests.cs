using MonoCloud.Cedar.Model;
using MonoCloud.Cedar.Model.Entity;
using MonoCloud.Cedar.Model.Exception;
using MonoCloud.Cedar.Model.Policy;
using MonoCloud.Cedar.Model.Schema;
using MonoCloud.Cedar.Value;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MonoCloud.Cedar.IntegrationTests;

public sealed class SharedIntegrationTests
{
  private static readonly string[] HandwrittenTestFiles =
  [
    "tests/decimal/1.json",
    "tests/decimal/2.json",
    "tests/example_use_cases/1a.json",
    "tests/example_use_cases/2a.json",
    "tests/example_use_cases/2b.json",
    "tests/example_use_cases/2c.json",
    "tests/example_use_cases/3a.json",
    "tests/example_use_cases/3b.json",
    "tests/example_use_cases/3c.json",
    "tests/example_use_cases/4a.json",
    "tests/example_use_cases/4d.json",
    "tests/example_use_cases/4e.json",
    "tests/example_use_cases/4f.json",
    "tests/example_use_cases/5b.json",
    "tests/ip/1.json",
    "tests/ip/2.json",
    "tests/ip/3.json",
    "tests/multi/1.json",
    "tests/multi/2.json",
    "tests/multi/3.json",
    "tests/multi/4.json",
    "tests/multi/5.json"
  ];

  private static readonly string? IntegrationTestsRoot = ResolveIntegrationTestsRoot();

  public static IEnumerable<object[]> TestFiles()
  {
    if (IntegrationTestsRoot is null)
    {
      yield return ["<integration test fixtures missing>"];
      yield break;
    }

    foreach (var file in HandwrittenTestFiles)
    {
      yield return [file];
    }

    var corpusDir = Path.Combine(IntegrationTestsRoot, "corpus-tests");
    if (Directory.Exists(corpusDir))
    {
      foreach (var path in Directory.EnumerateFiles(corpusDir, "*.json")
                 .Where(p => !p.EndsWith(".entities.json", StringComparison.Ordinal))
                 .OrderBy(p => p, StringComparer.Ordinal))
      {
        yield return [Path.GetRelativePath(IntegrationTestsRoot, path).Replace('\\', '/')];
      }
    }
  }

  [Theory]
  [MemberData(nameof(TestFiles))]
  public void IntegrationTest(string testFile)
  {
    if (IntegrationTestsRoot is null)
    {
      return;
    }

    var test = JsonSerializer.Deserialize<JsonTest>(
      File.ReadAllText(ResolvePath(testFile)),
      CedarJson.Options) ?? throw new InvalidOperationException($"Could not parse {testFile}");

    var policySet = PolicySet.ParsePoliciesFile(ResolvePath(test.Policies));
    var entities = LoadEntities(ResolvePath(test.Entities));
    var schema = new Schema(File.ReadAllText(ResolvePath(test.Schema)));

    ExecuteValidationTest(policySet, schema, test.ShouldValidate);
    foreach (var request in test.Requests)
    {
      ExecuteRequestTest(entities, policySet, request, schema);
    }
  }

  private static void ExecuteValidationTest(PolicySet policies, Schema schema, bool shouldValidate)
  {
    var auth = new BasicAuthorizationEngine();
    ValidationResponse result;
    try
    {
      result = auth.Validate(new ValidationRequest(schema, policies));
    }
    catch (InternalException)
    {
      Assert.False(shouldValidate);
      return;
    }

    if (result.Type == ValidationResponse.SuccessOrFailure.Failure)
    {
      Assert.False(shouldValidate);
      return;
    }

    if (shouldValidate)
    {
      var errors = result.Success?.ValidationErrors ?? [];
      Assert.True(result.ValidationPassed(),
        string.Join(", ", errors.Select(e => e.GetError().Message)));
    }
  }

  private static void ExecuteRequestTest(ISet<Entity> entities, PolicySet policySet, JsonRequest request, Schema schema)
  {
    var auth = new BasicAuthorizationEngine();
    var authRequest = new AuthorizationRequest(
      EntityUID.ParseFromJson(request.Principal)!,
      EntityUID.ParseFromJson(request.Action)!,
      EntityUID.ParseFromJson(request.Resource)!,
      request.Context,
      schema,
      request.ValidateRequest);

    var response = auth.IsAuthorized(authRequest, policySet, entities);

    if (response.Type == AuthorizationResponse.SuccessOrFailure.Failure)
    {
      // Parse errors map to Deny in the integration tests.
      Assert.Equal(AuthorizationSuccessResponse.Decision.Deny, request.Decision);
      return;
    }

    var success = response.Success!;
    Assert.Equal(request.Decision, success.GetDecision());
    Assert.Equal(new HashSet<string>(request.Reason), success.GetReason());
    // The integration tests only record the id of the erroring policy, not the full error message,
    // so just check the list lengths match.
    Assert.Equal(request.Errors.Count, success.GetErrors().Count);
  }

  private static ISet<Entity> LoadEntities(string entitiesFile)
  {
    var json = File.ReadAllText(entitiesFile);
    var entities = JsonSerializer.Deserialize<List<Entity>>(json, CedarJson.Options)
      ?? throw new InvalidOperationException($"Could not parse entities file {entitiesFile}");
    return new HashSet<Entity>(entities);
  }

  private static string ResolvePath(string path)
  {
    var resolved = Path.IsPathRooted(path)
      ? path
      : Path.Combine(IntegrationTestsRoot!, path);
    return Path.GetFullPath(resolved);
  }

  private static string? ResolveIntegrationTestsRoot()
  {
    var envOverride = Environment.GetEnvironmentVariable("CEDAR_INTEGRATION_TESTS_ROOT");
    if (!string.IsNullOrEmpty(envOverride) && Directory.Exists(envOverride))
    {
      return Path.GetFullPath(envOverride);
    }

    var candidates = new[]
    {
      Path.Combine(AppContext.BaseDirectory, "Resources", "cedar-integration-tests-main"),
      Path.Combine(AppContext.BaseDirectory, "cedar-integration-tests-main"),
      Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "IntegrationTests", "cedar-integration-tests-main")
    };

    return candidates.Select(Path.GetFullPath).FirstOrDefault(Directory.Exists);
  }

  private sealed class JsonTest
  {
    [JsonPropertyName("policies")] public string Policies { get; set; } = string.Empty;
    [JsonPropertyName("entities")] public string Entities { get; set; } = string.Empty;
    [JsonPropertyName("schema")] public string Schema { get; set; } = string.Empty;
    [JsonPropertyName("shouldValidate")] public bool ShouldValidate { get; set; }
    [JsonPropertyName("requests")] public List<JsonRequest> Requests { get; set; } = [];
  }

  private sealed class JsonRequest
  {
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("principal")] public JsonEUID Principal { get; set; } = null!;
    [JsonPropertyName("action")] public JsonEUID Action { get; set; } = null!;
    [JsonPropertyName("resource")] public JsonEUID Resource { get; set; } = null!;
    [JsonPropertyName("context")] public Dictionary<string, Value.Value>? Context { get; set; }
    [JsonPropertyName("validateRequest")] public bool ValidateRequest { get; set; } = true;
    [JsonPropertyName("decision")] public AuthorizationSuccessResponse.Decision Decision { get; set; }
    [JsonPropertyName("reason")] public List<string> Reason { get; set; } = [];
    [JsonPropertyName("errors")] public List<string> Errors { get; set; } = [];
  }
}
