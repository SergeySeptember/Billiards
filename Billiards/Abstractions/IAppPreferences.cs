namespace Billiards.Abstractions;

public interface IAppPreferences
{
    string GetString(string key, string defaultValue);
    bool GetBoolean(string key, bool defaultValue);
    void SetString(string key, string value);
    void SetBoolean(string key, bool value);
    bool Contains(string key);
}
