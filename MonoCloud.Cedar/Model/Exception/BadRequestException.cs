namespace MonoCloud.Cedar.Model.Exception;

public class BadRequestException : AuthException
{
  public BadRequestException(params string[] errors) : base(string.Join(Environment.NewLine, errors))
  {
    Errors = errors;
  }

  public IReadOnlyList<string> Errors { get; }
}
