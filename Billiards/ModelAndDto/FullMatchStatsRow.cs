namespace Billiards.ModelAndDto;

public class FullMatchStatsRow
{
    public string MatchNo { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
    public string Loser { get; set; } = string.Empty;
    public string Score { get; set; } = string.Empty;
    public string Accidental { get; set; } = string.Empty;
    public string Fouls { get; set; } = string.Empty;
    public string BreakShot { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
}