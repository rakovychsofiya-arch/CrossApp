// src/Core/Services/INotificationSink.cs
public interface INotificationSink
{
    void Notify(string message);
}

// src/Cli/Sinks/ConsoleSink.cs
public sealed class ConsoleSink : INotificationSink
{
    public void Notify(string message)
    {
        Console.WriteLine(message);
    }
}

// src/Cli/Sinks/FileSink.cs
public sealed class FileSink : INotificationSink
{
    private readonly string _filePath;

    public FileSink(string filePath)
    {
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
    }

    public void Notify(string message)
    {
        File.AppendAllText(_filePath, message + Environment.NewLine);
    }
}