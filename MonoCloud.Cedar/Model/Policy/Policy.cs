namespace MonoCloud.Cedar.Model.Policy;

public sealed class Policy
{
  private static int idCounter;
  private Dictionary<string, string>? annotations;

  public Policy(string policySrc, string? policyID = null)
  {
    PolicySrc = policySrc ?? throw new ArgumentNullException(nameof(policySrc));
    PolicyID = policyID ?? $"policy{Interlocked.Increment(ref idCounter)}";
  }

  [JsonPropertyName("policySrc")]
  public string PolicySrc { get; }

  [JsonPropertyName("policyID")]
  public string PolicyID { get; }

  public string GetID() => PolicyID;

  public string GetSource() => PolicySrc;

  public Effect Effect()
  {
    try
    {
      return ParseEffect(CedarJson.NativeAnswer(() => CedarFfi.CedarPolicyEffect(PolicySrc)));
    }
    catch (InternalException)
    {
      return ParseEffect(CedarJson.NativeAnswer(() => CedarFfi.CedarTemplateEffect(PolicySrc)));
    }
  }

  public string ToJson() => CedarJson.NativeAnswer(() => CedarFfi.CedarPolicyToJson(PolicySrc));

  public static Policy FromJson(string policyId, string policyJson) =>
    new(CedarJson.NativeAnswer(() => CedarFfi.CedarPolicyFromJson(policyJson ?? throw new ArgumentNullException(nameof(policyJson)))), policyId);

  public static Policy ParseStaticPolicy(string policyStr) =>
    new(CedarJson.NativeAnswer(() => CedarFfi.CedarParsePolicy(policyStr ?? throw new ArgumentNullException(nameof(policyStr)))));

  public static Policy ParsePolicyTemplate(string templateStr) =>
    new(CedarJson.NativeAnswer(() => CedarFfi.CedarParsePolicyTemplate(templateStr ?? throw new ArgumentNullException(nameof(templateStr)))));

  public IReadOnlyDictionary<string, string> GetAnnotations()
  {
    EnsureAnnotationsLoaded();
    return new Dictionary<string, string>(annotations!);
  }

  public string? GetAnnotation(string key)
  {
    EnsureAnnotationsLoaded();
#if NETSTANDARD2_0
    return annotations!.TryGetValue(key, out var value) ? value : null;
#else
    return annotations!.GetValueOrDefault(key);
#endif
  }

  public override string ToString() => $"// Policy ID: {PolicyID}\n{PolicySrc}";

  private void EnsureAnnotationsLoaded()
  {
    if (annotations is not null)
    {
      return;
    }

    try
    {
      annotations = CedarJson.Deserialize<Dictionary<string, string>>(CedarJson.NativeAnswer(() => CedarFfi.CedarGetPolicyAnnotations(PolicySrc)));
    }
    catch (InternalException ex) when (ex.Message.Contains("expected a static policy", StringComparison.OrdinalIgnoreCase))
    {
      annotations = CedarJson.Deserialize<Dictionary<string, string>>(CedarJson.NativeAnswer(() => CedarFfi.CedarGetTemplateAnnotations(PolicySrc)));
    }
  }

  private static Model.Effect ParseEffect(string effect) =>
    effect.Equals("permit", StringComparison.OrdinalIgnoreCase) ? Model.Effect.Permit :
    effect.Equals("forbid", StringComparison.OrdinalIgnoreCase) ? Model.Effect.Forbid :
    throw new ArgumentException($"Invalid Effect: {effect}. Expected 'permit' or 'forbid'");
}
