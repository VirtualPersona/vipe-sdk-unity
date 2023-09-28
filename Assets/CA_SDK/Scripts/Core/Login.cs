using System.Collections;
using UnityEngine;

public class Login : MonoBehaviour
{
    private const string LOGIN_URL = "https://testnet.vipe.io/connect?integrationLogin=true";
    private string WalletAddress;
    private string Signature;

    private bool messageObtained = false;
    private bool isLoginProcessActive = false;

    public bool isLogedIn = false;

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
        if(!isLogedIn)
        {
            OnLoginStart();
            Application.OpenURL(LOGIN_URL);
            isLoginProcessActive = true;
            StartCoroutine(GetClipboardMessage());
        }
    }

    private void CancelLogin()
    {
        StopCoroutine(GetClipboardMessage());
        isLoginProcessActive = false;
        loadingSpinner.SetActive(false);
    }

    public void ProcessMessage(string message)
    {
        Debug.Log("Message: " + message);
        WalletAddress = message;
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
        Debug.Log("Login process started.");

        loadingSpinner.SetActive(true);
    }

    private void OnLoginEnd()
    {
        if (!isLogedIn && messageObtained)
        {
            Debug.Log("Login process completed successfully.");
            if (MenuManager.Instance)
            {
                MenuManager.Instance.LoadAvatarUI();
                _ = MenuManager.Instance.LoadOwnVRMAsync();
            }
            isLogedIn = true;
        }
        else
            Debug.LogWarning("Login process completed without obtaining a valid message.");

        loadingSpinner.SetActive(false);
    }
}