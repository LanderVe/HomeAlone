namespace HomeAlone;

internal class ApplicationSettings
{
    public string TargetIp { get; set; } = string.Empty;
    public ushort TargetPort { get; set; }
    public string JobsCsvPath { get; set; } = string.Empty;
}
