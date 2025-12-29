namespace Billiards.Core.Entities.DB;

public class Player
{
    public string Name { get; set; } = null!;
}

public class PlayerStats : Player
{
    public int Balls { get; set; }
    public int AccidentalBalls { get; set; }
    public int FoulsBalls { get; set; }
    public int IsBreakShot { get; set; }
}

public class MatchStats
{
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