namespace MonoCloud.Cedar.Value;

public sealed class CedarMap : Value, IDictionary<string, Value>
{
  private readonly Dictionary<string, Value> map;

  public CedarMap() => map = [];

  public CedarMap(IDictionary<string, Value> source) => map = new(source);

  public Value this[string key]
  {
    get => map[key];
    set => map[key] = value ?? throw new ArgumentNullException(nameof(value));
  }

  public ICollection<string> Keys => map.Keys;

  public ICollection<Value> Values => map.Values;

  public int Count => map.Count;

  public bool IsReadOnly => false;

  public void Add(string key, Value value) => map.Add(key ?? throw new ArgumentNullException(nameof(key)), value ?? throw new ArgumentNullException(nameof(value)));

  public void Add(KeyValuePair<string, Value> item) => Add(item.Key, item.Value);

  public void Clear() => map.Clear();

  public bool Contains(KeyValuePair<string, Value> item) => ((IDictionary<string, Value>)map).Contains(item);

  public bool ContainsKey(string key) => map.ContainsKey(key);

  public void CopyTo(KeyValuePair<string, Value>[] array, int arrayIndex) => ((IDictionary<string, Value>)map).CopyTo(array, arrayIndex);

  public IEnumerator<KeyValuePair<string, Value>> GetEnumerator() => map.GetEnumerator();

  public bool Remove(string key) => map.Remove(key);

  public bool Remove(KeyValuePair<string, Value> item) => ((IDictionary<string, Value>)map).Remove(item);

  public bool TryGetValue(string key, out Value value) => map.TryGetValue(key, out value!);

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public override string ToString() => $"{{{string.Join(", ", map.Select(x => $"{x.Key}={x.Value}"))}}}";

  public override string ToCedarExpr() => "{" + string.Join(", ", map.Select(x => $"\"{x.Key}\": {x.Value.ToCedarExpr()}")) + "}";

  public override bool Equals(object? obj) =>
    obj is CedarMap other && map.Count == other.map.Count && map.All(x => other.map.TryGetValue(x.Key, out var value) && Equals(x.Value, value));

  public override int GetHashCode()
  {
#if NETSTANDARD2_0
    var hash = 17;
    foreach (var item in map.OrderBy(x => x.Key, StringComparer.Ordinal))
    {
      hash = hash * 31 + item.Key.GetHashCode();
      hash = hash * 31 + item.Value.GetHashCode();
    }

    return hash;
#else
    var hash = new HashCode();
    foreach (var item in map.OrderBy(x => x.Key, StringComparer.Ordinal))
    {
      hash.Add(item.Key);
      hash.Add(item.Value);
    }

    return hash.ToHashCode();
#endif
  }
}
