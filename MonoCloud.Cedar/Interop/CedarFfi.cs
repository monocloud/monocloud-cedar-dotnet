namespace MonoCloud.Cedar.Interop;

internal static partial class CedarFfi
{
#if NETSTANDARD2_0
  private delegate IntPtr NativeStringCall(IntPtr value);
  private delegate IntPtr NativeStringPairCall(IntPtr left, IntPtr right);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_free_string")]
  public static extern void FreeString(IntPtr value);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_version")]
  public static extern IntPtr CedarVersion();

  public static IntPtr CedarCall(string operation, string input) =>
    WithUtf8Strings(operation, input, CedarCallNative);

  public static IntPtr CedarParsePolicy(string policy) =>
    WithUtf8String(policy, CedarParsePolicyNative);

  public static IntPtr CedarPolicyToJson(string policy) =>
    WithUtf8String(policy, CedarPolicyToJsonNative);

  public static IntPtr CedarPolicyFromJson(string policyJson) =>
    WithUtf8String(policyJson, CedarPolicyFromJsonNative);

  public static IntPtr CedarPolicySetToJson(string policySet) =>
    WithUtf8String(policySet, CedarPolicySetToJsonNative);

  public static IntPtr CedarParsePolicies(string policies) =>
    WithUtf8String(policies, CedarParsePoliciesNative);

  public static IntPtr CedarGetPolicyAnnotations(string policy) =>
    WithUtf8String(policy, CedarGetPolicyAnnotationsNative);

  public static IntPtr CedarGetTemplateAnnotations(string template) =>
    WithUtf8String(template, CedarGetTemplateAnnotationsNative);

  public static IntPtr CedarParsePolicyTemplate(string template) =>
    WithUtf8String(template, CedarParsePolicyTemplateNative);

  public static IntPtr CedarPolicyEffect(string policy) =>
    WithUtf8String(policy, CedarPolicyEffectNative);

  public static IntPtr CedarTemplateEffect(string template) =>
    WithUtf8String(template, CedarTemplateEffectNative);

  public static IntPtr CedarEntityIdentifierRepr(string id) =>
    WithUtf8String(id, CedarEntityIdentifierReprNative);

  public static IntPtr CedarParseEntityTypeName(string typeName) =>
    WithUtf8String(typeName, CedarParseEntityTypeNameNative);

  public static IntPtr CedarEntityTypeNameRepr(string typeName) =>
    WithUtf8String(typeName, CedarEntityTypeNameReprNative);

  public static IntPtr CedarParseEntityUid(string entityUid) =>
    WithUtf8String(entityUid, CedarParseEntityUidNative);

  public static IntPtr CedarEuidRepr(string typeName, string id) =>
    WithUtf8Strings(typeName, id, CedarEuidReprNative);

  public static IntPtr CedarParseJsonSchema(string schema) =>
    WithUtf8String(schema, CedarParseJsonSchemaNative);

  public static IntPtr CedarParseCedarSchema(string schema) =>
    WithUtf8String(schema, CedarParseCedarSchemaNative);

  public static IntPtr CedarPoliciesStrToPretty(string policies) =>
    WithUtf8String(policies, CedarPoliciesStrToPrettyNative);

  public static IntPtr CedarPoliciesStrToPrettyWithConfig(string policies, UIntPtr lineWidth, IntPtr indentWidth) =>
    WithUtf8String(policies, value => CedarPoliciesStrToPrettyWithConfigNative(value, lineWidth, indentWidth));

  public static IntPtr CedarJsonToCedarSchema(string jsonSchema) =>
    WithUtf8String(jsonSchema, CedarJsonToCedarSchemaNative);

  public static IntPtr CedarCedarToJsonSchema(string cedarSchema) =>
    WithUtf8String(cedarSchema, CedarCedarToJsonSchemaNative);

  private static IntPtr WithUtf8String(string value, NativeStringCall call)
  {
    var ptr = StringToHGlobalUtf8(value);
    try
    {
      return call(ptr);
    }
    finally
    {
      FreeHGlobal(ptr);
    }
  }

  private static IntPtr WithUtf8Strings(string left, string right, NativeStringPairCall call)
  {
    var leftPtr = StringToHGlobalUtf8(left);
    var rightPtr = StringToHGlobalUtf8(right);
    try
    {
      return call(leftPtr, rightPtr);
    }
    finally
    {
      FreeHGlobal(rightPtr);
      FreeHGlobal(leftPtr);
    }
  }

  private static IntPtr StringToHGlobalUtf8(string value)
  {
    if (value is null)
    {
      return IntPtr.Zero;
    }

    var bytes = System.Text.Encoding.UTF8.GetBytes(value);
    var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
    Marshal.Copy(bytes, 0, ptr, bytes.Length);
    Marshal.WriteByte(ptr, bytes.Length, 0);
    return ptr;
  }

