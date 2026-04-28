namespace MonoCloud.Cedar.Interop;

internal static partial class CedarFfi
{
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
}
