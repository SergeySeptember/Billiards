using Billiards.Enum;

namespace Billiards.Abstractions;

public interface ISoundService
{
    Task PlayAsync(SoundId id);
    Task PlayStartButtonAsync();
    Task PlayMainBallsIncrementAsync();
    Task PlayAccidentalIncrementAsync();
    Task PlayFoulsIncrementAsync();
}