  private static void FreeHGlobal(IntPtr ptr)
  {
    if (ptr != IntPtr.Zero)
    {
      Marshal.FreeHGlobal(ptr);
    }
  }

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_call")]
  private static extern IntPtr CedarCallNative(IntPtr operation, IntPtr input);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_policy")]
  private static extern IntPtr CedarParsePolicyNative(IntPtr policy);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policy_to_json")]
  private static extern IntPtr CedarPolicyToJsonNative(IntPtr policy);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policy_from_json")]
  private static extern IntPtr CedarPolicyFromJsonNative(IntPtr policyJson);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policy_set_to_json")]
  private static extern IntPtr CedarPolicySetToJsonNative(IntPtr policySet);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_policies")]
  private static extern IntPtr CedarParsePoliciesNative(IntPtr policies);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_get_policy_annotations")]
  private static extern IntPtr CedarGetPolicyAnnotationsNative(IntPtr policy);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_get_template_annotations")]
  private static extern IntPtr CedarGetTemplateAnnotationsNative(IntPtr template);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_policy_template")]
  private static extern IntPtr CedarParsePolicyTemplateNative(IntPtr template);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policy_effect")]
  private static extern IntPtr CedarPolicyEffectNative(IntPtr policy);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_template_effect")]
  private static extern IntPtr CedarTemplateEffectNative(IntPtr template);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_entity_identifier_repr")]
  private static extern IntPtr CedarEntityIdentifierReprNative(IntPtr id);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_entity_type_name")]
  private static extern IntPtr CedarParseEntityTypeNameNative(IntPtr typeName);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_entity_type_name_repr")]
  private static extern IntPtr CedarEntityTypeNameReprNative(IntPtr typeName);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_entity_uid")]
  private static extern IntPtr CedarParseEntityUidNative(IntPtr entityUid);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_euid_repr")]
  private static extern IntPtr CedarEuidReprNative(IntPtr typeName, IntPtr id);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_json_schema")]
  private static extern IntPtr CedarParseJsonSchemaNative(IntPtr schema);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_cedar_schema")]
  private static extern IntPtr CedarParseCedarSchemaNative(IntPtr schema);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policies_str_to_pretty")]
  private static extern IntPtr CedarPoliciesStrToPrettyNative(IntPtr policies);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policies_str_to_pretty_with_config")]
  private static extern IntPtr CedarPoliciesStrToPrettyWithConfigNative(IntPtr policies, UIntPtr lineWidth, IntPtr indentWidth);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_json_to_cedar_schema")]
  private static extern IntPtr CedarJsonToCedarSchemaNative(IntPtr jsonSchema);

  [DllImport(CedarNativeLibrary.Name, EntryPoint = "cedar_cedar_to_json_schema")]
  private static extern IntPtr CedarCedarToJsonSchemaNative(IntPtr cedarSchema);
#else
  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_free_string")]
  public static partial void FreeString(IntPtr value);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_version")]
  public static partial IntPtr CedarVersion();

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_call", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarCall(string operation, string input);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_policy", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarParsePolicy(string policy);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policy_to_json", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarPolicyToJson(string policy);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policy_from_json", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarPolicyFromJson(string policyJson);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policy_set_to_json", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarPolicySetToJson(string policySet);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_policies", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarParsePolicies(string policies);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_get_policy_annotations", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarGetPolicyAnnotations(string policy);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_get_template_annotations", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarGetTemplateAnnotations(string template);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_policy_template", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarParsePolicyTemplate(string template);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policy_effect", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarPolicyEffect(string policy);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_template_effect", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarTemplateEffect(string template);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_entity_identifier_repr", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarEntityIdentifierRepr(string id);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_entity_type_name", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarParseEntityTypeName(string typeName);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_entity_type_name_repr", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarEntityTypeNameRepr(string typeName);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_entity_uid", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarParseEntityUid(string entityUid);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_euid_repr", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarEuidRepr(string typeName, string id);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_json_schema", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarParseJsonSchema(string schema);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_parse_cedar_schema", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarParseCedarSchema(string schema);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policies_str_to_pretty", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarPoliciesStrToPretty(string policies);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_policies_str_to_pretty_with_config", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarPoliciesStrToPrettyWithConfig(string policies, nuint lineWidth, nint indentWidth);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_json_to_cedar_schema", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarJsonToCedarSchema(string jsonSchema);

  [LibraryImport(CedarNativeLibrary.Name, EntryPoint = "cedar_cedar_to_json_schema", StringMarshalling = StringMarshalling.Utf8)]
  public static partial IntPtr CedarCedarToJsonSchema(string cedarSchema);
#endif
}
