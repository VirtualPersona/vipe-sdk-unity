using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VIPE_SDK
{
    public class AvatarLoaderGUI : EditorWindow
    {
        private VIPE vipe;
        private AvatarPresenter presenter;
        private string searchField = "";
        private Vector2 scrollPosition = Vector2.zero;
        private const string windowTitle = "Library";

        private List<string> collectionOptions = new List<string>();
        private List<Texture2D> collectionLogos = new List<Texture2D>();

        private Texture2D collectionLogoPlaceHolder;

        private bool isLoadingCollections = true;
        public bool IsAvatarsLoading = false;

        private void HandleDataChanged() => Repaint();
        [MenuItem("Tools/VIPE/Avatar Library")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<AvatarLoaderGUI>(windowTitle);
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
            await vipe.GetAvatarVRMModel(urlVRM, (model, path) => { });
        }
        private async Task LoadCollectionsNameList()
        {
            isLoadingCollections = true;
            await vipe.GetNFTCollections(async collections =>
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
        private async Task LoadLogoImage(string logoImageURL)
        {
            await vipe.GetAvatarPreviewImage(logoImageURL, texture =>
            {
                collectionLogos.Add(texture);
            }, collectionLogoPlaceHolder);
        }
        private void InitializeComponents()
        {
            vipe = new VIPE();
            presenter = new AvatarPresenter(vipe);
            collectionLogoPlaceHolder = Resources.Load<Texture2D>("Visuals/UI/Icons/dummy_pfp");
        }
        private void SubscribeToEvents()
        {
            EditorApplication.update += OnEditorUpdate;
            presenter.OnDataChanged += HandleDataChanged;
            presenter.OnVRMModelClicked += LoadVRM;
            vipe.ModelCreated += SetState;
        }
        private void UnsubscribeFromEvents()
        {
            EditorApplication.update -= OnEditorUpdate;
            presenter.OnDataChanged -= HandleDataChanged;
            presenter.OnVRMModelClicked -= LoadVRM;
            vipe.ModelCreated -= SetState;
        }
        private void SetState()
        {
            presenter.IsLoading = false;
            GUI.color = Color.white;
        }
        private void OnEditorUpdate()
        {
            presenter.LoadImagesIfNeeded();
            UpdateLoadingAnimation();
        }
        private void UpdateLoadingAnimation()
        {
            if (presenter.IsLoading || presenter.IslLoadingCollections)
            {
                presenter.RotationAngle += Time.deltaTime * 100;
                if (presenter.RotationAngle > 360f)
                {
                    presenter.RotationAngle -= 360f;
                }
                Repaint();
            }
        }
        private void DrawLoadCC0Button()
        {
            if (GUILayout.Button("Load Open Source"))
                presenter.OnGuestEnter();
        }
        private void DrawLoadOwnerButton()
        {
            if (GUILayout.Button("Load Owned"))
                presenter.OnOwnerEnter();
        }
        private void DrawPaginationControls()
        {
            EditorUIHelpers.DrawPaginationControls(
                () => presenter.LoadPreviousNfts(),
                () => presenter.LoadMoreNfts(),
                presenter.CurrentPage,
                presenter.TotalPages
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
                HorizontalMenuDrawer.DrawHorizontalMenu(ref scrollPosition, collectionOptions, collectionLogos, collection => presenter.LoadColletionAvatars(collection), presenter.IslLoadingCollections, presenter.LoadingTexture, presenter.RotationAngle, presenter.CurrentlyLoadingCollection);
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