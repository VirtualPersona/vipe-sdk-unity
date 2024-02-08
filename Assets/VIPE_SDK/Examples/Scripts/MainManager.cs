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
    public class MainManager : MonoBehaviour
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        public static MainManager Instance;

        private LoginManager login;
        public static CollectionManager collectionManager;
        public static AvatarManager avatarManager;
        public static VRMManager vrmManager;

        [SerializeField]
        private RectTransform loginPanel;

        [SerializeField]
        private RectTransform avatarsAndCollectionsPanel;
        public static VIPE VIPE;

        [SerializeField]
        private Button nextPageBtn;

        [SerializeField]
        private Button prevPageBtn;

        [SerializeField]
        private GameObject messageEnableUI;

        [SerializeField]
        private TMP_InputField searchField;


        [SerializeField]
        private TMP_InputField urlField;

        public bool LoginPanelOn = true;

        [SerializeField]
        private CustomToggleController ownerButton;


        private bool isOwnedSearch = false;

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

            VIPE = new VIPE();
            login = GetComponent<LoginManager>();
            collectionManager = GetComponent<CollectionManager>();
            avatarManager = GetComponent<AvatarManager>();
            vrmManager = GetComponent<VRMManager>();

        }

        private void Start()
        {
            nextPageBtn.onClick.AddListener(
                async () =>
                    await VIPE.NextPage(avatarsResult => avatarManager.DisplayAvatars(avatarsResult, cts))
            );
            prevPageBtn.onClick.AddListener(
                async () =>
                    await VIPE.PrevPage(avatarsResult => avatarManager.DisplayAvatars(avatarsResult, cts))
            );
            SetSearchField();
            Debug.Log("Start with cts " + cts.IsCancellationRequested);
            collectionManager.DisplayCollections(cts);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                avatarsAndCollectionsPanel.gameObject.SetActive(!avatarsAndCollectionsPanel.gameObject.activeSelf);
                messageEnableUI.SetActive(!messageEnableUI.activeSelf);
            }
            //print cancellationTOken every 1 seconds
            // if (Time.frameCount % 600 == 0)
            // {
            //     Debug.Log("Update with cts " + cts.IsCancellationRequested);
            // }

        }

        /// <summary> Loads a VRM model using a URL entered by the user. </summary>
        public void LoadByURL()
        {
            string url = urlField.text;
            if (url != "")
            {
                vrmManager.LoadVRMModel(url);
            }
        }
        /// <summary>Cancels any existing asynchronous tasks.</summary>
        private void CancelExistingTasks()
        {
            Debug.Log("CancelExistingTasks");
            cts.Cancel();
            cts = new CancellationTokenSource();
        }



        /// <summary>Changes the display to avatar panel. </summary>
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

        /// <summary>Sets up the search field to execute a search query when the text changes.</summary>
        public void SetSearchField()
        {
            avatarManager.ClearScrollView();
            searchField.onValueChanged.AddListener(async (value) => await DebounceSearch(value, 500));
        }

        /// <summary>Delays the execution of a search query and performs it after a specified delay.</summary>
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

        /// <summary>Executes a search query to load avatars based on the provided search criteria.</summary>
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
            VIPE.GetAvatars(CallDisplayAvatarsWithCts, parameters);
        }

        private void CallDisplayAvatarsWithCts(Models.NftsArray avatarsResult)
        {
            Debug.Log("CallDisplayAvatarsWithCts " + cts.IsCancellationRequested);
            avatarManager.DisplayAvatars(avatarsResult, cts);
        }

        /// <summary>Loads the user's own VRM models asynchronously.</summary>
        public async Task LoadOwnedAvatarsAsync()
        {
            CancelExistingTasks();
            avatarManager.ClearScrollView();

            Models.SearchAvatarsDto searchAvatar = new() { };
            VIPE.GetAvatarsByURL(VIPE.AvatarsResource + "/" + login.GetWallet(), CallDisplayAvatarsWithCts);
        }

        /// <summary>Wrapper for loading the user's own VRM models, handles login if the user is not logged in.</summary>
        public void LoadOwnedAvatarsButtonWrapper()
        {
            if (login.IsLoggedIn)
            {
                LoadOwnedAvatarsAsync();
            }
            else
            {
                login.OnLoginButtonClick();
            }
        }

        /// <summary> Loads open-source VRM models asynchronously.</summary>
        public void LoadOpenSourceAsync()
        {
            Debug.Log("LoadOpenSourceAsync");
            CancelExistingTasks();

            var parameters = new Dictionary<string, string>
            {
                { "license", "CC0" },
                {"limit","6" }
            };
            VIPE.GetAvatars(CallDisplayAvatarsWithCts, parameters);
        }



        /// <summary>Loads more avatars when the "Load More" button is clicked.</summary>
        public async void LoadMoreNfts()
        {
            await VIPE.NextPage(CallDisplayAvatarsWithCts);
        }
        /// <summary>
        /// Loads previous avatars when the "Load Previous" button is clicked.
        /// </summary>
        public async void LoadPreviousNfts()
        {
            await VIPE.PrevPage(CallDisplayAvatarsWithCts);
        }

        // private void OnEnable()
        // {
        //     cts = new CancellationTokenSource();
        //     avatarManager.ClearScrollView();
        //     collectionManager.ClearCollectionsDisplay();
        // }
        // private void OnDisable()
        // {
        //     CancelExistingTasks();
        //     avatarManager.ClearScrollView();
        //     collectionManager.ClearCollectionsDisplay();
        //     cts.Cancel();
        // }

        public void SetOwnerButtonToggleToTrue()
        {
            ownerButton.OnToggleValueChanged(true);
        }

    }
}