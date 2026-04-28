namespace MonoCloud.Cedar.Value;

[JsonConverter(typeof(ValueJsonConverter))]
public abstract class Value
{
  public abstract string ToCedarExpr();
}
