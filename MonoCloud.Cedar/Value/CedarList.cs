namespace MonoCloud.Cedar.Value;

public sealed class CedarList : Value, IList<Value>
{
  private readonly List<Value> list;

  public CedarList() => list = [];

  public CedarList(IEnumerable<Value> source) => list = [.. source];

  public Value this[int index]
  {
    get => list[index];
    set => list[index] = value ?? throw new ArgumentNullException(nameof(value));
  }

  public int Count => list.Count;

  public bool IsReadOnly => false;

  public void Add(Value item) => list.Add(item ?? throw new ArgumentNullException(nameof(item)));

  public void Clear() => list.Clear();

  public bool Contains(Value item) => list.Contains(item);

  public void CopyTo(Value[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

  public IEnumerator<Value> GetEnumerator() => list.GetEnumerator();

  public int IndexOf(Value item) => list.IndexOf(item);

  public void Insert(int index, Value item) => list.Insert(index, item ?? throw new ArgumentNullException(nameof(item)));

  public bool Remove(Value item) => list.Remove(item);

  public void RemoveAt(int index) => list.RemoveAt(index);

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public override string ToString() => $"[{string.Join(", ", list)}]";

  public override string ToCedarExpr() => $"[{string.Join(", ", list.Select(x => x.ToCedarExpr()))}]";

  public override bool Equals(object? obj) => obj is CedarList other && list.SequenceEqual(other.list);

  public override int GetHashCode()
  {
    var hash = new HashCode();
    foreach (var item in list)
    {
      hash.Add(item);
    }

    return hash.ToHashCode();
  }
}
