namespace Billiards.ModelAndDto;

public class PlayerStats
{
    public string PlayerName { get; set; } = null!;
    public int GamePlayed { get; set; }
    public double WinRate { get; set; }
    public int AccidentalBalls { get; set; }
    public int FoulsBalls { get; set; }
    public int BreakShot { get; set; }
}