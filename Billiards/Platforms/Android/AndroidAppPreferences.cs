using Android.Content;
using Billiards.Abstractions;

namespace Billiards.Platforms.Android;

public sealed class AndroidAppPreferences : IAppPreferences
{
    private const string PreferencesName = "billiards_preferences";

    private readonly ISharedPreferences _preferences =
        global::Android.App.Application.Context.GetSharedPreferences(PreferencesName, FileCreationMode.Private)!;

    public string GetString(string key, string defaultValue) =>
        _preferences.GetString(key, defaultValue) ?? defaultValue;

    public bool GetBoolean(string key, bool defaultValue) =>
        _preferences.GetBoolean(key, defaultValue);

    public void SetString(string key, string value)
    {
        var editor = _preferences.Edit();
        if (editor is null)
        {
            return;
        }

        editor.PutString(key, value);
        editor.Apply();
    }

    public void SetBoolean(string key, bool value)
    {
        var editor = _preferences.Edit();
        if (editor is null)
        {
            return;
        }

        editor.PutBoolean(key, value);
        editor.Apply();
    }

    public bool Contains(string key) => _preferences.Contains(key);
}
