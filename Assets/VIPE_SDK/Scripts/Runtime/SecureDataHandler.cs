using UnityEngine;
using System.IO;
namespace VIPE_SDK
{
    public static class SecureDataHandler
    {
        private static readonly string resourcePath = "APIKeyConfig";
        private static APIKeyConfig _apiKeyConfig;

        public static APIKeyConfig apiKeyConfig
        {
            get
            {
                if (_apiKeyConfig == null)
                {
                    _apiKeyConfig = Resources.Load<APIKeyConfig>(resourcePath);

#if UNITY_EDITOR
                    if (_apiKeyConfig == null)
                    {
                        _apiKeyConfig = ScriptableObject.CreateInstance<APIKeyConfig>();
                        string dirPath = Path.Combine(Application.dataPath, "Resources");
                        if (!Directory.Exists(dirPath))
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                        }
                        UnityEditor.AssetDatabase.CreateAsset(_apiKeyConfig, "Assets/Resources/APIKeyConfig.asset");
                        UnityEditor.AssetDatabase.SaveAssets();
                    }
#endif
                }
                return _apiKeyConfig;
            }
        }

        public static string LoadAPIKey()
        {
            return apiKeyConfig.API_KEY;
        }

#if UNITY_EDITOR
        public static void SaveAPIKey(string apiKey)
        {
            apiKeyConfig.API_KEY = apiKey;
            UnityEditor.EditorUtility.SetDirty(apiKeyConfig);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif

        private const string WALLET_PREF = "WALLET";

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
}