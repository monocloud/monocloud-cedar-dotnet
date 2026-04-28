namespace MonoCloud.Cedar.Model.Policy;

public sealed class TemplateLink
{
  public TemplateLink(string templateId, string resultPolicyId, IEnumerable<LinkValue> linkValues)
  {
    TemplateId = templateId;
    ResultPolicyId = resultPolicyId;
    LinkValues = [.. linkValues];
  }

  public string TemplateId { get; }

  [JsonPropertyName("newId")]
  public string ResultPolicyId { get; }

  [JsonIgnore]
  public IReadOnlyList<LinkValue> LinkValues { get; }

  [JsonPropertyName("values")]
  public IReadOnlyDictionary<string, EntityUID> Values => GetLinkValues();

  public string GetTemplateId() => TemplateId;

  public string GetResultPolicyId() => ResultPolicyId;

  public IReadOnlyDictionary<string, EntityUID> GetLinkValues() =>
    LinkValues.ToDictionary(x => x.GetSlot(), x => x.GetValue());
}
