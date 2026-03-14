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
        [SoundId.BreakShot] = "breakShot.mp3",
        [SoundId.Start] = "start.mp3"
    };

    private static readonly string[] StartButtonSounds =
    [
        "start.mp3"
    ];

    private static readonly string[] MainBallsIncrementSounds =
    [
        "shot.mp3"
    ];

    private static readonly string[] AccidentalIncrementSounds =
    [
        "sorry.mp3",
        "accidental_plus.mp3"
    ];

    private static readonly string[] FoulsIncrementSounds =
    [
        "fall.mp3",
        "mepo.mp3"
    ];

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

        await PlayFileAsync(fileName);
    }

    public Task PlayStartButtonAsync() => PlayRandomAsync(StartButtonSounds);

    public Task PlayMainBallsIncrementAsync() => PlayRandomAsync(MainBallsIncrementSounds);

    public Task PlayAccidentalIncrementAsync() => PlayRandomAsync(AccidentalIncrementSounds);

    public Task PlayFoulsIncrementAsync() => PlayRandomAsync(FoulsIncrementSounds);

    private async Task PlayRandomAsync(IReadOnlyList<string> fileNames)
    {
        if (!appPreferences.GetBoolean(Const.SoundsKey, false) || fileNames.Count == 0)
        {
            return;
        }

        var fileName = fileNames[Random.Shared.Next(fileNames.Count)];
        await PlayFileAsync(fileName);
    }

    private async Task PlayFileAsync(string fileName)
    {
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
