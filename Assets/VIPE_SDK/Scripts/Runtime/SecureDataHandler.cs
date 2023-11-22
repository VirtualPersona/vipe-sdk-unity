using UnityEngine;
using System.IO;
namespace VIPE_SDK
{
    public static class SecureDataHandler
    {
        private static readonly string resourcePath = "APIKeyConfig";
        private static APIKeyConfig apiKeyConfig;

        public static APIKeyConfig ApiKeyConfig
        {
            get
            {
                if (apiKeyConfig == null)
                {
                    apiKeyConfig = Resources.Load<APIKeyConfig>(resourcePath);

#if UNITY_EDITOR
                    if (apiKeyConfig == null)
                    {
                        apiKeyConfig = ScriptableObject.CreateInstance<APIKeyConfig>();
                        string dirPath = Path.Combine(Application.dataPath, "Resources");
                        if (!Directory.Exists(dirPath))
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                        }
                        UnityEditor.AssetDatabase.CreateAsset(apiKeyConfig, "Assets/Resources/APIKeyConfig.asset");
                        UnityEditor.AssetDatabase.SaveAssets();
                    }
#endif
                }
                return apiKeyConfig;
            }
        }

        public static string LoadAPIKey()
        {
            return ApiKeyConfig.API_KEY;
        }

#if UNITY_EDITOR
        public static void SaveAPIKey(string apiKey)
        {
            ApiKeyConfig.API_KEY = apiKey;
            UnityEditor.EditorUtility.SetDirty(ApiKeyConfig);
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