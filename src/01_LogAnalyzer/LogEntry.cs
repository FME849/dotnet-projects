public enum LogLevel
{
    Info,
    Debug,
    Error,
    Warn
}
public record LogEntry(DateTimeOffset Timestamp, LogLevel Level, string Message)
{
    public static LogEntry Parse(string line)
    {
        string[] parts = line.Split(' ', 3);
        if (parts.Length < 3)
            throw new ArgumentException("Invalid log format.", nameof(line));

        // Get Timestamp
        DateTimeOffset timestamp = DateTimeOffset.Parse(parts[0][1..^1]);

        // Get Log Level
        LogLevel level = Enum.Parse<LogLevel>(parts[1][..^1], true);

        var record = new LogEntry(timestamp, level, parts[2]);
        return record;
    }
};