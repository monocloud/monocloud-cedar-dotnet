namespace MonoCloud.Cedar.Model.Exception;

public class InternalException : AuthException
{
  public InternalException(params string[] errors) : base(string.Join(Environment.NewLine, errors))
  {
    Errors = errors;
  }

  public InternalException(IEnumerable<string> errors) : this(errors.ToArray())
  {
  }

  public IReadOnlyList<string> Errors { get; }
}
