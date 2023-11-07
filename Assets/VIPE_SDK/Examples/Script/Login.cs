using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace VIPE_SDK
{
    public class Login : MonoBehaviour
    {
        private const string LOGIN_URL = "https://vipe.io/connect?integrationLogin=true";
        private const string POST_URL = "https://api.cryptoavatars.io/v1/login/vipe";
        private string WalletAddress;
        private string Signature;

        private bool messageObtained = false;
        private bool isLoginProcessActive = false;

        public bool isLoggedIn = false;

        public GameObject loadingSpinner;

        private void Update()
        {
            if (isLoginProcessActive && Input.GetKeyDown(KeyCode.Escape))
            {
                CancelLogin();
            }
        }

        public string GetWallet()
        {
            return WalletAddress;
        }

        public void OnLoginButtonClick()
        {
            if (!isLoggedIn)
            {
                OnLoginStart();
                Application.OpenURL(LOGIN_URL);

#if UNITY_WEBGL && !UNITY_EDITOR
                CallJavaScriptFunction();
#else
                StartCoroutine(GetClipboardMessage());
#endif
            }
        }

        // M�todo para llamar a una funci�n JavaScript en WebGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void CallJavaScriptFunction();

        public void OnWebGLLogin(string data)
        {
            print("Received wallet and signature " + data);
            messageObtained = true;
            ProcessMessage(data);
            OnLoginEnd();
        }


        private void CancelLogin()
        {
            StopCoroutine(GetClipboardMessage());
            isLoginProcessActive = false;
            loadingSpinner.SetActive(false);
        }

        public void ProcessMessage(string message)
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

        private IEnumerator GetClipboardMessage()
        {
            float timeout = 60f;
            float elapsedTime = 0f;

            while (!messageObtained && elapsedTime < timeout)
            {
                string copiedMessage = GUIUtility.systemCopyBuffer;

                if (!string.IsNullOrEmpty(copiedMessage) && copiedMessage.StartsWith("0x"))
                {
                    messageObtained = true;
                    ProcessMessage(copiedMessage);
                    GUIUtility.systemCopyBuffer = string.Empty;
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                    elapsedTime += 1f;
                }
            }

            OnLoginEnd();
        }

        private void OnLoginStart()
        {
            isLoginProcessActive = true;
            loadingSpinner.SetActive(true);
        }

        private void OnLoginEnd()
        {
            if (!isLoggedIn && messageObtained)
            {
                StartCoroutine(SendLoginData());
            }
            else
                Debug.LogWarning("Login process completed without obtaining a valid message.");

            loadingSpinner.SetActive(false);
        }

        private IEnumerator SendLoginData()
        {
            if (Signature == null || WalletAddress == null)
            {
                Debug.LogWarning("Signature or WalletAddress is null");
                yield break;
            }
            WWWForm form = new WWWForm();
            form.AddField("wallet", WalletAddress);
            form.AddField("signature", Signature);

            using (UnityWebRequest www = UnityWebRequest.Post(POST_URL, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("Error while sending: " + www.error);
                    CancelLogin();
                }
                else
                {
                    if (www.responseCode == 200)
                    {
                        Debug.Log("Login successful");
                        isLoginProcessActive = false;

                        if (MenuManager.Instance)
                        {
                            MenuManager.Instance.LoadAvatarUI();
                            isLoggedIn = true;
                            MenuManager.Instance.SetOwnerButtonToggleToTrue();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Failed to log in, HTTP status: " + www.responseCode);
                        isLoggedIn = false;
                        CancelLogin();
                    }
                }
            }
        }
    }
}
