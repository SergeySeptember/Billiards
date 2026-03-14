namespace Billiards.ModelAndDto;

public sealed class StatsRow
{
    public bool IsSummary { get; set; }
    public string MatchNo { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
    public string Loser { get; set; } = string.Empty;
    public string Score { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
}
