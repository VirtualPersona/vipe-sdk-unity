using CA;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Threading;

public class AvatarPresenter
{
    private CancellationTokenSource cts = new CancellationTokenSource();
    public CryptoAvatars cryptoAvatars;
    private List<CAModels.Nft> nfts = new List<CAModels.Nft>();
    private Texture2D[] textures;

    public int currentPage;
    public int totalPages;
    private int hoverIndex = -1;
    private Vector2 scrollPosition;
    private object lockObject = new object();

    private bool shouldLoadImages = false;
    public bool isLoading = false;
    public bool isLoadingImages = false;

    public event Action OnDataChanged;
    public event Action<string> OnVRMModelClicked;

    private Texture2D loadingTexture;
    private Texture2D placeHolder;
    private int creatingAvatarIndex = -1;
    public float rotationAngle = 0f;
    private float scale = 1.0f;
    private const float RectExpansion = 8f;
    private const float BorderThickness = 3f;

    private async Task ExecuteLoad(Func<Task> action)
    {
        if (!isLoading && !isLoadingImages) await action();
    }
    public AvatarPresenter(CryptoAvatars cryptoAvatars)
    {
        this.cryptoAvatars = cryptoAvatars;
        loadingTexture = Resources.Load<Texture2D>("LoadingBar/Materials/spinner_main");
        placeHolder = Resources.Load<Texture2D>("Visuals/UI/dummy_pfp");
    }
    public async void OnGuestEnter() => await ExecuteLoad(() => LoadAvatars(new CAModels.SearchAvatarsDto()));
    public async void LoadColletionAvatars(string collection) => await ExecuteLoad(() => LoadCollectionAvatars(collection));
    public async void LoadMoreNfts() => await ExecuteLoad(() => cryptoAvatars.NextPage(LoadAndDisplayAvatars));
    public async void LoadPreviousNfts() => await ExecuteLoad(() => cryptoAvatars.PrevPage(LoadAndDisplayAvatars));
    public async void SearchAvatars(string value) => await ExecuteLoad(() => DebounceSearch(value, 500));
    private async Task LoadAvatars(CAModels.SearchAvatarsDto searchParams) => await cryptoAvatars.GetAvatars(LoadAndDisplayAvatars,searchParams.name);
    private async Task LoadCollectionAvatars(string collection) => await cryptoAvatars.GetAvatars(LoadAndDisplayAvatars,collection);
    private bool HasTextures() => textures != null && textures.Length > 0;
    public async void LoadImagesIfNeeded()
    {
        if (shouldLoadImages) await LoadImages();
    }
    public void RenderUI()
    {
        lock (lockObject)
        {
            if (!HasTextures()) return;

            scale = EditorGUILayout.Slider("Image Scale", scale, 0.4f, 2.5f);
            float imageWidth = 150.0f * scale;

            float windowWidth = EditorGUIUtility.currentViewWidth;
            float padding = 6.5f;
            float borderWidth = 1;

            (int cols, int rowCount) = EditorUIHelpers.CalculateGridDimensions(windowWidth, padding, borderWidth, imageWidth);

            float availableWidth = windowWidth - (cols * (padding + borderWidth));
            imageWidth = availableWidth / cols;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);

            GUILayout.Space(padding);

            int actualNftCount = nfts.Count;

            for (int y = 0; y < rowCount; y++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < cols; x++)
                {
                    int index = y * cols + x;

                    if (index < actualNftCount)
                    {
                        RenderTextureButton(index, Mathf.RoundToInt(padding), Mathf.RoundToInt(imageWidth));
                    }
                    else
                    {
                        GUILayout.Space(Mathf.RoundToInt(padding + imageWidth));
                    }
                }

                GUILayout.Space(padding);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(padding);
            }

            EditorGUILayout.EndScrollView();
        }
    }
    private void LoadAndDisplayAvatars(CAModels.NftsArray onAvatarsResult)
    {
        lock (lockObject)
        {
            nfts.Clear();
            foreach (var nft in onAvatarsResult.nfts)
            {
                nfts.Add(nft);
            }
            currentPage = onAvatarsResult.currentPage;
            totalPages = onAvatarsResult.totalPages;
            shouldLoadImages = true;
        }
    }
    private void RenderTextureButton(int index, int padding, int imageWidth)
    {
        GUILayout.Space(padding);
        Rect buttonRect = GUILayoutUtility.GetRect(imageWidth, imageWidth, GUILayout.Width(imageWidth), GUILayout.Height(imageWidth));
        HandleButtonHover(index, ref buttonRect);
        if (GUI.Button(buttonRect, GUIContent.none))
            HandleButtonClick(index);

        DrawButtonContent(index, buttonRect);
    }
    private void DrawButtonContent(int index, Rect buttonRect)
    {
        if (textures != null && index >= 0 && index < textures.Length)
        {
            if (isLoading && index == creatingAvatarIndex)
            {
                EditorUIHelpers.DrawLoadingSpinner(buttonRect, loadingTexture, rotationAngle);
            }
            else if (textures[index] != null)
            {
                GUI.DrawTexture(buttonRect, textures[index]);
            }
        }
    }
    public void HandleButtonClick(int index)
    {
        if (isLoading) return;

        isLoading = true;
        creatingAvatarIndex = index;
        OnVRMModelClicked?.Invoke(nfts[index].metadata.asset);
    }
    private void HandleButtonHover(int index, ref Rect buttonRect)
    {
        bool isHovering = buttonRect.Contains(Event.current.mousePosition);

        if (isHovering)
        {
            hoverIndex = index;
            buttonRect = EditorUIHelpers.ExpandRect(buttonRect, RectExpansion);
            EditorUIHelpers.DrawBorder(buttonRect, BorderThickness, Color.white);
        }
        else if (hoverIndex == index)
        {
            hoverIndex = -1;
        }
    }
    private async Task DebounceSearch(string value, int delayMilliseconds)
    {
        cts.Cancel();
        cts = new CancellationTokenSource();

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
    private async Task ExecuteSearch(string value)
    {
        CAModels.SearchAvatarsDto searchAvatar = new CAModels.SearchAvatarsDto { name = value };
        await Task.Run(() => MainThreadDispatcher.RunOnMainThread(async () => await LoadAvatars(searchAvatar)), cts.Token);
    }
    private async Task LoadImages()
    {
        List<Task> tasks = new List<Task>();

        if (shouldLoadImages)
        {
            textures = new Texture2D[nfts.Count];
            for (int i = 0; i < nfts.Count; i++)
            {
                int index = i;
                tasks.Add(cryptoAvatars.GetAvatarPreviewImage(nfts[i].metadata.image, texture =>
                {
                    textures[index] = texture;
                    OnDataChanged?.Invoke();
                    if (index == nfts.Count - 1)
                    {
                        isLoadingImages = false;
                    }

                }, placeHolder));
            }
            shouldLoadImages = false;

        }

        await Task.WhenAll(tasks);
    }
}