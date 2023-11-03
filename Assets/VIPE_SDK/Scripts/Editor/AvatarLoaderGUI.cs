using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VIPE_SDK
{
    public class AvatarLoaderGUI : EditorWindow
    {
        private VIPE cryptoAvatars;
        private AvatarPresenter presenter;
        private string searchField = "";
        private Vector2 scrollPosition = Vector2.zero;
        private const string WindowTitle = "Library";

        private List<string> collectionOptions = new List<string>();
        private List<Texture2D> collectionLogos = new List<Texture2D>();

        private Texture2D collectionLogoPlaceHolder;

        private bool isLoadingCollections = true;
        public bool isAvatarsLoading = false;

        private void HandleDataChanged() => Repaint();
        [MenuItem("Tools/VIPE/Avatar Library")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<AvatarLoaderGUI>(WindowTitle);
        }
        private async void LoadData()
        {
            await LoadCollectionsNameList();
        }
        private void InitializeAndSubscribe()
        {
            InitializeComponents();
            SubscribeToEvents();
        }
        private async void LoadVRM(string urlVRM)
        {
            await cryptoAvatars.GetAvatarVRMModel(urlVRM, (model, path) => { });
        }
        private async Task LoadCollectionsNameList()
        {
            isLoadingCollections = true;
            await cryptoAvatars.GetNFTCollections(async collections =>
            {
                collectionOptions.Clear();
                collectionLogos.Clear();
                foreach (var collection in collections.nftCollections)
                {
                    collectionOptions.Add(collection.slug);
                    await LoadLogoImage(collection.logoImage);
                }
                isLoadingCollections = false;
            });
        }
        private async Task LoadLogoImage    (string logoImageURL)
        {
            await cryptoAvatars.GetAvatarPreviewImage(logoImageURL, texture =>
            {
                collectionLogos.Add(texture);
            }, collectionLogoPlaceHolder);
        }
        private void InitializeComponents   ()
        {
            MainThreadDispatcher mainThreadDispatcher = MainThreadDispatcher.Instance;
            cryptoAvatars = new VIPE(mainThreadDispatcher);
            presenter = new AvatarPresenter(cryptoAvatars);
            collectionLogoPlaceHolder = Resources.Load<Texture2D>("Visuals/UI/Icons/dummy_pfp");
        }
        private void SubscribeToEvents      ()
        {
            EditorApplication.update += OnEditorUpdate;
            presenter.OnDataChanged += HandleDataChanged;
            presenter.OnVRMModelClicked += LoadVRM;
            cryptoAvatars.modelCreated += SetState;
        }
        private void UnsubscribeFromEvents  ()
        {
            EditorApplication.update -= OnEditorUpdate;
            presenter.OnDataChanged -= HandleDataChanged;
            presenter.OnVRMModelClicked -= LoadVRM;
            cryptoAvatars.modelCreated -= SetState;
        }
        private void SetState               ()
        {
            presenter.isLoading = false;
            GUI.color = Color.white;
        }
        private void OnEditorUpdate         ()
        {
            presenter.LoadImagesIfNeeded();
            UpdateLoadingAnimation();
        }
        private void UpdateLoadingAnimation ()
        {
            if (presenter.isLoading || presenter.islLoadingCollections)
            {
                presenter.rotationAngle += Time.deltaTime * 100;
                if (presenter.rotationAngle > 360f)
                {
                    presenter.rotationAngle -= 360f;
                }
                Repaint();
            }
        }
        private void DrawLoadCC0Button      ()
        {
            if (GUILayout.Button("Load Open Source"))
                presenter.OnGuestEnter();
        }
        private void DrawLoadOwnerButton    ()
        {
            if (GUILayout.Button("Load Owned"))
                presenter.OnOwnerEnter();
        }
        private void DrawPaginationControls ()
        {
            EditorUIHelpers.DrawPaginationControls(
                () => presenter.LoadPreviousNfts(),
                () => presenter.LoadMoreNfts(),
                presenter.currentPage,
                presenter.totalPages
            );
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            DrawLoadOwnerButton();
            DrawLoadCC0Button();
            GUILayout.EndHorizontal();

            searchField = EditorUIHelpers.DrawSearchField(searchField, newSearch => presenter.SearchAvatars(newSearch), "Find VRM by Name");

            if (isLoadingCollections)
            {
                GUILayout.Label("Loading collections...");
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.Label("CC0 Collection");
                HorizontalMenuDrawer.DrawHorizontalMenu(ref scrollPosition, collectionOptions, collectionLogos, collection => presenter.LoadColletionAvatars(collection), presenter.islLoadingCollections, presenter.loadingTexture, presenter.rotationAngle, presenter.currentlyLoadingCollection);
            }

            DrawPaginationControls();
            presenter.RenderUI();
        }
        private void OnEnable()
        {
            InitializeAndSubscribe();
            LoadData();
        }
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
    }
}