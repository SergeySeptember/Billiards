using Billiards.Abstractions;
using Billiards.Enum;
using Billiards.Utils;
using Plugin.Maui.Audio;

namespace Billiards.Core;

public sealed class SoundService(IAudioManager audioManager, IAppPreferences appPreferences) : ISoundService
{
    private static readonly Dictionary<SoundId, string> Map = new()
    {
        [SoundId.FreshMeat] = "fresh_meat.mp3",
        [SoundId.AccidentalPlus] = "sorry.mp3",
        [SoundId.Fall] = "fall.mp3",
        [SoundId.Shot] = "shot.mp3",
        [SoundId.Start] = "start.mp3"
    };

    public async Task PlayAsync(SoundId id)
    {
        if (!appPreferences.GetBoolean(Const.SoundsKey, false))
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
