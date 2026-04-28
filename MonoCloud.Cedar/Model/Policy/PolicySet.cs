namespace MonoCloud.Cedar.Model.Policy;

public sealed class PolicySet
{
  public PolicySet()
  {
    Policies = new HashSet<Policy>();
    Templates = new HashSet<Policy>();
    TemplateLinks = [];
  }

  public PolicySet(ISet<Policy> policies) : this(policies, new HashSet<Policy>(), [])
  {
  }

  public PolicySet(ISet<Policy> policies, ISet<Policy> templates) : this(policies, templates, [])
  {
  }

  public PolicySet(ISet<Policy> policies, ISet<Policy> templates, IReadOnlyList<TemplateLink> templateLinks)
  {
    Policies = policies;
    Templates = templates;
    TemplateLinks = templateLinks;
  }

  [JsonIgnore]
  public ISet<Policy> Policies { get; }

  [JsonIgnore]
  public ISet<Policy> Templates { get; }

  [JsonPropertyName("templateLinks")]
  public IReadOnlyList<TemplateLink> TemplateLinks { get; }

  [JsonPropertyName("staticPolicies")]
  public IReadOnlyDictionary<string, string> StaticPolicies => GetStaticPolicies();

  [JsonPropertyName("templates")]
  public IReadOnlyDictionary<string, string> TemplateMap => GetTemplates();

  public IReadOnlyDictionary<string, string> GetStaticPolicies() => Policies.ToDictionary(x => x.GetID(), x => x.GetSource());

  public IReadOnlyDictionary<string, string> GetTemplates() => Templates.ToDictionary(x => x.GetID(), x => x.GetSource());

  public int GetNumPolicies() => Policies.Count;

  public int GetNumTemplates() => Templates.Count;

  public string ToJson() => CedarJson.NativeAnswer(() => Interop.CedarFfi.CedarPolicySetToJson(CedarJson.Serialize(this)));

  public static PolicySet ParsePoliciesFile(string filePath) => ParsePolicies(File.ReadAllText(filePath));

  public static PolicySet ParsePolicies(string policiesString)
  {
    using var document = JsonDocument.Parse(CedarJson.NativeAnswer(() => Interop.CedarFfi.CedarParsePolicies(policiesString)));
    var root = document.RootElement;

    var policies = root.GetProperty("staticPolicies")
      .EnumerateArray()
      .Select(x => new Policy(x.GetProperty("text").GetString()!, x.GetProperty("id").GetString()))
#if NETSTANDARD2_0
      .ToList();
#else
      .ToHashSet();
#endif
    var templates = root.GetProperty("templates")
      .EnumerateArray()
      .Select(x => new Policy(x.GetProperty("text").GetString()!, x.GetProperty("id").GetString()))
#if NETSTANDARD2_0
      .ToList();
#else
      .ToHashSet();
#endif

#if NETSTANDARD2_0
    return new PolicySet(new HashSet<Policy>(policies), new HashSet<Policy>(templates));
#else
    return new PolicySet(policies, templates);
#endif
  }
}
