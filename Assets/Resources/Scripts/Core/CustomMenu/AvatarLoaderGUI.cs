using CA;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class AvatarLoaderGUI : EditorWindow
{
    private AvatarPresenter presenter;
    private CryptoAvatars cryptoAvatars;
    private string searchField = "";

    private const string WindowTitle = "Library";
    private const string MenuItemPath = "Crypto Avatars/Avatar Library";

    private List<string> collectionOptions = new List<string>();
    private int selectedCollectionIndex = 0;
    private Vector2 scrollPosition;

    private List<Texture2D> collectionLogos = new List<Texture2D>();

    private Texture2D collectionLogoPlaceHolder;

    [MenuItem(MenuItemPath)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<AvatarLoaderGUI>(WindowTitle);
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
        EditorApplication.update += EditorUpdate;
        EditorApplication.update += OnEditorUpdate;
        SubscribeToPresenterEvents();
        cryptoAvatars.modelCreated += SetState;
    }
    private void SubscribeToPresenterEvents()
    {
        presenter.OnDataChanged += HandleDataChanged;
        presenter.OnVRMModelClicked += LoadVRM;
        presenter.OnHoverStateChanged += () => Repaint();
    }
    private void UnsubscribeFromEvents()
    {
        EditorApplication.update -= EditorUpdate;
        EditorApplication.update -= OnEditorUpdate;
        UnsubscribeFromPresenterEvents();
        cryptoAvatars.modelCreated -= SetState;
    }
    private void UnsubscribeFromPresenterEvents()
    {
        presenter.OnDataChanged -= HandleDataChanged;
        presenter.OnVRMModelClicked -= LoadVRM;
        presenter.OnHoverStateChanged -= () => Repaint();
    }
    private void SetState()
    {
        presenter.isLoading = false;
        GUI.color = Color.white;
    }
    private void OnEditorUpdate()
    {
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
    private async void LoadVRM(string urlVRM)
    {
        await cryptoAvatars.GetAvatarVRMModel(urlVRM, (model, path) => { });
    }
    private void HandleDataChanged()
    {
        Repaint();
    }
    private void EditorUpdate()
    {
        presenter.LoadImagesIfNeeded();
    }
    private void DrawLoadCC0Button()
    {
        if (GUILayout.Button("Load CC0"))
        {
            presenter.OnGuestEnter();
            presenter.isLoadingImages = true;
        }
    }
    private void DrawPaginationControls()
    {
        if (presenter.cryptoAvatars.nextPageUrl == null && presenter.cryptoAvatars.prevPageUrl == null)
        {
            return;
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous Page"))
        {
            presenter.LoadPreviousNfts();
            presenter.isLoadingImages = true;
        }
        DrawPageInfo();
        if (GUILayout.Button("Next Page"))
        {
            presenter.LoadMoreNfts();
            presenter.isLoadingImages = true;
        }
        EditorGUILayout.EndHorizontal();
    }
    private void DrawPageInfo()
    {
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.FlexibleSpace();
        GUILayout.Label($"{presenter.currentPage} | {presenter.totalPages}", centeredStyle);
        GUILayout.FlexibleSpace();
    }
    private void DrawSearchField()
    {
        string newSearchField = EditorGUILayout.TextField("Search Avatars:", searchField);
        if (newSearchField != searchField)
        {
            searchField = newSearchField;
            presenter.SearchAvatars(searchField);
        }
        GUILayout.Space(10);
    }
    private void DrawUI()
    {
        presenter.RenderUI();
    }

    //DrawHorizontalCarousel
    public async void LoadCollectionsNameList()
    {
        await cryptoAvatars.GetNFTCollections(async (collections) =>
        {
            collectionOptions.Clear();
            collectionLogos.Clear();

            for (int i = 0; i < collections.nftCollections.Length; i++)
            {
                collectionOptions.Add(collections.nftCollections[i].name);
                string logoImageURL = collections.nftCollections[i].logoImage;

                await cryptoAvatars.GetAvatarPreviewImage(logoImageURL, (texture) =>
                {
                    collectionLogos.Add(texture);
                }, collectionLogoPlaceHolder);
            }
        });
    }
    private void DrawHorizontalCarousel()
    {
        if (collectionOptions.Count == 0 || collectionLogos.Count == 0)
        {
            GUILayout.Label("Loading collections...");
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Height(140));
        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < collectionOptions.Count; i++)
        {
            DrawCollectionBox(i);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }
    private void DrawCollectionBox(int index)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(140), GUILayout.Height(140));

        DrawPaddedArea(() => DrawCollectionLogo(index), 20);
        DrawPaddedArea(() => DrawCollectionLabel(index), 10);

        EditorGUILayout.EndVertical();
    }
    private void DrawPaddedArea(Action content, int padding)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(padding);
        content();
        GUILayout.Space(padding);
        EditorGUILayout.EndHorizontal();
    }
    private void DrawCollectionLogo(int index)
    {
        Rect logoRect = GUILayoutUtility.GetRect(100, 100);

        if (index < collectionLogos.Count && collectionLogos[index] != null)
        {
            GUI.DrawTexture(logoRect, collectionLogos[index]);
        }

        HandleClickOnRect(logoRect, index);
    }
    private void DrawCollectionLabel(int index)
    {
        GUIStyle style = new GUIStyle() { alignment = TextAnchor.UpperCenter };
        style.normal.textColor = Color.white;
        GUILayout.Label(collectionOptions[index], style);
    }
    private void HandleClickOnRect(Rect rect, int index)
    {
        if (rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                selectedCollectionIndex = index;
                presenter.LoadColletionAvatars(collectionOptions[selectedCollectionIndex]);
            }
        }
    }
    //----
    private void OnGUI()
    {
        DrawLoadCC0Button();
        DrawPaginationControls();
        DrawSearchField();
        DrawHorizontalCarousel();
        DrawUI();
    }
    private void OnEnable()
    {
        InitializeComponents();
        SubscribeToEvents();
        LoadCollectionsNameList();
    }
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
}