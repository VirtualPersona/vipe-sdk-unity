using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VIPE_SDK
{
    public class VipeMenu : EditorWindow
    {
        private Texture2D _iconTexture;
        private string _apiKey;

        private const string LOGIN_URL   = "https://vipe.io/connect?integrationLogin=true";
        private const string API_KEY     = "https://docs.vipe.io/reference/intro/getting-started";
        private const string Twitter_URL = "https://twitter.com/vipeio";
        private const string Discord_URL = "https://discord.com/invite/vipeio";
        private const string WEB_URL     = "https://vipe.io/";
        private const string POST_URL    = "https://api.cryptoavatars.io/v1/login/vipe";

        private string WalletAddress;
        private string Signature;
        private bool messageObtained = false;
        public bool isLogedIn = false;

        [MenuItem("Tools/VIPE/Settings")]
        public static void OpenWindow()
        {
            var window = GetWindow<VipeMenu>("Settings");
            window.Show();
        }

        private void OnEnable()
        {
            LoadApiKey();
            LoadWallet();
            _iconTexture = Resources.Load<Texture2D>("Visuals/UI/Icons/Vipe_Logo_v3");
        }
        private void OnGUI()
        {
            // Draw banner at the top
            DrawBanner();

            // Define area below the banner
            float bannerHeight = this.position.height * 0.2f;
            GUILayout.BeginArea(new Rect(0, bannerHeight, this.position.width, this.position.height - bannerHeight));

            // Render settings section
            RenderSettingsSection(10, 10, 10, 0);

            // Render API key button
            RenderButtonWithPadding("Get API KEY", () => Application.OpenURL(API_KEY), 10, 10, 0, 5);

            // Draw separator
            DrawSeparator(10, 20, 20, 2);

            // Wallet connection settings
            RenderLabelWithPadding("Wallet connect setting menu", EditorStyles.boldLabel, 10, 10, 0, 0, 16);
            RenderCustomAlertMessage("Log in to see your owned avatars.", WalletAddress, 10, 10, 10, 0);
            RenderLabelWithPadding("Wallet Address:", WalletAddress, 10, 10, 0, 0);

            // Login/Logout buttons
            if (isLogedIn)
            {
                RenderButtonWithPadding("Log out from VIPE", OnLogOffButtonClick, 10, 10, 5, 0);
            }
            else
            {
                RenderButtonWithPadding("Login to VIPE", OnLoginButtonClick, 10, 10, 5, 0);
            }

            // Draw another separator
            DrawSeparator(10, 20, 20, 2);

            // Render settings menu
            RenderLabelWithPadding("Render setting menu", EditorStyles.boldLabel, 10, 10, 0, 0, 16);
            RenderLabelWithPadding("Project Render Pipeline:", IsProjectUsingURP() ? "URP" : "Built-in", 10, 10, 5, 0);
            RenderLabelWithPadding("VRM Configuration:", GetScriptConfiguration("MaterialFactory"), 10, 10, 0, 5);

            // Pipeline configuration buttons
            RenderButtonWithPadding("Set to URP", SetMode("MaterialFactory", "Material_URP"), 10, 10, 5, 0);
            RenderButtonWithPadding("Set to Built-in", SetMode("MaterialFactory", "Material_Built"), 10, 10, 0, 5);

            GUILayout.Space(50);
            // Draw separator
            DrawSeparator(10, 20, 10, 2);

            Community();
            // End of the defined area
            GUILayout.EndArea();
        }
        public void Community()
        {
            RenderLabelWithPadding("Community", EditorStyles.boldLabel, 10, 10, 0, 0, 15);
            GUILayout.BeginHorizontal();
            RenderButtonWithPadding("VIPE WEB",     () => Application.OpenURL(WEB_URL), 10, 0, 0, 5);
            RenderButtonWithPadding("Twitter",  () => Application.OpenURL(Twitter_URL), 0, 0, 0, 5);
            RenderButtonWithPadding("Discord",  () => Application.OpenURL(Discord_URL), 0, 0, 0, 5);
            GUILayout.EndHorizontal();
        }
        void DrawSeparator(int marginSide, int marginTop, int marginBot, int Height)
        {
            GUILayout.Space(marginTop);
            Texture2D whiteTexture = new Texture2D(Height, Height);
            whiteTexture.SetPixel(0, 0, Color.black);
            whiteTexture.Apply();

            GUIStyle separatorStyle = new GUIStyle();
            separatorStyle.normal.background = whiteTexture;

            GUILayout.BeginHorizontal();
            GUILayout.Space(marginSide);


            GUILayout.Box("", separatorStyle, GUILayout.Height(Height), GUILayout.ExpandWidth(true));

            GUILayout.Space(marginSide);
            GUILayout.EndHorizontal();


            GUILayout.Space(marginBot);
        }
        private bool IsProjectUsingURP()
        {
            UnityEngine.Rendering.RenderPipelineAsset currentSRP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            return currentSRP != null;
        }
        private string GetScriptConfiguration(string scriptName)
        {
            string scriptPath = FindScriptPath(scriptName);
            if (string.IsNullOrEmpty(scriptPath))
            {
                return "Unknown";
            }

            string scriptContent = File.ReadAllText(scriptPath);

            // Buscar las cadenas específicas en el contenido del script.
            if (scriptContent.Contains("//URP Configured"))
            {
                return "URP";
            }
            else if (scriptContent.Contains("//Built-in Configured"))
            {
                return "Built-in";
            }
            else
            {
                return "Unknown";
            }
        }
        private static string FindScriptPath(string scriptName)
        {
            string[] guids = AssetDatabase.FindAssets(scriptName + " t:Script");
            if (guids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }
            return null;
        }
        private static void OverwriteScriptContent(string targetScriptName, string sourceScriptPath)
        {
            string targetScriptPath = FindScriptPath(targetScriptName);
            if (string.IsNullOrEmpty(targetScriptPath))
            {
                Debug.LogError($"No se encontró el script {targetScriptName}.");
                return;
            }

            string newContent = File.ReadAllText(sourceScriptPath);

            newContent = newContent.Replace("/*", "").Replace("*/", "");

            File.WriteAllText(targetScriptPath, newContent);

            AssetDatabase.Refresh();
        }

        public Action SetMode(string targetScriptName, string sourceScriptName)
        {
            return () =>
            {
                string sourceScriptPath = FindScriptPath(sourceScriptName);
                if (string.IsNullOrEmpty(sourceScriptPath))
                {
                    Debug.LogError($"No se encontró el script {sourceScriptName}.");
                    return;
                }
                OverwriteScriptContent(targetScriptName, sourceScriptPath);
                Debug.Log($"El script {targetScriptName} ha sido sobrescrito con el contenido de {sourceScriptName}.");
            };
        }
        private void OnLoginButtonClick()
        {
            if (!isLogedIn)
            {
                Application.OpenURL(LOGIN_URL);
                StartGetClipboardMessage();
            }
        }
        private void OnLogOffButtonClick()
        {
            if (isLogedIn)
            {
                WalletAddress = string.Empty;
                SecureDataHandler.SaveWallet(WalletAddress);
                isLogedIn = false;
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

                EditorApplication.update -= GetClipboardMessage;
                OnLoginEnd();
            }
        }
        private void ProcessMessage(string message)
        {
            string[] parts = message.Split(';');
            if (parts.Length == 2)
            {
                WalletAddress = parts[0];
                Signature = parts[1];
            }
            else
            {
                Debug.LogWarning("Message is not in the expected format. Expected Wallet;Signature");
            }
        }
        private void OnLoginEnd()
        {
            if (!isLogedIn && messageObtained)
            {
                Debug.Log("Login process completed successfully.");
                Task.Run(SendLoginData);
                isLogedIn = true;
                SecureDataHandler.SaveWallet(WalletAddress);
            }
            else
            {
                Debug.LogWarning("Login process completed without obtaining a valid message.");
            }
        }
        private async Task SendLoginData()
        {
            if (Signature == null || WalletAddress == null)
            {
                Debug.LogWarning("Signature or WalletAddress is null");
                return;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                var formData = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("wallet", WalletAddress),
                new KeyValuePair<string, string>("signature", Signature),
                });

                HttpResponseMessage response = await httpClient.PostAsync(POST_URL, formData);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Debug.Log("Login successful: " + content);
                }
                else
                {
                    Debug.LogWarning("Failed to log in, HTTP status: " + response.StatusCode);
                    isLogedIn = false;
                }
            }
        }
        private void RenderLabelWithPadding     (string labelText, string value, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
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
        private void RenderLabelWithPadding     (string labelText, GUIStyle textStyle, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom, int fontSize = -1)
        {
            GUIStyle modifiedTextStyle = new GUIStyle(textStyle);

            if (fontSize > 0)
            {
                modifiedTextStyle.fontSize = fontSize;
            }

            GUILayout.BeginVertical();
            GUILayout.Space(paddingTop);

            GUILayout.BeginHorizontal();
            GUILayout.Space(paddingLeft);

            EditorGUILayout.LabelField(labelText, modifiedTextStyle);

            GUILayout.Space(paddingRight);
            GUILayout.EndHorizontal();

            GUILayout.Space(paddingBottom);
            GUILayout.EndVertical();
        }
        private void RenderButtonWithPadding    (string buttonText, System.Action buttonAction, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
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
        private void RenderSettingsSection      (float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(paddingTop);

            GUILayout.BeginHorizontal();
            GUILayout.Space(paddingLeft);

            RenderLabelWithPadding("API Key setting menu", EditorStyles.boldLabel, 0, 0, 0, 5, 16);

            GUILayout.Space(paddingRight);
            GUILayout.EndHorizontal();

            RenderCustomAlertMessage("Please enter an API key.", _apiKey, paddingLeft, paddingRight, 0, 0);
            RenderApiKeyField(paddingLeft, paddingRight, 0, 0);

            GUILayout.Space(paddingBottom);
            GUILayout.EndVertical();
        }
        private void RenderCustomAlertMessage   (string message, string value, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
        {
            if (string.IsNullOrEmpty(value))
            {
                GUILayout.BeginVertical();
                GUILayout.Space(paddingTop);

                GUILayout.BeginHorizontal();
                GUILayout.Space(paddingLeft);

                EditorGUILayout.HelpBox(message, MessageType.Info);

                GUILayout.Space(paddingRight);
                GUILayout.EndHorizontal();

                GUILayout.Space(paddingBottom);
                GUILayout.EndVertical();
            }
        }
        private void RenderApiKeyField          (float paddingLeft, float paddingRight, float paddingTop, float paddingBottom)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(paddingTop);

            GUILayout.BeginHorizontal();
            GUILayout.Space(paddingLeft);

            string previousApiKey = _apiKey;
            _apiKey = EditorGUILayout.TextField("API Key:", _apiKey, GUILayout.ExpandWidth(true));

            float buttonWidth = 19;
            if (GUILayout.Button(EditorGUIUtility.IconContent("Clipboard"), GUILayout.Width(buttonWidth), GUILayout.MaxHeight(buttonWidth)))
            {
                _apiKey = EditorGUIUtility.systemCopyBuffer;
            }

            if (_apiKey != previousApiKey)
            {
                SecureDataHandler.SaveAPIKey(_apiKey);
            }

            GUILayout.Space(paddingRight);
            GUILayout.EndHorizontal();

            GUILayout.Space(paddingBottom);
            GUILayout.EndVertical();
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
}