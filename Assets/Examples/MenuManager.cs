using CA;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private RectTransform loginPanel;
    [SerializeField]
    private RectTransform avatarsPanel;

    private CryptoAvatars cryptoAvatars;

    [SerializeField]
    private Button useAsGuestBtn;
    [SerializeField]
    private Button nextPageBtn;
    [SerializeField]
    private Button prevPageBtn;

    /*[SerializeField]
    private WalletLogin metamaskLogin;*/

    public bool userLoggedIn { get; private set; }

    [SerializeField]
    private TMP_Text walletAddress;

    [SerializeField]
    private GameObject avatarCardPrefab;
    [SerializeField]
    private GameObject loadingSpinner;

    [SerializeField]
    private ScrollRect scrollViewAvatars;
    [SerializeField]
    private TMP_InputField searchField;

    [SerializeField]
    private TMP_Text currentPageText;

    private GameObject vrm;

    private bool loginPanelOn = true;

    private void Start()
    {
        TryGetComponent(out cryptoAvatars);

        vrm = GameObject.Find("AVATAR");

        useAsGuestBtn.onClick.AddListener(OnGuestEnter);

        nextPageBtn.onClick.AddListener(() => cryptoAvatars.NextPage(avatarsResult => LoadAndDisplayAvatars(avatarsResult)));
        prevPageBtn.onClick.AddListener(() => cryptoAvatars.PrevPage(avatarsResult => LoadAndDisplayAvatars(avatarsResult)));

        searchField.onValueChanged.AddListener((value) => {
            CAModels.SearchAvatarsDto searchAvatar = new() { name = value };
            cryptoAvatars.GetAvatars(searchAvatar, LoadAndDisplayAvatars);
        });
    }

    private void OnGuestEnter()
    {
        Vector3 screenPos = loginPanelOn
            ? loginPanel.GetComponent<RectTransform>().position
            : avatarsPanel.GetComponent<RectTransform>().position;

        Vector3 hiddenPos = loginPanelOn
            ? avatarsPanel.GetComponent<RectTransform>().position
            : loginPanel.GetComponent<RectTransform>().position;

        avatarsPanel.GetComponent<RectTransform>().position = loginPanelOn ? screenPos : hiddenPos;
        loginPanel.GetComponent<RectTransform>().position = loginPanelOn ? hiddenPos : screenPos;

        cryptoAvatars.GetAvatars(new (), avatarsResult => LoadAndDisplayAvatars(avatarsResult));
    }

    public void LoginProcess(string email, string password)
    {
        cryptoAvatars.Web2Login(email, password, onLoginResult => {
            this.userLoggedIn = onLoginResult.userId != null;
            if (this.userLoggedIn)
                cryptoAvatars.GetUserAvatars(onLoginResult.wallet, onAvatarsResult => LoadAndDisplayAvatars(onAvatarsResult));
        });
    }

    public void MetamaskLoginProccess(string walletAddress, string signature)
    {
        cryptoAvatars.Web3Login(walletAddress, signature, onLoginResult => {
            this.userLoggedIn = onLoginResult.userId != null;
            if (this.userLoggedIn)
                cryptoAvatars.GetUserAvatars(walletAddress, onAvatarsResult => LoadAndDisplayAvatars(onAvatarsResult));
        });
    }

    private void LoadAndDisplayAvatars(CAModels.NftsArray onAvatarsResult)
    {
        foreach (Transform child in scrollViewAvatars.content)
            Destroy(child.gameObject);

        prevPageBtn.enabled = cryptoAvatars.HasPrevPage();
        nextPageBtn.enabled = cryptoAvatars.HasNextPage();

        currentPageText.text = $"{onAvatarsResult.currentPage}/{onAvatarsResult.totalPages}";
        CAModels.Nft[] nfts = onAvatarsResult.nfts;

        foreach (var nft in nfts)
        {
            GameObject avatarCard = Instantiate(avatarCardPrefab, scrollViewAvatars.content.transform);
            CardAvatarController cardController = avatarCard.GetComponentInChildren<CardAvatarController>();
            cardController.SetAvatarData(nft.metadata.name, nft.metadata.asset, urlVRM => {

                loadingSpinner.SetActive(true);
                cryptoAvatars.GetAvatarVRMModel(urlVRM, (model, path) =>
                {
                    Vector3 avatarPos = vrm.transform.position;
                    Quaternion avatarRot = vrm.transform.rotation;
                    Destroy(vrm);
                    vrm = model;
                    vrm.name = "AVATAR";
                    vrm.transform.SetPositionAndRotation(avatarPos, avatarRot);
                    vrm.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Anims/VRM");
                    loadingSpinner.SetActive(false);
                });

            });

            this.cryptoAvatars.GetAvatarPreviewImage(nft.metadata.image, texture => {
                cardController.LoadAvatarImage(texture);
            });
        }
    }

    public void LoadMoreNfts()
    {
        cryptoAvatars.NextPage(onAvatarsResult => LoadAndDisplayAvatars(onAvatarsResult));
    }

    public void LoadPreviousNfts()
    {
        cryptoAvatars.PrevPage(onAvatarsResult => LoadAndDisplayAvatars(onAvatarsResult));
    }

    public void OnAnimSelected(TMP_Dropdown dropdown)
    {
        GameObject.Find("VRM").GetComponent<Animator>().SetTrigger(dropdown.options[dropdown.value].text.ToLower());
    }

}
