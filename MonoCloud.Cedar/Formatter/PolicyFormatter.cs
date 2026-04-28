namespace MonoCloud.Cedar.Formatter;

public static class PolicyFormatter
{
  public static string PoliciesStrToPretty(string policies) =>
    CedarJson.NativeAnswer(() => CedarFfi.CedarPoliciesStrToPretty(policies));

  public static string PoliciesStrToPrettyWithConfig(string policies, Config config) =>
#if NETSTANDARD2_0
    CedarJson.NativeAnswer(() => CedarFfi.CedarPoliciesStrToPrettyWithConfig(policies, (UIntPtr)config.GetLineWidth(), (IntPtr)config.GetIndentWidth()));
#else
    CedarJson.NativeAnswer(() => CedarFfi.CedarPoliciesStrToPrettyWithConfig(policies, (nuint)config.GetLineWidth(), (nint)config.GetIndentWidth()));
#endif
}
