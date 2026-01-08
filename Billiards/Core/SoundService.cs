using Billiards.Abstractions;
using Billiards.Enum;
using Plugin.Maui.Audio;

namespace Billiards.Core;

public sealed class SoundService(IAudioManager audioManager) : ISoundService
{
    private const string SoundsEnabledKey = "sounds_enabled";

    private static readonly Dictionary<SoundId, string> Map = new()
    {
        [SoundId.FreshMeat] = "fresh_meat.mp3",
        [SoundId.AccidentalPlus] = "accidental_plus.mp3",
        [SoundId.Fall] = "fall.mp3"
    };

    public async Task PlayAsync(SoundId id)
    {
        if (!Preferences.Default.Get(SoundsEnabledKey, false))
        {
            return;
        }

        if (!Map.TryGetValue(id, out var fileName))
        {
            return;
        }

        var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        var player = audioManager.CreatePlayer(stream);
        player.Volume = Math.Clamp(1.0, 0, 1);

        player.PlaybackEnded += (_, _) =>
        {
            player.Dispose();
            stream.Dispose();
        };

        player.Play();
    }
}