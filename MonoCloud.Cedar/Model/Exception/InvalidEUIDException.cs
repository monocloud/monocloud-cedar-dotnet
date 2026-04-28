namespace MonoCloud.Cedar.Model.Exception;

public sealed class InvalidEUIDException(string message) : ArgumentException(message);
