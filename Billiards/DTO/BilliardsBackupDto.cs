namespace Billiards.DTO;

public sealed class BilliardsBackupDto
{
    public DateTime Exported { get; set; } = DateTime.Now;

    public List<PlayerDto> Players { get; set; } = new();
    public List<MatchStatsDto> Matches { get; set; } = new();
}

public sealed class PlayerDto
{
    public string Name { get; set; } = string.Empty;
}

public sealed class MatchStatsDto
{
    public DateTime CurrentDateTime { get; set; }
    public string GameTypes { get; set; } = string.Empty;

    public string WinnerPlayer { get; set; } = string.Empty;
    public string LosePlayer { get; set; } = string.Empty;

    public string MatchTime { get; set; } = string.Empty;

    public int BallsWinnerPlayer { get; set; }
    public int BallsLosePlayer { get; set; }

    public int AccidentalBallsWinnerPlayer { get; set; }
    public int AccidentalBallsLosePlayer { get; set; }

    public int FoulsBallsWinnerPlayer { get; set; }
    public int FoulsBallsLosePlayer { get; set; }

    public string? BreakShotPlayer { get; set; }
}