namespace Billiards.DataBase.Entities;

public class PlayerStats : Player
{
    public string PlayerName { get; set; } = null!;
    public int WinRate { get; set; }
    public int Balls { get; set; }
    public int AccidentalBalls { get; set; }
    public int FoulsBalls { get; set; }
    public int IsBreakShot { get; set; }
}