using System.Text.Json;

string[] lines = await File.ReadAllLinesAsync("app.log");

List<LogEntry> logEntries = lines
                            .Where(line => !string.IsNullOrEmpty(line))
                            .Select(line => LogEntry.Parse(line))
                            .ToList();

List<LogEntry> logError = logEntries.Where(log => log.Level == LogLevel.Error).ToList();
var errorStats = logError.GroupBy(log => log.Message).Select(g => new { ErrorMessage = g.Key, Count = g.Count() }).ToList();
var jsonOption = new JsonSerializerOptions
{
    WriteIndented = true
};
var errorStatsJson = JsonSerializer.Serialize(errorStats, jsonOption);

await File.WriteAllTextAsync("report.json", errorStatsJson);
