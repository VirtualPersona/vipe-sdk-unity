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
        private const string LOGIN_URL = "http://localhost:4200/connect?integrationToken=julian";

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
                OnLoginStart();
                Application.OpenURL(LOGIN_URL);

                StartCoroutine(CheckForUserToken());
            }
        }

        private IEnumerator CheckForUserToken()
        {
            float checkInterval = 2f; // Interval in seconds between each API check
            float timeout = 60f; // Timeout in seconds
            float elapsedTime = 0f;

            var queryParams = new Dictionary<string, string>
    {
        { "integrationToken", "julian"}
    };

            string pageUrl = HttpService.instance.AddOrUpdateParametersInUrl(USER_INTEGRATION_TOKEN_URL, queryParams);

            Debug.Log("pageUrl:" + pageUrl);

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
                        Debug.Log("result:" + result);
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
                        Debug.Log("User found: " + user.wallet);
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


        private void OnLoginStart()
        {
            isLoginProcessActive = true;
            LoadingSpinner.SetActive(true);
        }

        public void OnLoginSuccess()
        {

            Debug.Log("Login successful");
            isLoginProcessActive = false;

            LoadingSpinner.SetActive(false);

            if (MenuManager.Instance)
            {
                MenuManager.Instance.LoadAvatarUI();
                IsLoggedIn = true;
                MenuManager.Instance.SetOwnerButtonToggleToTrue();
            }
        }

        private void CancelLogin()
        {
            StopCoroutine(CheckForUserToken());
            isLoginProcessActive = false;
            LoadingSpinner.SetActive(false);
        }

    }
}
