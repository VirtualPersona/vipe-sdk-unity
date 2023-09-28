using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CAMenu : EditorWindow
{
    private const string ApiKeyEditorPrefKey = "API_KEY";
    private const string WalletEditorPrefKey = "WALLET";
    private const string ExampleScenePath = "Assets/Scenes/NombreDeLaEscena.unity";
    private const string PrefabPath = "Assets/Ruta/Al/Prefab.prefab";
    private Texture2D _iconTexture;
    private string _apiKey;

    private const string LOGIN_URL = "https://testnet.vipe.io/connect?integrationLogin=true";
    private string WalletAddress;
    private bool messageObtained = false;
    public bool isLogedIn = false;

    [MenuItem("VIPE/Settings")]
    public static void OpenWindow()
    {
        var window = GetWindow<CAMenu>("Settings");
        window.Show();
    }

    private void OnEnable()
    {
        LoadApiKey();
        LoadWallet();
        _iconTexture = Resources.Load<Texture2D>("Visuals/UI/Icons/Vipe_Logo_v3");
        Debug.Log(_iconTexture);
    }
    private void OnGUI()
    {
        DrawBanner();

        float bannerHeight = this.position.height * 0.2f;
        GUILayout.BeginArea(new Rect(0, bannerHeight, this.position.width, this.position.height - bannerHeight));

        RenderSettingsSection(10, 10, 10, 10);
        RenderLabelWithPadding("Wallet Address:", WalletAddress, 10, 10, 10, 10);
        RenderButtonWithPadding("Login to VIPE", OnLoginButtonClick, 10, 10, 5, 5);
        RenderButtonWithPadding("Get API KEY", () => Application.OpenURL("https://cryptoavatars.io/integrations"), 10, 10, 5, 5);
        RenderButtonWithPadding("Load Example Scene", () => EditorSceneManager.OpenScene(ExampleScenePath), 10, 10, 5, 5);
        RenderButtonWithPadding("Load Example Prefab", () => LoadAndInstantiatePrefab(), 10, 10, 5, 5);

        GUILayout.EndArea();
    }
    private void OnLoginButtonClick()
    {
        if (!isLogedIn)
        {
            Application.OpenURL(LOGIN_URL);
            StartGetClipboardMessage();
        }
    }
    private void StartGetClipboardMessage()
    {
        EditorApplication.update += GetClipboardMessage;
    }
    private void GetClipboardMessage()
    {
        string copiedMessage = EditorGUIUtility.systemCopyBuffer;

        if (!string.IsNullOrEmpty(copiedMessage) && copiedMessage.StartsWith("0x"))
        {
            messageObtained = true;
            ProcessMessage(copiedMessage);
            EditorGUIUtility.systemCopyBuffer = string.Empty;

            EditorApplication.update -= GetClipboardMessage; // Stop checking
            OnLoginEnd();
        }
    }
    private void ProcessMessage(string message)
    {
        Debug.Log("Message: " + message);
        WalletAddress = message;
    }
    private void OnLoginEnd()
    {
        if (!isLogedIn && messageObtained)
        {
            Debug.Log("Login process completed successfully.");
            isLogedIn = true;
            SecureDataHandler.SaveWallet(WalletAddress);
        }
        else
            Debug.LogWarning("Login process completed without obtaining a valid message.");
    }
    private void RenderLabelWithPadding(string labelText, string value, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
    {
        GUILayout.BeginVertical();
        GUILayout.Space(paddingTop);

        GUILayout.BeginHorizontal();
        GUILayout.Space(paddingLeft);

        EditorGUILayout.LabelField(labelText, value);

        GUILayout.Space(paddingRight);
        GUILayout.EndHorizontal();

        GUILayout.Space(paddingBottom);
        GUILayout.EndVertical();
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
    private void RenderSettingsSection(float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
    {
        GUILayout.BeginVertical();
        GUILayout.Space(paddingTop);

        GUILayout.BeginHorizontal();
        GUILayout.Space(paddingLeft);

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        GUILayout.Space(paddingRight);
        GUILayout.EndHorizontal();

        RenderApiKeyMessage(paddingLeft, paddingRight, 0, 0);
        RenderApiKeyField(paddingLeft, paddingRight, 0, 0);

        GUILayout.Space(paddingBottom);
        GUILayout.EndVertical();
    }
    private void RenderApiKeyMessage(float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            GUILayout.BeginVertical();
            GUILayout.Space(paddingTop);

            GUILayout.BeginHorizontal();
            GUILayout.Space(paddingLeft);

            EditorGUILayout.HelpBox("Please enter an API key.", MessageType.Info);

            GUILayout.Space(paddingRight);
            GUILayout.EndHorizontal();

            GUILayout.Space(paddingBottom);
            GUILayout.EndVertical();
        }
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
            SecureDataHandler.SaveAPIKey(_apiKey);
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
    private void DrawBanner()
    {
        float bannerHeight = this.position.height * 0.2f;
        float iconPaddingTop = (bannerHeight - _iconTexture.height * 0.2f) * 0.5f;
        float iconPaddingLeft = 10.0f;

        Color backgroundColor = new Color(0.02f, 0.02f, 0.02f);
        Rect bannerRect = new Rect(0, 0, this.position.width, bannerHeight);
        GUI.color = backgroundColor;
        GUI.DrawTexture(bannerRect, EditorGUIUtility.whiteTexture);
        GUI.color = Color.white;

        if (_iconTexture != null)
        {
            float originalIconWidth = _iconTexture.width;
            float originalIconHeight = _iconTexture.height;

            float scale = bannerHeight * 0.5f / originalIconHeight;
            float scaledWidth = originalIconWidth * scale;
            float scaledHeight = originalIconHeight * scale;

            Rect iconRect = new Rect(iconPaddingLeft, iconPaddingTop, scaledWidth, scaledHeight);
            GUI.DrawTexture(iconRect, _iconTexture);
        }
    }
    private void LoadApiKey()
    {
        _apiKey = SecureDataHandler.LoadAPIKey();
    }
    private void LoadWallet()
    {
        WalletAddress = SecureDataHandler.LoadWallet();
    }
}