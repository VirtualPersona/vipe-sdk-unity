using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CAMenu : EditorWindow
{
    private const string ApiKeyEditorPrefKey = "CryptoAvatars_APIKey";
    private const string ExampleScenePath = "Assets/Scenes/NombreDeLaEscena.unity";
    private const string PrefabPath = "Assets/Ruta/Al/Prefab.prefab";
    private Texture2D _iconTexture;

    private string _apiKey;


    [MenuItem("Crypto Avatars/Settings")]
    public static void OpenWindow()
    {
        var window = GetWindow<CAMenu>("Settings");
        window.Show();
    }

    private void OnEnable()
    {
        LoadApiKey();
        _iconTexture = Resources.Load<Texture2D>("Visuals/UI/VIPE - Standard Yellow");
    }
    private void OnGUI()
    {
        DrawBanner();

        float bannerHeight = this.position.height * 0.2f;
        GUILayout.BeginArea(new Rect(0, bannerHeight, this.position.width, this.position.height - bannerHeight));

        RenderSettingsSection(10, 10, 10, 10);
        RenderButtonWithPadding("Get API KEY", () => Application.OpenURL("https://cryptoavatars.io/integrations"), 10, 10, 5, 5);
        RenderButtonWithPadding("Load Example Scene", () => EditorSceneManager.OpenScene(ExampleScenePath), 10, 10, 5, 5);
        RenderButtonWithPadding("Load Example Prefab", () => LoadAndInstantiatePrefab(), 10, 10, 5, 5);

        GUILayout.EndArea();
    }
    private void RenderButtonWithPadding(string buttonText, System.Action buttonAction, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
    {
        GUILayout.BeginVertical();
        GUILayout.Space(paddingTop);

        GUILayout.BeginHorizontal();
        GUILayout.Space(paddingLeft);

        if (GUILayout.Button(buttonText))
        {
            buttonAction();
        }

        GUILayout.Space(paddingRight);
        GUILayout.EndHorizontal();

        GUILayout.Space(paddingBottom);
        GUILayout.EndVertical();
    }
    private void DrawBanner()
    {
        float bannerHeight = this.position.height * 0.2f;
        float iconPaddingTop = bannerHeight * 0.1f;
        float iconPaddingLeft = 0.0f;

        Color backgroundColor = new Color(0.02f, 0.02f, 0.02f);
        Rect bannerRect = new Rect(0, 0, this.position.width, bannerHeight);
        GUI.color = backgroundColor;
        GUI.DrawTexture(bannerRect, EditorGUIUtility.whiteTexture);
        GUI.color = Color.white;

        if (_iconTexture != null)
        {
            float originalIconWidth = _iconTexture.width;
            float originalIconHeight = _iconTexture.height;

            float scale = bannerHeight * 0.8f / originalIconHeight;
            float scaledWidth = originalIconWidth * scale;
            float scaledHeight = originalIconHeight * scale;

            Rect iconRect = new Rect(iconPaddingLeft, iconPaddingTop, scaledWidth, scaledHeight);
            GUI.DrawTexture(iconRect, _iconTexture);
        }
    }
    private void RenderSettingsSection(float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
    {
        GUILayout.BeginVertical();
        GUILayout.Space(paddingTop);

        GUILayout.BeginHorizontal();
        GUILayout.Space(paddingLeft);

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        GUILayout.Space(paddingRight);
        GUILayout.EndHorizontal();

        RenderApiKeyField(paddingLeft, paddingRight, 0, 0);

        GUILayout.Space(paddingBottom);
        GUILayout.EndVertical();
    }
    private void RenderApiKeyField(float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
    {
        GUILayout.BeginVertical();
        GUILayout.Space(paddingTop);

        GUILayout.BeginHorizontal();
        GUILayout.Space(paddingLeft);

        string previousApiKey = _apiKey;
        _apiKey = EditorGUILayout.TextField("API Key:", _apiKey);

        if (_apiKey != previousApiKey)
        {
            SaveApiKey();
        }

        GUILayout.Space(paddingRight);
        GUILayout.EndHorizontal();

        GUILayout.Space(paddingBottom);
        GUILayout.EndVertical();
    }
    private void LoadAndInstantiatePrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        if (prefab != null)
        {
            PrefabUtility.InstantiatePrefab(prefab);
        }
        else
        {
            Debug.LogError("No se pudo cargar el prefab.");
        }
    }
    private void LoadApiKey()
    {
        _apiKey = EditorPrefs.GetString(ApiKeyEditorPrefKey, string.Empty);
    }
    private void SaveApiKey()
    {
        EditorPrefs.SetString(ApiKeyEditorPrefKey, _apiKey);
    }
}