namespace MonoCloud.Cedar.Model.Policy;

public sealed class LinkValue(string slot, EntityUID value)
{
  public string GetSlot() => slot;

  public EntityUID GetValue() => value;
}
