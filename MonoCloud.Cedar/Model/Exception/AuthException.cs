namespace MonoCloud.Cedar.Model.Exception;

public class AuthException : System.Exception
{
  public AuthException(string message) : base(message)
  {
  }

  public AuthException(string message, System.Exception innerException) : base(message, innerException)
  {
  }
}
