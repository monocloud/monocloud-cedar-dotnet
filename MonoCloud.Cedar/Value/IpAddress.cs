namespace MonoCloud.Cedar.Value;

public sealed class IpAddress : Value
{
  private readonly string ipAddress;

  public IpAddress(string ipAddress)
  {
    if (string.IsNullOrWhiteSpace(ipAddress))
    {
      throw new ArgumentException("Input string is not a valid IPv4 or IPv6 address", nameof(ipAddress));
    }

    var addressPart = ipAddress.Split('/')[0];
    if (!IPAddress.TryParse(addressPart, out _))
    {
      throw new ArgumentException("Input string is not a valid IPv4 or IPv6 address", nameof(ipAddress));
    }

    this.ipAddress = ipAddress;
  }

  public override string ToCedarExpr() => $"ip(\"{ipAddress}\")";

  public override string ToString() => ipAddress;

  public override bool Equals(object? obj) => obj is IpAddress other && ipAddress == other.ipAddress;

  public override int GetHashCode() => ipAddress.GetHashCode();
}
