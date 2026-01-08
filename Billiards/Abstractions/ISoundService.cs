using Billiards.Enum;

namespace Billiards.Abstractions;

public interface ISoundService
{
    Task PlayAsync(SoundId id);
}