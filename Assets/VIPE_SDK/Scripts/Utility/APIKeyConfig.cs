using UnityEngine;

namespace VIPE_SDK
{
    public class APIKeyConfig : ScriptableObject
    {
        public string API_KEY;

#if UNITY_EDITOR
        public void SaveAPIKey(string apiKey)
        {
            API_KEY = apiKey;
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}