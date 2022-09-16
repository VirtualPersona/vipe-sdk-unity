using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityStandardAssets.Utility;
using static Structs;

public class UseManager : MonoBehaviour
{
    // TEST API-KEY , request your own api key. Will be disabled soon.
    private const string API_KEY = "$2b$10$jXmDbzXmgU7YjsshSRuSnOfdlMky/eUX7LPhJ0Y8jAtypyu4vJK1a";

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
    public GameObject playingPanel;
    public Button playingPanelBtn;
    public GameObject contentScrollView;
    public GameObject avatarPreviewLayout;
    public Button loadMoreBtn;
    public Button loadPreviousBtn;
    public Text pageInputField; //public InputField pageInputField;
    public Text totalPagesField;
    public Dropdown collectionSelector;
    public Toggle openSourceToggle;

    //Configurable
    private int nftPerLoad = 20;
    private int nftsSkipped = 0;
    private int pageCount = 1;
    //
    private GameObject Vrm;
    private GameObject Vrm_Target;
    private GameObject Cam;
    private GameObject camTarget;
    //
    private int totalNfts = 0;
    private int totalPages = 0;

    private string nextPage = "";
    private string previousPage = "";

    private string collectionAddressSelected = "0xc1def47cf1e15ee8c2a92f4e0e968372880d18d1";
    private string licenseType = "CC0";

    private string userWallet = "";
    //private bool openSource = true;

    public void Awake()
    {
        this.userLoggedIn = false;
        this.cryptoAvatars = new CryptoAvatars(API_KEY);
        loginBtn.onClick.AddListener(OnLoginClick);
        enterAsGuestBtn.onClick.AddListener(onGuestEnterClick);

        this.Vrm = GameObject.Find("VRM");
        this.Vrm_Target = GameObject.Find("VRM_Target");
        this.Cam = GameObject.Find("Main Camera");
        this.camTarget = GameObject.Find("Camera_Target");
    }

