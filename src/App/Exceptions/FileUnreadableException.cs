namespace src.Exceptions;

public class FileUnreadableException : Exception
{
    public FileUnreadableException(string message)
        : base(message) { }
}
