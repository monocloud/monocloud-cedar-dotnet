namespace MonoCloud.Cedar.Formatter;

public static class PolicyFormatter
{
  public static string PoliciesStrToPretty(string policies) =>
    CedarJson.NativeAnswer(() => CedarFfi.CedarPoliciesStrToPretty(policies));

  public static string PoliciesStrToPrettyWithConfig(string policies, Config config) =>
    CedarJson.NativeAnswer(() => CedarFfi.CedarPoliciesStrToPrettyWithConfig(policies, (nuint)config.GetLineWidth(), (nint)config.GetIndentWidth()));
}
