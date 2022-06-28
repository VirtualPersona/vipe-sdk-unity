using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DemoUse : MonoBehaviour
{
    // TEST API-KEY , request your own api key. Will be disabled soon.
    //private const string API_KEY = "1ab2c3d4e5f61ab2c3d4e5f6";
    private const string API_KEY = " $2b$10$cd85DoGHWH4h3tdMi9TAweJlmKWl3cdBuMlYXN16hPXqE4GbJZhcy";

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
    public Button loadMoreBtn;
    public Button loadPreviousBtn;
    public InputField pageInputField;

    //Configurable
    private int nftPerLoad = 20;
    private int nftsSkipped = 0;

    //
    private int totalNfts = 0;
    private int totalPages = 0;

    private string nextPage = "";

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
                this.downloadAvatars(nftsSkipped, nftPerLoad ,"");
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
        this.downloadAvatars(nftsSkipped, nftPerLoad,"");
        loadMoreBtn.onClick.AddListener(loadMoreNfts);
        loadPreviousBtn.onClick.AddListener(loadPreviousNfts);
        pageInputField.onValueChanged.AddListener(loadNftsPage);

    }

    private void backToLogin()
    {
        errorLoginTxt.SetActive(false);
        loginPanel.SetActive(true);
        avatarsPanel.SetActive(false);

        if(contentScrollView.transform.childCount > 0)
        {
            removeCurrentAvatarsCards();
        }
    }

    private void downloadAvatarsUsers()
    {

    }

    private void removeCurrentAvatarsCards()
    {
        
        int childrenLength = contentScrollView.transform.childCount;
        for (int i = 0; i < childrenLength; i++)
        {
            GameObject cardAvatar = contentScrollView.transform.GetChild(i).gameObject;
            Destroy(cardAvatar);
        }
    }

    private void loadNftsPage(string value)
    {

        int page;
        if(value == null || value == "")
            page = 0;

        else
            page = Int32.Parse(value);

        if (page < 0)
            page = 0;
        else if (page > this.totalPages)
            page = this.totalPages;

        if(page >= 0 && page <= this.totalPages)
        {
            removeCurrentAvatarsCards();

            //Navigate to page
            int toSkip = page * nftPerLoad;
            this.nftsSkipped = toSkip;

            downloadAvatars(nftsSkipped, nftPerLoad,"");

        }

    }

    private void loadMoreNfts()
    {
        //Remove card avatars already loaded
        removeCurrentAvatarsCards();
        this.nftsSkipped += this.nftPerLoad;
        if (this.nftsSkipped >= (this.totalNfts - this.nftPerLoad))
            this.nftsSkipped = this.totalNfts - this.nftPerLoad;

        //Load the new avatars
        //Aquí si hay que pasar la url de next
        downloadAvatars(nftsSkipped, nftPerLoad,this.nextPage);

    }

    private void loadPreviousNfts()
    {
        //Remove card avatars already loaded
        removeCurrentAvatarsCards();
        
        this.nftsSkipped -= this.nftPerLoad;
        if (this.nftsSkipped < 0)
            this.nftsSkipped = 0;

        //Load the new avatars
        downloadAvatars(nftsSkipped, nftPerLoad, "");

    }

    private void downloadAvatars(int skip, int limit, string nextPageUrl)
    {
        IEnumerator getAvatars = cryptoAvatars.GetAvatars(skip, limit, nextPageUrl, onAvatarsResult =>
        {
            Structs.Nft[] nfts = onAvatarsResult.nfts;
            this.totalNfts = onAvatarsResult.total;
            this.totalPages = totalNfts / nftPerLoad;

            int pos = onAvatarsResult.next.IndexOf(".io/");
            this.nextPage = onAvatarsResult.next.Substring(pos+4);


            for (int i = 0; i < nfts.Length; i++)
            {
                // Create panel layout for each avatar
                Structs.Nft nft = nfts[i];
                GameObject cardAvatar = Instantiate(avatarPreviewLayout);
                cardAvatar.transform.SetParent(contentScrollView.transform, false);
                CardAvatarController cardAvatarController = cardAvatar.GetComponent<CardAvatarController>();
                cardAvatarController.SetAvatarData(nft.metadata.name, nft.metadata.asset, i, urlVrm => {

                    if (GameObject.Find("VRM"))
                        Destroy(GameObject.Find("VRM"));

                    IEnumerator downloadVRM = this.cryptoAvatars.GetAvatarVRMModel(urlVrm, (model) =>
                    {
                        model.transform.Rotate(new Vector3(0, 180, 0));
                        model.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Anims/Pruebas") as RuntimeAnimatorController;
                        //
                        //model.transform.eulerAngles = new Vector3(0.0f, -180.0f, 0.0f);
                        //model.transform.eulerAngles += new Vector3(0, 180, 0);
                        //model.transform.localScale.Scale(new Vector3(1, -1, 1));
                        float h = Input.GetAxisRaw("Horizontal");
                        float v = Input.GetAxisRaw("Vertical");
                        Vector3 dir = new Vector3(h, 0, v);
                        model.transform.InverseTransformDirection(dir);

                        
                        model.GetComponent<Animator>().stabilizeFeet = true;
                        //model.transform.forward = new Vector3(0.0f, 0.0f, 1.0f);
                        
                        model.transform.position += new Vector3(0, GameObject.Find("Cylinder").transform.localScale.y, 0);
                    });

                    StartCoroutine(downloadVRM);

                });

                IEnumerator loadAvatarPreviewImage = this.cryptoAvatars.GetAvatarPreviewImage(nft.metadata.image, texture => {
                    cardAvatarController.LoadAvatarImage(texture);
                });

                StartCoroutine(loadAvatarPreviewImage);
            }


        });

        StartCoroutine(getAvatars);
    }

}
