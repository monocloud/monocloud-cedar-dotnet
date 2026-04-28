namespace MonoCloud.Cedar;

public sealed class BasicAuthorizationEngine : AuthorizationEngine
{
  private const string AuthorizationOperation = "AuthorizationOperation";
  private const string AuthorizationPartialOperation = "AuthorizationPartialOperation";
  private const string ValidateOperation = "ValidateOperation";
  private const string ValidateWithLevelOperation = "ValidateWithLevelOperation";
  private const string ValidateEntitiesOperation = "ValidateEntities";

#if NETSTANDARD2_0
  public static string GetCedarLangVersion() => "4.0";
#endif

  static BasicAuthorizationEngine()
  {
    var nativeVersion = CedarJson.NativeString(CedarFfi.CedarVersion);
#if NETSTANDARD2_0
    var langVersion = GetCedarLangVersion();
#else
    var langVersion = AuthorizationEngine.GetCedarLangVersion();
#endif
    if (nativeVersion != langVersion)
    {
      throw new TypeInitializationException(
        typeof(BasicAuthorizationEngine).FullName,
        new InvalidOperationException($"Error, .NET Cedar Language version is {langVersion} but native Cedar Language version is {nativeVersion}"));
    }
  }

  public AuthorizationResponse IsAuthorized(AuthorizationRequest request, PolicySet policySet, ISet<Entity> entities) =>
    Call<AuthorizationEnvelope, AuthorizationResponse>(
      AuthorizationOperation,
      new AuthorizationEnvelope(request, policySet, entities));

  public AuthorizationResponse IsAuthorized(AuthorizationRequest request, PolicySet policySet, Entities entities) =>
    IsAuthorized(request, policySet, entities.GetEntities());

  [Experimental(ExperimentalFeature.PartialEvaluation)]
  public PartialAuthorizationResponse IsAuthorizedPartial(PartialAuthorizationRequest request, PolicySet policySet, ISet<Entity> entities)
  {
    try
    {
      return Call<PartialAuthorizationEnvelope, PartialAuthorizationResponse>(
        AuthorizationPartialOperation,
        new PartialAuthorizationEnvelope(request, policySet, entities));
    }
    catch (InternalException ex) when (ex.Message.Contains(AuthorizationPartialOperation, StringComparison.Ordinal))
    {
      throw new MissingExperimentalFeatureException(ExperimentalFeature.PartialEvaluation);
    }
  }

  [Experimental(ExperimentalFeature.PartialEvaluation)]
  public PartialAuthorizationResponse IsAuthorizedPartial(PartialAuthorizationRequest request, PolicySet policySet, Entities entities) =>
    IsAuthorizedPartial(request, policySet, entities.GetEntities());

  public ValidationResponse Validate(ValidationRequest request) =>
    Call<ValidationRequest, ValidationResponse>(ValidateOperation, request);

  public ValidationResponse ValidateWithLevel(LevelValidationRequest request) =>
    Call<LevelValidationRequest, ValidationResponse>(ValidateWithLevelOperation, request);

  public void ValidateEntities(EntityValidationRequest request)
  {
    var input = CedarJson.Serialize(request);
    var json = CedarJson.NativeString(() => CedarFfi.CedarCall(ValidateEntitiesOperation, input));
    using var document = JsonDocument.Parse(json);
    var root = document.RootElement;
    var success = root.GetProperty("success").GetString() == "true";

    if (success)
    {
      return;
    }

    var errors = root.GetProperty("errors")
      .EnumerateArray()
      .Select(error => error.GetString() ?? string.Empty)
      .ToArray();

    if (root.GetProperty("isInternal").GetBoolean())
    {
      throw new InternalException(errors);
    }

    throw new BadRequestException(errors);
  }

  private static TResponse Call<TRequest, TResponse>(string operation, TRequest request)
  {
    try
    {
      var input = CedarJson.Serialize(request);
      var response = CedarJson.NativeString(() => CedarFfi.CedarCall(operation, input));
      return CedarJson.Deserialize<TResponse>(response);
    }
    catch (JsonException ex)
    {
      throw new AuthException("JSON Serialization Error", ex);
    }
    catch (ArgumentException ex)
    {
      throw new AuthException("Authorization error caused by illegal argument exception.", ex);
    }
  }

  private sealed class AuthorizationEnvelope : AuthorizationRequest
  {
    public AuthorizationEnvelope(AuthorizationRequest request, PolicySet policies, ISet<Entity> entities)
#if NETSTANDARD2_0
      : base(request.PrincipalEUID, request.ActionEUID, request.ResourceEUID, request.Context is null ? null : request.Context.ToDictionary(x => x.Key, x => x.Value), request.Schema, request.EnableRequestValidation)
#else
      : base(request.PrincipalEUID, request.ActionEUID, request.ResourceEUID, request.Context?.ToDictionary(), request.Schema, request.EnableRequestValidation)
#endif
    {
      Policies = policies;
      Entities = entities;
    }

    [JsonPropertyName("policies")]
    public PolicySet Policies { get; }

    [JsonPropertyName("entities")]
    public ISet<Entity> Entities { get; }
  }

  private sealed class PartialAuthorizationEnvelope : PartialAuthorizationRequest
  {
    public PartialAuthorizationEnvelope(PartialAuthorizationRequest request, PolicySet policies, ISet<Entity> entities)
#if NETSTANDARD2_0
      : base(request.Principal, request.Action, request.Resource, request.Context is null ? null : request.Context.ToDictionary(x => x.Key, x => x.Value), request.Schema, request.EnableRequestValidation)
#else
      : base(request.Principal, request.Action, request.Resource, request.Context?.ToDictionary(), request.Schema, request.EnableRequestValidation)
#endif
    {
      Policies = policies;
      Entities = entities;
    }

    [JsonPropertyName("policies")]
    public PolicySet Policies { get; }

    [JsonPropertyName("entities")]
    public ISet<Entity> Entities { get; }
  }

}
