using System.Collections;
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
        IEnumerator login = cryptoAvatars.UserLogin(email, pass, onLoginResult => {
            this.userLoggedIn = onLoginResult;
            if (this.userLoggedIn)
            {
                errorLoginTxt.SetActive(false);
                loginPanel.SetActive(false);
                avatarsPanel.SetActive(true);
                avatarsPanelBtn.GetComponentInChildren<Text>().text = "My avatars";
                avatarsPanelBtn.onClick.AddListener(downloadAvatarsUsers);
                this.downloadAvatars();
                return;
            }

            errorLoginTxt.SetActive(true);
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

    private void downloadAvatarsUsers()
    {

    }

    private void downloadAvatars()
    {
        IEnumerator getAvatars = cryptoAvatars.GetAvatars(0, 10, onAvatarsResult =>
        {
            Structs.Avatar[] avatars = onAvatarsResult.avatars;
            for (int i = 0; i < avatars.Length; i++)
            {
                // Create panel layout for each avatar
                Structs.Avatar avatar = avatars[i];
                GameObject cardAvatar = Instantiate(avatarPreviewLayout);
                cardAvatar.transform.SetParent(contentScrollView.transform, false);
                CardAvatarController cardAvatarController = cardAvatar.GetComponent<CardAvatarController>();
                cardAvatarController.SetAvatarData(avatar.metadata.name, avatar.metadata.asset, i, urlVrm => {

                    Debug.Log(urlVrm);
                    // Cargar VRM en el pedestal, descargarlo y limpiar recursos al cargar otro vrm
                    IEnumerator downloadVRM = this.cryptoAvatars.downloadVRM(urlVrm);
                    StartCoroutine(downloadVRM);

                });

                IEnumerator loadAvatarPreviewImage = this.cryptoAvatars.GetAvatarPreviewImage(avatar.metadata.image, texture => {
                    //texture.Resize(250, 350);
                    cardAvatarController.LoadAvatarImage(texture);
                });

                StartCoroutine(loadAvatarPreviewImage);
            }
        });

        StartCoroutine(getAvatars);
    }



}
