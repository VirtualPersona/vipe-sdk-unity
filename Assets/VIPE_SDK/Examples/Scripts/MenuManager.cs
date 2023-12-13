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

        private LoginManager login;

        [SerializeField]
        private RectTransform loginPanel;

        [SerializeField]
        private RectTransform avatarsAndCollectionsPanel;
        private VIPE VIPE;

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
        private TMP_InputField searchField;


        [SerializeField]
        private TMP_InputField urlField;

        [SerializeField]
        private TMP_Text currentPageText;

        [SerializeField]
        private GameObject Avatar;

        public bool LoginPanelOn = true;

        [SerializeField]
        private CustomToggleController ownerButton;


        [SerializeField]
        private GameObject avatarsScrollObject;

        [SerializeField]
        private GameObject collectionsScrollObject;

        private ScrollRect avatarsScroll => avatarsScrollObject.GetComponent<ScrollRect>();
        private ScrollRect collectionsScroll => collectionsScrollObject.GetComponent<ScrollRect>();
        private ToggleGroup avatarToggleGroup => avatarsScroll.GetComponent<ToggleGroup>();
        private ToggleGroup collectionToggleGroup => collectionsScroll.GetComponent<ToggleGroup>();

        private void Awake()
        {
            //debug log of this objects name
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("Multiple instances of MenuManager");
            }

            VIPE = new VIPE();
            login = GetComponent<LoginManager>();


        }
        private void Start()
        {
            nextPageBtn.onClick.AddListener(
                async () =>
                    await VIPE.NextPage(avatarsResult => DisplayAvatars(avatarsResult))
            );
            prevPageBtn.onClick.AddListener(
                async () =>
                    await VIPE.PrevPage(avatarsResult => DisplayAvatars(avatarsResult))
            );
            SetSearchField();
            DisplayCollections();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                avatarsAndCollectionsPanel.gameObject.SetActive(!avatarsAndCollectionsPanel.gameObject.activeSelf);
                messageEnableUI.SetActive(!messageEnableUI.activeSelf);
            }
        }
        /// <summary>
        /// Loads a VRM model using a URL entered by the user.
        /// </summary>
        public void LoadByURL()
        {
            string url = urlField.text;
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
            Vector3 screenPos = LoginPanelOn
                ? loginPanel.GetComponent<RectTransform>().position
                : avatarsAndCollectionsPanel.GetComponent<RectTransform>().position;

            Vector3 hiddenPos = LoginPanelOn
                ? avatarsAndCollectionsPanel.GetComponent<RectTransform>().position
                : loginPanel.GetComponent<RectTransform>().position;

            avatarsAndCollectionsPanel.GetComponent<RectTransform>().position = LoginPanelOn ? screenPos : hiddenPos;
            loginPanel.GetComponent<RectTransform>().position = LoginPanelOn ? hiddenPos : screenPos;

            LoginPanelOn = !LoginPanelOn;
        }
        /// <summary>
        /// Changes the display to avatar panel.
        /// </summary>
        public void LoadAvatarUI()
        {
            if (!LoginPanelOn)
            {
                return;
            }
            Vector3 screenPos = LoginPanelOn
                ? loginPanel.GetComponent<RectTransform>().position
                : avatarsAndCollectionsPanel.GetComponent<RectTransform>().position;

            Vector3 hiddenPos = LoginPanelOn
                ? avatarsAndCollectionsPanel.GetComponent<RectTransform>().position
                : loginPanel.GetComponent<RectTransform>().position;

            avatarsAndCollectionsPanel.GetComponent<RectTransform>().position = LoginPanelOn ? screenPos : hiddenPos;
            loginPanel.GetComponent<RectTransform>().position = LoginPanelOn ? hiddenPos : screenPos;

            LoginPanelOn = false;
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
            VIPE.GetAvatarsByURL(VIPE.AvatarsResource + "/" + login.GetWallet(), DisplayAvatars);
        }

        /// <summary>
        /// Wrapper for loading the user's own VRM models, handles login if the user is not logged in.
        /// </summary>
        public void LoadOwnVRMButtonWrapper()
        {
            if (login.IsLoggedIn)
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
        public void LoadOpenSourceAsync()
        {
            CancelExistingTasks();

            var parameters = new Dictionary<string, string>
            {
                { "license", "CC0" },
                {"limit","6" }
            };
            VIPE.GetAvatars(DisplayAvatars, parameters);
        }
        /// <summary>
        /// Wrapper for loading open-source VRM models.
        /// </summary>
        // public void LoadOpenSourceButtonWrapper()
        // {
        //     //_ = LoadOpenSourceAsync();
        //     LoadOpenSourceAsync();
        // }


        /// <summary>
        /// Removes the Cards from the scroll.
        /// </summary>        
        private void ClearScrollView()
        {
            if (avatarsScroll)
                foreach (Transform child in avatarsScroll.content)
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
            Vector3 avatarPos = Avatar.transform.position;
            Quaternion avatarRot = Avatar.transform.rotation;
            Destroy(Avatar);
            Avatar = model;
            Avatar.name = "AVATAR";
            Avatar.transform.SetPositionAndRotation(avatarPos, avatarRot);
            Avatar.GetComponent<Animator>().runtimeAnimatorController =
                Resources.Load<RuntimeAnimatorController>(
                    "Anims/Animator/ThirdPersonAnimatorController"
                );
            Avatar.AddComponent<ResizeCapsuleCollider>();
            Avatar.AddComponent<ThirdPersonUserControl>();
            Camera.main.GetComponent<OrbitCamera>().targetPosition = Avatar.transform;
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

            try
            {
                Models.Nft[] nfts = onAvatarsResult.nfts;
                currentPageText.text = onAvatarsResult.currentPage.ToString() + " | " + onAvatarsResult.totalPages;

                foreach (var nft in nfts)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    GameObject avatarCard = Instantiate(
                        avatarCardPrefab,
                        avatarsScroll.content.transform
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
        private void ClearCollectionsDisplay()
        {
            if (collectionsScroll)
            {
                foreach (Transform child in collectionsScroll.content.transform)
                {
                    if (child.gameObject.name != "AllCollectionsCard")
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
        }


        /// <summary>
        /// Loads NFT collections and their associated avatars.
        /// </summary>
        public async void DisplayCollections()
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
                            collectionsScroll.content.transform
                        );

                        CardManager cardManager =
                            avatarCard.GetComponent<CardManager>();

                        cardManager.SetCardData(
                                 collections.nftCollections[i].slug,
                                 collections.nftCollections[i].logoImage,
                                 (Action<string>)(async collectionName =>
                                     await VIPE.GetAvatars((Action<Models.NftsArray>)this.DisplayAvatars, parameters))
                             );

                        cardManager.SetToggleGroup(collectionToggleGroup);

                        Task task = VIPE.GetAvatarPreviewImage(
                            collections.nftCollections[i].logoImage,
                            texture => cardManager.LoadCardImage(texture)
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
            ClearCollectionsDisplay();
        }
        private void OnDisable()
        {
            CancelExistingTasks();
            ClearScrollView();
            ClearCollectionsDisplay();
            cts.Cancel();
        }

        public void SetOwnerButtonToggleToTrue()
        {
            ownerButton.OnToggleValueChanged(true);
        }

    }
}