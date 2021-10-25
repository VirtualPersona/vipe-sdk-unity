using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DemoUse : MonoBehaviour
{

    private const string API_KEY = "1ab2c3d4e5f61ab2c3d4e5f6";

    private bool userLoggedIn;

    private CryptoAvatars cryptoAvatars;

    // Login panel
    public GameObject loginPanel;
    public InputField emailField;
    public InputField passField;
    public Button loginBtn;
    public Button enterAsGuestBtn;
    public GameObject errorLoginTxt;

    // Avatars panel
    public GameObject avatarsPanel;
    public Button avatarsPanelBtn;
    public GameObject contentScrollView;
    public GameObject avatarPreviewLayout;

    // 

    public void Awake()
    {
        this.userLoggedIn = false;
        this.cryptoAvatars = new CryptoAvatars(API_KEY);
        loginBtn.onClick.AddListener(OnLoginClick);
        enterAsGuestBtn.onClick.AddListener(onGuestEnterClick);
    }

    private void OnLoginClick()
    {
        string email = emailField.text;
        string pass = passField.text;
        IEnumerator login = cryptoAvatars.UserLogin(email, pass, (onLoginResult) => {
            this.userLoggedIn = onLoginResult;
            if (this.userLoggedIn)
            {
                errorLoginTxt.SetActive(false);
                loginPanel.SetActive(false);
                avatarsPanel.SetActive(true);
                avatarsPanelBtn.GetComponentInChildren<Text>().text = "My avatars";
                this.downloadAvatars();
            }
            else
            {
                errorLoginTxt.SetActive(true);
            }
        });
        StartCoroutine(login);
    }

    private void onGuestEnterClick()
    {
        avatarsPanelBtn.GetComponentInChildren<Text>().text = "Back to login";
        avatarsPanelBtn.onClick.AddListener(backToLogin);
        loginPanel.SetActive(false);
        avatarsPanel.SetActive(true);
        this.downloadAvatars();
    }

    private void backToLogin()
    {
        errorLoginTxt.SetActive(false);
        loginPanel.SetActive(true);
        avatarsPanel.SetActive(false);
    }

    private void downloadAvatars()
    {
        IEnumerator getAvatars = cryptoAvatars.GetAvatars((onAvatarsResult) => {
            Structs.Avatar[] avatars = onAvatarsResult.avatars;
            for (int i = 0; i < avatars.Length; i++)
            {

            }
        });
        StartCoroutine(getAvatars);
    }

}
