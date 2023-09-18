using CA;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

public class AvatarLoaderGUI : EditorWindow
{
    private AvatarPresenter presenter;
    private CryptoAvatars cryptoAvatars;
    private string searchField = "";
    private Vector2 scrollPosition = Vector2.zero;
    private const string WindowTitle = "Library";
    private const string MenuItemPath = "Crypto Avatars/Avatar Library";

    private List<string> collectionOptions = new List<string>();
    private List<Texture2D> collectionLogos = new List<Texture2D>();

    private Texture2D collectionLogoPlaceHolder;

    private void HandleDataChanged() => Repaint();
    [MenuItem(MenuItemPath)]
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
        await cryptoAvatars.GetNFTCollections(async collections =>
        {
            collectionOptions.Clear();
            collectionLogos.Clear();

            foreach (var collection in collections.nftCollections)
            {
                collectionOptions.Add(collection.name);
                await LoadLogoImage(collection.logoImage);
            }
        });
    }
    private async Task LoadLogoImage(string logoImageURL)
    {
        await cryptoAvatars.GetAvatarPreviewImage(logoImageURL, texture =>
        {
            collectionLogos.Add(texture);
        }, collectionLogoPlaceHolder);
    }
    private void InitializeComponents()
    {
        MainThreadDispatcher mainThreadDispatcher = MainThreadDispatcher.Instance;
        cryptoAvatars = new CryptoAvatars(mainThreadDispatcher);
        presenter = new AvatarPresenter(cryptoAvatars);
        collectionLogoPlaceHolder = Resources.Load<Texture2D>("Visuals/UI/dummy_pfp");
    }
    private void SubscribeToEvents()
    {
        EditorApplication.update += OnEditorUpdate;
        presenter.OnDataChanged += HandleDataChanged;
        presenter.OnVRMModelClicked += LoadVRM;
        cryptoAvatars.modelCreated += SetState;
    }
    private void UnsubscribeFromEvents()
    {
        EditorApplication.update -= OnEditorUpdate;
        presenter.OnDataChanged -= HandleDataChanged;
        presenter.OnVRMModelClicked -= LoadVRM;
        cryptoAvatars.modelCreated -= SetState;
    }
    private void SetState()
    {
        presenter.isLoading = false;
        GUI.color = Color.white;
    }
    private void OnEditorUpdate()
    {
        presenter.LoadImagesIfNeeded();
        UpdateLoadingAnimation();
    }
    private void UpdateLoadingAnimation()
    {
        if (presenter.isLoading)
        {
            presenter.rotationAngle += Time.deltaTime;
            if (presenter.rotationAngle > 360f)
            {
                presenter.rotationAngle -= 360f;
            }
            Repaint();
        }
    }
    private void DrawLoadCC0Button()
    {
        if (GUILayout.Button("Load CC0"))
            presenter.OnGuestEnter();
    }
    private void DrawPaginationControls()
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
        DrawLoadCC0Button();
        searchField = EditorUIHelpers.DrawSearchField(searchField, newSearch => presenter.SearchAvatars(newSearch));
        HorizontalMenuDrawer.DrawHorizontalMenu(ref scrollPosition, collectionOptions, collectionLogos, collection => presenter.LoadColletionAvatars(collection));
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