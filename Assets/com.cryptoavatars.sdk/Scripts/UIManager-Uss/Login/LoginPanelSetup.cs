using BloodUI;
using CA;
using System.Collections;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
public class LoginPanelSetup : MonoBehaviour
{
    [Header("API KEY IS REQUIRED TO PERFORM HTTP REQUESTS")]
    [SerializeField]
    private const string API_KEY = "";

    private bool userLoggedIn;
    private string userWallet = "";
    private CryptoAvatars cryptoAvatars;
    //Configurable
    private int nftPerLoad = 20;
    private int nftsSkipped = 0;
    private int pageCount = 1;
    // 
    private int totalNfts = 0;
    private int totalPages = 0;

    // UI Toolkit 
    private UIDocument doc;
    private VisualElement root;
    // TODO - Add an Error Text to LoginPanelWindow
    public LoginPanelWindow loginPanelWindow;
    [SerializeField]
    private GameObject AvatarsUIDoc;
    private void Awake()
    {
        this.userLoggedIn = false;
        this.cryptoAvatars = new CryptoAvatars(API_KEY);
        loginPanelWindow = new LoginPanelWindow();
    }
    private void OnEnable()
    {
        TryGetComponent(out doc);
        root = doc.rootVisualElement;
        loginPanelWindow = new LoginPanelWindow();
        loginPanelWindow.LoginRequested += OnLoginClick;
        loginPanelWindow.GuestRequested += OnGuestClickEnter;
        root.Add(loginPanelWindow);
        //root.Add(avatarPanelWindow);
        //AvatarHide();
    }
    public void LoginHide()
    {
        if (loginPanelWindow != null)
            root.Remove(loginPanelWindow);
    }

    public void LoginShow()
    {
        if (loginPanelWindow != null)
            root.Add(loginPanelWindow);
    }
    private void OnLoginClick()
    {
        string email = loginPanelWindow.emailTextField.text;
        string pass = loginPanelWindow.passwordTextField.text;
        this.pageCount = 1;
        IEnumerator login = cryptoAvatars.UserLogin(email, pass, onLoginResult =>
        {
            this.userLoggedIn = onLoginResult.userId != null;

            if (this.userLoggedIn)
            {
                this.userWallet = onLoginResult.wallet;
                if (AvatarsUIDoc.activeSelf)
                    AvatarsUIDoc.GetComponent<AvatarSelectionSetup>().ShowAvatarSelection();

                AvatarsUIDoc.SetActive(true);
                AvatarsUIDoc.GetComponent<AvatarSelectionSetup>().UserLoggedIn = userLoggedIn;
                AvatarsUIDoc.GetComponent<AvatarSelectionSetup>().UserWallet = userWallet;
                LoginHide();
                // Show Error
                return;
            }
        });
        StartCoroutine(login);
    }
    private void OnGuestClickEnter()
    {
        this.pageCount = 1;
        LoginHide();
        if (AvatarsUIDoc.activeSelf)
            AvatarsUIDoc.GetComponent<AvatarSelectionSetup>().ShowAvatarSelection();
        // Show Avatar List
        AvatarsUIDoc.SetActive(true);
    }

}

