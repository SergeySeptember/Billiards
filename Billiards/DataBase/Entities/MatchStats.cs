namespace Billiards.DataBase.Entities;

public class MatchStats
{
    public long Id { get; set; }
    public DateTime CurrentDateTime { get; set; }
    public string GameTypes { get; set; } = null!;
    public string WinnerPlayer { get; set; } = null!;
    public string LosePlayer { get; set; } = null!;
    public string MatchTime { get; set; } = null!;
    public int BallsWinnerPlayer { get; set; }
    public int BallsLosePlayer { get; set; }
    public int AccidentalBallsWinnerPlayer { get; set; }
    public int AccidentalBallsLosePlayer { get; set; }
    public int FoulsBallsWinnerPlayer { get; set; }
    public int FoulsBallsLosePlayer { get; set; }
    public string? BreakShotPlayer { get; set; }
}