using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

namespace VIPE_SDK
{
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
        private VIPE VIPE;

        [SerializeField]
        private Button loadVRM;

        [SerializeField]
        private Button nextPageBtn;

        [SerializeField]
        private Button prevPageBtn;

        [SerializeField]
        private GameObject messageEnableUI;

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

        public bool loginPanelOn = true;

        [SerializeField]
        private CustomToggleController ownerButton;

        [SerializeField]
        private GameObject loadingBar;

        private ToggleGroup avatarToggleGroup;
        private ToggleGroup collectionToggleGroup;

        private void Awake()
        {
            //debug log of this objects name
            Debug.Log(this.gameObject.name);
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("Multiple instances of MenuManager");
            }

            VIPE = new VIPE();
            login = GetComponent<Login>();

            avatarToggleGroup = new GameObject("ToggleGroup").AddComponent<ToggleGroup>();
            collectionToggleGroup = new GameObject("ToggleGroup").AddComponent<ToggleGroup>();
        }
        private void Start()
        {
            vrm = GameObject.Find("AVATAR");

            nextPageBtn.onClick.AddListener(
                async () =>
                    await VIPE.NextPage(avatarsResult => DisplayAvatars(avatarsResult))
            );
            prevPageBtn.onClick.AddListener(
                async () =>
                    await VIPE.PrevPage(avatarsResult => DisplayAvatars(avatarsResult))
            );
            SetSearchField();
            LoadCollections();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                avatarsPanel.gameObject.SetActive(!avatarsPanel.gameObject.activeSelf);
                messageEnableUI.SetActive(!messageEnableUI.activeSelf);
            }
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
        /// //NOT USED..consider deleting
        public void ToggleAvatarUI()
        {
            Vector3 screenPos = loginPanelOn
                ? loginPanel.GetComponent<RectTransform>().position
                : avatarsPanel.GetComponent<RectTransform>().position;

            Vector3 hiddenPos = loginPanelOn
                ? avatarsPanel.GetComponent<RectTransform>().position
                : loginPanel.GetComponent<RectTransform>().position;

            avatarsPanel.GetComponent<RectTransform>().position = loginPanelOn ? screenPos : hiddenPos;
            loginPanel.GetComponent<RectTransform>().position = loginPanelOn ? hiddenPos : screenPos;

            loginPanelOn = !loginPanelOn;
        }
        /// <summary>
        /// Changes the display to avatar panel.
        /// </summary>
        public void LoadAvatarUI()
        {
            if (!loginPanelOn)
            {
                return;
            }
            Vector3 screenPos = loginPanelOn
                ? loginPanel.GetComponent<RectTransform>().position
                : avatarsPanel.GetComponent<RectTransform>().position;

            Vector3 hiddenPos = loginPanelOn
                ? avatarsPanel.GetComponent<RectTransform>().position
                : loginPanel.GetComponent<RectTransform>().position;

            avatarsPanel.GetComponent<RectTransform>().position = loginPanelOn ? screenPos : hiddenPos;
            loginPanel.GetComponent<RectTransform>().position = loginPanelOn ? hiddenPos : screenPos;

            loginPanelOn = false;
        }
        /// <summary>
        /// Sets up the search field to execute a search query when the text changes.
        /// </summary>
        public void SetSearchField()
        {
            ClearScrollView();
            searchField.onValueChanged.AddListener(async (value) => await DebounceSearch(value, 500));
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
                {"name", value },
                {"license","CC0" },
                {"limit","6" }
            };
            Models.SearchAvatarsDto searchAvatar = new() { name = value };
            VIPE.GetAvatars(DisplayAvatars, parameters);
        }
        /// <summary>
        /// Loads the user's own VRM models asynchronously.
        /// </summary>
        public async Task LoadOwnVRMAsync()
        {
            CancelExistingTasks();
            ClearScrollView();

            Models.SearchAvatarsDto searchAvatar = new() { };
            VIPE.GetAvatarsByURL(VIPE.avatarsResource + "/" + login.GetWallet(), DisplayAvatars);
        }

        /// <summary>
        /// Wrapper for loading the user's own VRM models, handles login if the user is not logged in.
        /// </summary>
        public void LoadOwnVRMButtonWrapper()
        {
            if (login.isLoggedIn)
            {
                LoadOwnVRMAsync();
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
            // ClearScrollView();

            var parameters = new Dictionary<string, string>
        {
            { "license", "CC0" },
            {"limit","6" }
        };

            //Action wrapperAction = async () => await VIPE.GetAvatars(LoadAndDisplayAvatars, parameters);

            //await Task.Run(() => MainThreadDispatcher.RunOnMainThread(wrapperAction));

            await VIPE.GetAvatars(DisplayAvatars, parameters);
        }
        /// <summary>
        /// Wrapper for loading open-source VRM models.
        /// </summary>
        public void LoadOpenSourceButtonWrapper()
        {
            //_ = LoadOpenSourceAsync();
            LoadOpenSourceAsync();
        }


        /// <summary>
        /// Removes the Cards from the scroll.
        /// </summary>        
        private void ClearScrollView()
        {
            if (scrollViewAvatars)
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

        /// <summary>
        /// Displays a list of avatars in the scroll view.
        /// </summary>
        /// <param name="onAvatarsResult">The data containing avatars to display.</param>
        private async void DisplayAvatars(Models.NftsArray onAvatarsResult)
        {
            ClearScrollView();
            await Task.Delay(1);
            //timeout
            DisplayLoadingBar();

            try
            {
                Models.Nft[] nfts = onAvatarsResult.nfts;
                currentPageText.text = onAvatarsResult.currentPage.ToString() + " | " + onAvatarsResult.totalPages;

                foreach (var nft in nfts)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    GameObject avatarCard = Instantiate(
                        avatarCardPrefab,
                        scrollViewAvatars.content.transform
                    );

                    CardManager cardManager =
                        avatarCard.GetComponent<CardManager>();

                    cardManager.SetCardData(
                        nft.metadata.name,
                        nft.metadata.asset,
                        urlVRM => LoadVRMModel(urlVRM)
                    );

                    cardManager.SetToggleGroup(avatarToggleGroup);

                    VIPE.GetAvatarPreviewImage(
                        nft.metadata.image,
                        texture => cardManager.LoadCardImage(texture)
                    );
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Operation canceled in DisplayAvatars");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error displaying avatars: " + ex.Message);
            }

            HideLoadingBar();
        }
        /// <summary>
        /// Loads a VRM model based on a URL.
        /// </summary>
        /// <param name="urlVRM">The URL of the VRM model to load.</param>
        private async void LoadVRMModel(string urlVRM)
        {
            loadingSpinner.SetActive(true);
            await VIPE.GetAvatarVRMModel(
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
            await VIPE.NextPage(onAvatarsResult => DisplayAvatars(onAvatarsResult));
        }
        /// <summary>
        /// Loads previous avatars when the "Load Previous" button is clicked.
        /// </summary>
        public async void LoadPreviousNfts()
        {
            await VIPE.PrevPage(onAvatarsResult => DisplayAvatars(onAvatarsResult));
        }
        /// <summary>
        /// Clears the collection scroll view by destroying its child objects.
        /// </summary>
        private void ClearCollection()
        {
            if (scrollViewCollections)
                foreach (Transform child in scrollViewCollections.content.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
        }
        /// <summary>
        /// Loads NFT collections and their associated avatars.
        /// </summary>
        public async void LoadCollections()
        {
            try
            {
                await VIPE.GetNFTCollections((Action<Models.NftCollectionsArray>)(async (collections) =>
                {
                    List<string> options = new List<string>();

                    for (int i = 0; i < collections.nftCollections.Length; i++)
                    {
                        //State of the task
                        //cts.Token.ThrowIfCancellationRequested();

                        string slug = collections.nftCollections[i].slug;

                        var parameters = new Dictionary<string, string>
                        {
                            {"license", "CC0" },
                            {"limit","6" },
                            {"collectionSlug", slug},
                        };

                        GameObject avatarCard = Instantiate(
                            collectionCardPrefab,
                            scrollViewCollections.content.transform
                        );

                        avatarCard
                            .GetComponent<CardManager>()
                            .SetCardData(
                                collections.nftCollections[i].slug,
                                collections.nftCollections[i].logoImage,
                                (Action<string>)(async collectionName =>
                                    await VIPE.GetAvatars((Action<Models.NftsArray>)this.DisplayAvatars, parameters))
                            );

                        Task task = VIPE.GetAvatarPreviewImage(
                            collections.nftCollections[i].logoImage,
                            texture =>
                                avatarCard
                                    .GetComponent<CardManager>()
                                    .LoadCardImage(texture)
                        );
                    }
                }));
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Operaci�n cancelada");
            }
        }
        private void OnEnable()
        {
            cts = new CancellationTokenSource();
            ClearScrollView();
            ClearCollection();
        }
        private void OnDisable()
        {
            CancelExistingTasks();
            ClearScrollView();
            ClearCollection();
            cts.Cancel();
        }

        public void SetOwnerButtonToggleToTrue()
        {
            ownerButton.OnToggleValueChanged(true);
        }

        public void DisplayLoadingBar()
        {
            Debug.Log("displaying");
            loadingBar.SetActive(true);
        }

        public void HideLoadingBar()
        {
            Debug.Log("hiding");
            loadingBar.SetActive(false);
        }
    }
}