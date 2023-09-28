using CA;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class MenuManager : MonoBehaviour
{
    private CancellationTokenSource cts = new CancellationTokenSource();

    public static MenuManager Instance;

    private Login login;

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private RectTransform loginPanel;

    [SerializeField]
    private RectTransform avatarsPanel;
    private CryptoAvatars cryptoAvatars;

    [SerializeField]
    private Button loadVRM;

    [SerializeField]
    private Button nextPageBtn;

    [SerializeField]
    private Button prevPageBtn;

    public bool userLoggedIn { get; private set; }

    [SerializeField]
    private TMP_Text walletAddress;

    [SerializeField]
    private GameObject avatarCardPrefab;

    [SerializeField]
    private GameObject collectionCardPrefab;

    [SerializeField]
    private GameObject loadingSpinner;

    [SerializeField]
    private ScrollRect scrollViewAvatars;

    [SerializeField]
    private ScrollRect scrollViewCollections;

    [SerializeField]
    private TMP_InputField searchField;


    [SerializeField]
    private TMP_InputField URLField;

    [SerializeField]
    private TMP_Text currentPageText;

    [SerializeField]
    private GameObject vrm;

    private bool loginPanelOn = true;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of MenuManager");
        }
        MainThreadDispatcher mainThreadDispatcher = MainThreadDispatcher.Instance;
        cryptoAvatars = new CryptoAvatars(mainThreadDispatcher);
        login = GetComponent<Login>();
    }

    private void Start()
    {
        vrm = GameObject.Find("AVATAR");

        nextPageBtn.onClick.AddListener(
            async () =>
                await cryptoAvatars.NextPage(avatarsResult => LoadAndDisplayAvatars(avatarsResult))
        );
        prevPageBtn.onClick.AddListener(
            async () =>
                await cryptoAvatars.PrevPage(avatarsResult => LoadAndDisplayAvatars(avatarsResult))
        );

        SetSearchField();
        LoadCollections();
    }
    /// <summary>
    /// Loads a VRM model using a URL entered by the user.
    /// </summary>
    public void LoadByURL()
    {
        string url = URLField.text;
        if (url != "")
        {
            LoadVRMModel(url);
        }
    }
    /// <summary>
    /// Cancels any existing asynchronous tasks.
    /// </summary>
    private void CancelExistingTasks()
    {
        cts.Cancel();
        cts = new CancellationTokenSource();
    }
    /// <summary>
    /// Toggles the display between the login panel and avatar panel.
    /// </summary>
    public void LoadAvatarUI()
    {
        Vector3 screenPos = loginPanelOn
            ? loginPanel.GetComponent<RectTransform>().position
            : avatarsPanel.GetComponent<RectTransform>().position;

        Vector3 hiddenPos = loginPanelOn
            ? avatarsPanel.GetComponent<RectTransform>().position
            : loginPanel.GetComponent<RectTransform>().position;

        avatarsPanel.GetComponent<RectTransform>().position = loginPanelOn ? screenPos : hiddenPos;
        loginPanel.GetComponent<RectTransform>().position = loginPanelOn ? hiddenPos : screenPos;
    }
    /// <summary>
    /// Sets up the search field to execute a search query when the text changes.
    /// </summary>
    public void SetSearchField()
    {
        ClearScrollView();
        searchField.onValueChanged.AddListener(async (value) => await DebounceSearch(value, 300));
    }
    /// <summary>
    /// Delays the execution of a search query and performs it after a specified delay.
    /// </summary>
    /// <param name="value">The search query value.</param>
    /// <param name="delayMilliseconds">The delay duration in milliseconds.</param>
    private async Task DebounceSearch(string value, int delayMilliseconds)
    {
        CancelExistingTasks();

        try
        {
            await Task.Delay(delayMilliseconds, cts.Token);
            await ExecuteSearch(value);
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Task was canceled, no further action needed");
        }
    }
    /// <summary>
    /// Executes a search query to load avatars based on the provided search criteria.
    /// </summary>
    /// <param name="value">The search query value.</param>
    private async Task ExecuteSearch(string value)
    {
        var parameters = new Dictionary<string, string>
        {
            { "name", value },
            {"limit","6" }
        };
        CAModels.SearchAvatarsDto searchAvatar = new() { name = value };
        Action wrapperAction = async () =>
            await cryptoAvatars.GetAvatars(LoadAndDisplayAvatars,parameters);

        await Task.Run(() =>
        {
            MainThreadDispatcher.RunOnMainThread(wrapperAction);
        });
    }
    /// <summary>
    /// Loads the user's own VRM models asynchronously.
    /// </summary>
    public async Task LoadOwnVRMAsync()
    {
        CancelExistingTasks();
        ClearScrollView();

        CAModels.SearchAvatarsDto searchAvatar = new() { };
        Action wrapperAction = async () => await cryptoAvatars.GetAvatarsByURL(CryptoAvatars.avatarsResource + "/" + login.GetWallet(), LoadAndDisplayAvatars);

        await Task.Run(() => MainThreadDispatcher.RunOnMainThread(wrapperAction));
    }
    /// <summary>
    /// Wrapper for loading the user's own VRM models, handles login if the user is not logged in.
    /// </summary>
    public void LoadOwnVRMButtonWrapper()
    {
        if (login.isLogedIn)
        {
            _ = LoadOwnVRMAsync();
        }
        else
        {
            login.OnLoginButtonClick();
        }
    }
    /// <summary>
    /// Loads open-source VRM models asynchronously.
    /// </summary>
    public async Task LoadOpenSourceAsync()
    {
        CancelExistingTasks();
        ClearScrollView();

        var parameters = new Dictionary<string, string>
        {
            { "license", "CC0" },
            {"limit","6" }
        };

        Action wrapperAction = async () => await cryptoAvatars.GetAvatars(LoadAndDisplayAvatars, parameters);

        await Task.Run(() => MainThreadDispatcher.RunOnMainThread(wrapperAction));
    }
    /// <summary>
    /// Wrapper for loading open-source VRM models.
    /// </summary>
    public void LoadOpenSourceButtonWrapper()
    {
        _ = LoadOpenSourceAsync();
    }
    /// <summary>
    /// Clears the avatar scroll view by destroying its child objects.
    /// </summary>
    private void LoadAndDisplayAvatars(CAModels.NftsArray onAvatarsResult)
    {
        ClearScrollView();
        DisplayAvatars(onAvatarsResult);
    }
    /// <summary>
    /// Replaces the current VRM model with a new one.
    /// </summary>
    /// <param name="model">The new VRM model GameObject.</param>
    private void ClearScrollView()
    {
        if(scrollViewAvatars)
            foreach (Transform child in scrollViewAvatars.content)
            {
                Destroy(child.gameObject);
            }
    }
    /// <summary>
    /// Displays a list of avatars in the scroll view.
    /// </summary>
    /// <param name="onAvatarsResult">The data containing avatars to display.</param>
    private void ReplaceVRMModel(GameObject model)
    {
        Vector3 avatarPos = vrm.transform.position;
        Quaternion avatarRot = vrm.transform.rotation;
        Destroy(vrm);
        vrm = model;
        vrm.name = "AVATAR";
        vrm.transform.SetPositionAndRotation(avatarPos, avatarRot);
        vrm.GetComponent<Animator>().runtimeAnimatorController =
            Resources.Load<RuntimeAnimatorController>(
                "Anims/Animator/ThirdPersonAnimatorController"
            );
        vrm.AddComponent<ResizeCapsuleCollider>();
        vrm.AddComponent<ThirdPersonUserControl>();
        Camera.main.GetComponent<OrbitCamera>().targetPosition = vrm.transform;
    }
    // <summary>
    /// Displays a list of avatars in the scroll view.
    /// </summary>
    /// <param name="onAvatarsResult">The data containing avatars to display.</param>
    private async void DisplayAvatars(CAModels.NftsArray onAvatarsResult)
    {
        CAModels.Nft[] nfts = onAvatarsResult.nfts;
        currentPageText.text = onAvatarsResult.currentPage.ToString() + " | " + onAvatarsResult.totalPages;
        foreach (var nft in nfts)
        {
            GameObject avatarCard = Instantiate(
                avatarCardPrefab,
                scrollViewAvatars.content.transform
            );
            CardAvatarController cardController =
                avatarCard.GetComponentInChildren<CardAvatarController>();

            cardController.SetAvatarData(
                nft.metadata.name,
                nft.metadata.asset,
                urlVRM => LoadVRMModel(urlVRM)
            );

            await cryptoAvatars.GetAvatarPreviewImage(
                nft.metadata.image,
                texture => cardController.LoadAvatarImage(texture)
            );
        }
    }
    /// <summary>
    /// Loads a VRM model based on a URL.
    /// </summary>
    /// <param name="urlVRM">The URL of the VRM model to load.</param>
    private async void LoadVRMModel(string urlVRM)
    {
        loadingSpinner.SetActive(true);
        await cryptoAvatars.GetAvatarVRMModel(
            urlVRM,
            (model, path) =>
            {
                ReplaceVRMModel(model);
                loadingSpinner.SetActive(false);
            }
        );
    }
    /// <summary>
    /// Loads more avatars when the "Load More" button is clicked.
    /// </summary>
    public async void LoadMoreNfts()
    {
        await cryptoAvatars.NextPage(onAvatarsResult => LoadAndDisplayAvatars(onAvatarsResult));
    }
    /// <summary>
    /// Loads previous avatars when the "Load Previous" button is clicked.
    /// </summary>
    public async void LoadPreviousNfts()
    {
        await cryptoAvatars.PrevPage(onAvatarsResult => LoadAndDisplayAvatars(onAvatarsResult));
    }
    /// <summary>
    /// Clears the collection scroll view by destroying its child objects.
    /// </summary>
    private void ClearCollection()
    {
        if(scrollViewCollections)
            foreach (Transform child in scrollViewCollections.content.transform)
            {
                Destroy(child.gameObject);
            }
    }
    /// <summary>
    /// Loads NFT collections and their associated avatars.
    /// </summary>
    public async void LoadCollections()
    {
        ClearCollection();

        await cryptoAvatars.GetNFTCollections(
            (collections) =>
            {
                List<string> options = new List<string>();

                for (int i = 0; i < collections.nftCollections.Length; i++)
                {
                    string name = collections.nftCollections[i].name;

                    var parameters = new Dictionary<string, string>
                    {
                        { "license", "CC0" },
                        {"limit","6" },
                        { "collectionName", name},
                    };
                    GameObject avatarCard = Instantiate(
                        collectionCardPrefab,
                        scrollViewCollections.content.transform
                    );
                    avatarCard
                        .GetComponent<CardCollectionController>()
                        .SetCollectionData(
                            collections.nftCollections[i].name,
                            collections.nftCollections[i].logoImage,
                            async collectionName =>
                                await cryptoAvatars.GetAvatars(LoadAndDisplayAvatars,parameters)
                        );
                    Task task = cryptoAvatars.GetAvatarPreviewImage(
                        collections.nftCollections[i].logoImage,
                        texture =>
                            avatarCard
                                .GetComponent<CardCollectionController>()
                                .LoadCollectionImage(texture)
                    );
                }
            }
        );
    }
    private void OnDisable()
    {
        CancelExistingTasks();
        ClearScrollView();
        ClearCollection();
    }
}