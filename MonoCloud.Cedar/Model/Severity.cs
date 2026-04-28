namespace MonoCloud.Cedar.Model;

public enum Severity
{
  [EnumMember(Value = "advice")]
  Advice,
  [EnumMember(Value = "warning")]
  Warning,
  [EnumMember(Value = "error")]
  Error
}
