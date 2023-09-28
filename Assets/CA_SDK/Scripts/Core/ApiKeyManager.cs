using UnityEditor;

public static class ApiKeyManager
{
    private const string ApiKeyEditorPrefKey = "CryptoAvatars_APIKey";
    public static string GetApiKey()
    {
        return EditorPrefs.GetString(ApiKeyEditorPrefKey, string.Empty);
    }
    public static void SetApiKey(string apiKey)
    {
        EditorPrefs.SetString(ApiKeyEditorPrefKey, apiKey);
    }
}