    private void OnLoginClick()
    {
        string email = emailField.text;
        string pass = passField.text;
        this.pageCount = 1;
        this.pageInputField.text = this.pageCount.ToString();

        IEnumerator login = cryptoAvatars.UserLogin(email, pass, onLoginResult => {
            this.userLoggedIn = onLoginResult.userId != null;
            
            if (this.userLoggedIn)
            {

                this.userWallet = onLoginResult.wallet;

                avatarsPanelBtn.GetComponentInChildren<Text>().text = "Back to login";
                avatarsPanelBtn.onClick.AddListener(backToLogin);
                loginPanel.SetActive(false);
                avatarsPanel.SetActive(true);
                this.downloadAvatarsUsers($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
                loadMoreBtn.onClick.AddListener(loadMoreNfts);
                loadPreviousBtn.onClick.AddListener(loadPreviousNfts);
                //pageInputField.onValueChanged.AddListener(loadNftsPage);
                collectionSelector.onValueChanged.AddListener(changeCollection);
                openSourceToggle.onValueChanged.AddListener(refreshAvatars);

                return;
            }

            errorLoginTxt.SetActive(true);
        });
        StartCoroutine(login);
    }

    private void onGuestEnterClick()
    {
        this.pageCount = 1;
        this.pageInputField.text = this.pageCount.ToString();
        
        avatarsPanelBtn.GetComponentInChildren<Text>().text = "Back to login";
        avatarsPanelBtn.onClick.AddListener(backToLogin);
        loginPanel.SetActive(false);
        avatarsPanel.SetActive(true);
        this.downloadAvatars( $"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        loadMoreBtn.onClick.AddListener(loadMoreNfts);
        loadPreviousBtn.onClick.AddListener(loadPreviousNfts);
        //pageInputField.onValueChanged.AddListener(loadNftsPage);
        collectionSelector.onValueChanged.AddListener(changeCollection);
        openSourceToggle.onValueChanged.AddListener(refreshAvatars);

    }
    private void changeCollection(int value)
    {
        switch (value)
        {
            case 0:
                this.collectionAddressSelected = "0xc1def47cf1e15ee8c2a92f4e0e968372880d18d1";
                break;
            case 1:
                this.collectionAddressSelected = "0xd047666daea0b7275e8d4f51fcff755aa05c3f0a";
                break;
            case 2:
                this.collectionAddressSelected = "0x28ccbe824455a3b188c155b434e4e628babb6ffa";
                break;
        }
        removeCurrentAvatarsCards();
        if (this.userWallet != "")
        {
            this.downloadAvatarsUsers($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        }
        else
        {
            this.downloadAvatars($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        }
    }
    
    private void refreshAvatars(bool value)
    {
        
        if (this.userLoggedIn)
        {
            this.downloadAvatarsUsers($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        }
        else
        {
            this.downloadAvatars($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        }
    }
    private void backToLogin()
    {
        errorLoginTxt.SetActive(false);
        loginPanel.SetActive(true);
        avatarsPanel.SetActive(false);

        //Remove Listeners because they will keep adding and stakking them when clicking 'login' or 'Enter as guest'
        loadMoreBtn.onClick.RemoveListener(loadMoreNfts);
        loadPreviousBtn.onClick.RemoveListener(loadPreviousNfts);
        collectionSelector.onValueChanged.RemoveListener(changeCollection);
        openSourceToggle.onValueChanged.RemoveListener(refreshAvatars);
        avatarsPanelBtn.onClick.RemoveListener(backToLogin);

        if (contentScrollView.transform.childCount > 0)
        {
            removeCurrentAvatarsCards();
        }
        this.Vrm = GameObject.Find("VRM"); 
        if (Vrm)
        {
            this.Vrm.transform.position = this.Vrm_Target.transform.position;
            this.Vrm.transform.rotation = this.Vrm_Target.transform.rotation;
            Destroy(this.Vrm.GetComponent<CharacterController>());

            this.Cam.transform.position = this.camTarget.transform.position;
            this.Cam.transform.rotation = this.camTarget.transform.rotation;

            this.Cam.GetComponent<SmoothFollow>().target = null;
        }
    }

    private void backToAvatars()
    {
        errorLoginTxt.SetActive(false);
        loginPanel.SetActive(false);
        playingPanel.SetActive(false);

        avatarsPanel.SetActive(true);

        playingPanelBtn.onClick.RemoveListener(backToAvatars);
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
            //this.nftsSkipped = toSkip;

            downloadAvatars($"nfts/avatars/list?skip={toSkip}&limit={this.nftPerLoad}");

        }

    }

    private void loadMoreNfts() 
    {
        this.nftsSkipped += this.nftPerLoad;
        if (this.nftsSkipped >= (this.totalNfts - this.nftPerLoad))
            this.nftsSkipped = this.totalNfts - this.nftPerLoad;

        if (this.pageCount > this.totalPages)
            this.pageCount = this.totalPages;

        //Load the new avatars with a corroutine
        if (this.nextPage != "")
        {
            this.pageCount += 1;
            this.pageInputField.text = this.pageCount.ToString();

            //Remove card avatars already loaded
            StartCoroutine(disablePageButton(1.3f));
            downloadAvatars(this.nextPage);
        }
    }

    private void loadPreviousNfts()
    {
        this.nftsSkipped -= this.nftPerLoad;
        if (this.nftsSkipped < 0)
            this.nftsSkipped = 0;
        if(this.pageCount < 0)
            this.pageCount = 0;

        //Load the new avatars
        if(this.previousPage != "")
        {
            this.pageCount -= 1;
            this.pageInputField.text = this.pageCount.ToString();

            //Remove card avatars already loaded
            StartCoroutine(disablePageButton(1.3f));
            downloadAvatars(this.previousPage);
        }

    }

    private void displayAndLoadAvatars(NftsArray onAvatarsResult)
    {
        Structs.Nft[] nfts = onAvatarsResult.nfts;
        this.totalNfts = onAvatarsResult.totalNfts;
        this.totalPages = totalNfts / nftPerLoad;
        totalPagesField.text = this.totalPages.ToString();


        const string urlServer = "https://api.cryptoavatars.io/v1/";
        int pos = urlServer.IndexOf(".io/v1/");

        if (onAvatarsResult.next != null && onAvatarsResult.next != "Null" && onAvatarsResult.next != "")
            this.nextPage = onAvatarsResult.next.Substring(pos + 7);
        else
            this.nextPage = "";

        if (onAvatarsResult.prev != null && onAvatarsResult.prev != "Null" && onAvatarsResult.prev != "")
            this.previousPage = onAvatarsResult.prev.Substring(pos + 7);
        else
            this.previousPage = "";

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
                    model.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Anims/Animator/ThirdPersonAnimatorController") as RuntimeAnimatorController;

                    //Adjust axis (It comes with Y and Z flipped) (Blender)

                    float h = Input.GetAxisRaw("Horizontal");
                    float v = Input.GetAxisRaw("Vertical");
                    Vector3 dir = new Vector3(h, 0, v);
                    model.transform.InverseTransformDirection(dir);

                    //model.transform.position += new Vector3(0, GameObject.Find("Cylinder").transform.localScale.y, 0);

                    ////STANDARD ASSETS

                    SkinnedMeshRenderer[] comps = model.GetComponentsInChildren<SkinnedMeshRenderer>();
                    Vector3 totalSize = new Vector3(0, 0, 0);
                    for (int j = 0; j < comps.Length; j++)
                    {
                        totalSize += comps[j].bounds.size;
                    }
                    //Debug.Log("Avatar Size: ");
                    //Debug.Log(totalSize);

                    model.AddComponent<CapsuleCollider>();
                    model.GetComponent<CapsuleCollider>().radius = 0.2f;
                    model.GetComponent<CapsuleCollider>().height = totalSize.y;
                    model.GetComponent<CapsuleCollider>().center = new Vector3(0.0f, totalSize.y / 2.0f, 0.0f);

                    model.AddComponent<Rigidbody>().useGravity = true;

                    model.AddComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
                    model.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>().m_JumpPower = 5.5f;
                    model.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>().m_GroundCheckDistance = 0.4f;
                    model.AddComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();

                    //si ya esta el VRM en escena, lo seleccionamos como target de nuestro follow script que se encuentra en la camara
                    if (this.Cam)
                    {
                        var child = new GameObject();
                        child.name = "VRM_Child";
                        child.transform.localPosition = new Vector3(0, 1, 0);
                        child.transform.localRotation = Quaternion.Euler(0,-180,0);
                        child.transform.parent = model.transform;
                        this.Cam.GetComponent<SmoothFollow>().target = child.transform;
                    }
                });

                StartCoroutine(downloadVRM);
                this.avatarsPanel.SetActive(false);
                this.playingPanel.SetActive(true);
                this.playingPanelBtn.onClick.AddListener(backToAvatars);
            });

            IEnumerator loadAvatarPreviewImage = this.cryptoAvatars.GetAvatarPreviewImage(nft.metadata.image, texture => {
                cardAvatarController.LoadAvatarImage(texture);
            });

            StartCoroutine(loadAvatarPreviewImage);
        }

    }
    IEnumerator disablePageButton(float seconds)
    {
        avatarsPanelBtn.onClick.RemoveListener(backToLogin);
        loadPreviousBtn.onClick.RemoveListener(loadPreviousNfts);
        loadMoreBtn.onClick.RemoveListener(loadMoreNfts);
        yield return new WaitForSeconds(seconds);
        avatarsPanelBtn.onClick.AddListener(backToLogin);
        loadMoreBtn.onClick.AddListener(loadMoreNfts);
        loadPreviousBtn.onClick.AddListener(loadPreviousNfts);
    }

    private void downloadAvatars(string pageUrl)
    {
        removeCurrentAvatarsCards();
        IEnumerator getAvatars = cryptoAvatars.GetAvatars(this.collectionAddressSelected,this.licenseType,pageUrl, onAvatarsResult => displayAndLoadAvatars(onAvatarsResult));

        StartCoroutine(getAvatars);
    }

    private void downloadAvatarsUsers(string pageUrl)
    {
        removeCurrentAvatarsCards();
        //Debug.Log(openSourceToggle.isOn);
        if (openSourceToggle.isOn)
        {
            IEnumerator getAvatars = cryptoAvatars.GetAvatars(this.collectionAddressSelected, this.licenseType, pageUrl, onAvatarsResult => displayAndLoadAvatars(onAvatarsResult));
            StartCoroutine(getAvatars);
        }

        //Use userWallet
        IEnumerator getAvatarsUser = cryptoAvatars.GetUserAvatars(this.collectionAddressSelected, this.userWallet , pageUrl, onAvatarsResult => displayAndLoadAvatars(onAvatarsResult));
        StartCoroutine(getAvatarsUser);

    }
}
