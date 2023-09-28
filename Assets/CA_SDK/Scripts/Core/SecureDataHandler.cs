using UnityEditor;
using UnityEngine;

public static class SecureDataHandler
{
    private const string API_KEY_PREF = "API_KEY";
    private const string WALLET_PREF = "WALLET";

    public static void SaveAPIKey(string apiKey)
    {
        PlayerPrefs.SetString(API_KEY_PREF, apiKey);
        PlayerPrefs.Save();
    }

    public static string LoadAPIKey()
    {
        return PlayerPrefs.GetString(API_KEY_PREF, "");
    }

    public static void SaveWallet(string wallet)
    {
        PlayerPrefs.SetString(WALLET_PREF, wallet);
        PlayerPrefs.Save();
    }

    public static string LoadWallet()
    {
        return PlayerPrefs.GetString(WALLET_PREF, "");
    }
}