namespace MonoCloud.Cedar.Model.Formatter;

public sealed class Config(int lineWidth, int indentWidth)
{
  public int GetLineWidth() => lineWidth;

  public int GetIndentWidth() => indentWidth;
}
