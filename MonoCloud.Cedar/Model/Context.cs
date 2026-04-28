namespace MonoCloud.Cedar.Model;

public sealed class Context
{
  private readonly Dictionary<string, Value.Value> context;

  public Context() => context = [];

  public Context(IDictionary<string, Value.Value> contextMap) => context = new(contextMap);

  public Context(IEnumerable<KeyValuePair<string, Value.Value>> contextList)
  {
    context = [];
    Merge(contextList);
  }

  public bool IsEmpty() => context.Count == 0;

  public Dictionary<string, Value.Value> GetContext() => new(context);

  public void Merge(Context contextToMerge) => Merge(contextToMerge.GetContext());

  public void Merge(IEnumerable<KeyValuePair<string, Value.Value>> contextMaps)
  {
    foreach (var item in contextMaps)
    {
      if (context.ContainsKey(item.Key))
      {
        throw new InvalidOperationException($"Duplicate key '{item.Key}' in existing context");
      }

      context[item.Key] = item.Value;
    }
  }

  public Value.Value? Get(string key) =>
#if NETSTANDARD2_0
    context.TryGetValue(key, out var value) ? value : null;
#else
    context.GetValueOrDefault(key);
#endif

  public override string ToString() => $"{{{string.Join(", ", context.Select(x => $"{x.Key}={x.Value}"))}}}";
}
