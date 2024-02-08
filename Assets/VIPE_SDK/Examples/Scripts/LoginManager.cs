using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static VIPE_SDK.Models;

namespace VIPE_SDK
{
    public class LoginManager : MonoBehaviour
    {
        // private const string LOGIN_URL = "https://vipe.io/connect?integrationLogin=true";
        private const string LOGIN_URL = "http://localhost:4200/connect?integrationToken=";

        // private const string USER_INTEGRATION_TOKEN_URL = "https://api.cryptoavatars.io/v1/users";
        private const string USER_INTEGRATION_TOKEN_URL = "/users";

        private string walletAddress;
        private string signature;

        private bool messageObtained = false;
        private bool isLoginProcessActive = false;

        public bool IsLoggedIn = false;

        public GameObject LoadingSpinner;

        private void Update()
        {
            if (isLoginProcessActive && Input.GetKeyDown(KeyCode.Escape))
            {
                CancelLogin();
            }
        }

        public string GetWallet()
        {
            return walletAddress;
        }

        public void OnLoginButtonClick()
        {
            if (!IsLoggedIn)
            {
                isLoginProcessActive = true;
                LoadingSpinner.SetActive(true);

                (string integrationToken, bool isExistingToken) = GetIntegrationToken();

                if (!isExistingToken)
                {
                    string modifiedUrl = LOGIN_URL + integrationToken;
                    Application.OpenURL(modifiedUrl);
                }

                StartCoroutine(CheckForUserToken(integrationToken));
            }
        }

        private IEnumerator CheckForUserToken(string integrationToken)
        {
            float checkInterval = 2f; // Interval in seconds between each API check
            float timeout = 60f; // Timeout in seconds
            float elapsedTime = 0f;

            var queryParams = new Dictionary<string, string> { { "integrationToken", integrationToken } };

            // Continue with the login process...

            string pageUrl = HttpService.instance.AddOrUpdateParametersInUrl(USER_INTEGRATION_TOKEN_URL, queryParams);

            while (elapsedTime < timeout)
            {
                string result = null;
                Exception exception = null;

                // Nested function to handle async call
                async Task RequestAsync()
                {
                    try
                    {
                        result = await HttpService.Instance().Get(pageUrl);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }

                // Start the async request
                Task requestTask = RequestAsync();

                // Wait for the task to complete
                yield return new WaitUntil(() => requestTask.IsCompleted);

                if (exception != null)
                {
                    CancelLogin();
                    Debug.LogError("Error while checking for user: " + exception.Message);
                }
                else
                {
                    User user = JsonUtility.FromJson<User>(result);
                    if (!string.IsNullOrEmpty(result) && !ReferenceEquals(user, null) && !string.IsNullOrEmpty(user.wallet))
                    {
                        walletAddress = user.wallet;
                        OnLoginSuccess();
                        yield break; // Exit the coroutine if user is found
                    }
                    else
                    {
                        CancelLogin();
                        Debug.Log("User not found. Checking again...");
                    }
                }

                yield return new WaitForSeconds(checkInterval);
                elapsedTime += checkInterval;
            }

            if (elapsedTime >= timeout)
            {
                Debug.LogWarning("Timeout reached. User check failed.");
                // Handle timeout scenario
            }
        }

        public void OnLoginSuccess()
        {

            Debug.Log("Login successful");
            isLoginProcessActive = false;

            LoadingSpinner.SetActive(false);

            if (MainManager.Instance)
            {
                MainManager.Instance.LoadAvatarUI();
                IsLoggedIn = true;
                MainManager.Instance.SetOwnerButtonToggleToTrue();
            }
        }

        private void CancelLogin()
        {
            string integrationToken = GetIntegrationToken().Item1;
            StopCoroutine(CheckForUserToken(integrationToken));
            isLoginProcessActive = false;
            LoadingSpinner.SetActive(false);
        }

        private (string, bool) GetIntegrationToken()
        {
            string integrationToken;
            bool isExistingToken = false;

            if (PlayerPrefs.HasKey("integrationToken"))
            {
                // If an integration token exists, fetch it
                integrationToken = PlayerPrefs.GetString("integrationToken");
                isExistingToken = true;
            }
            else
            {
                // If no integration token exists, create a new one
                integrationToken = Guid.NewGuid().ToString();

                // Save the new integration token
                PlayerPrefs.SetString("integrationToken", integrationToken);
                PlayerPrefs.Save();
            }

            return (integrationToken, isExistingToken);
        }

    }
}
