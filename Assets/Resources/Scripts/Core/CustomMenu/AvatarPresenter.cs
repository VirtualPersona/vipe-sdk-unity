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
    public event Action OnHoverStateChanged;

    private Texture2D loadingTexture;
    private Texture2D placeHolder;
    private int creatingAvatarIndex = -1;
    public float rotationAngle = 0f;
    private float scale = 1.0f;

    public AvatarPresenter(CryptoAvatars cryptoAvatars)
    {
        this.cryptoAvatars = cryptoAvatars;
        this.loadingTexture = Resources.Load<Texture2D>("LoadingBar/Materials/spinner_main");
        this.placeHolder = Resources.Load<Texture2D>("Visuals/UI/dummy_pfp");
    }
    public async void OnGuestEnter()
    {
        if (!isLoading && !isLoadingImages)
            await LoadAvatars(new CAModels.SearchAvatarsDto());
    }
    public async void LoadColletionAvatars(string collection)
    {
        if (!isLoading && !isLoadingImages)
            await LoadCollectionAvatars(collection);
    }
    public async void LoadMoreNfts()
    {
        if (!isLoading && !isLoadingImages)
            await cryptoAvatars.NextPage(LoadAndDisplayAvatars);
    }
    public async void LoadPreviousNfts()
    {
        if (!isLoading && !isLoadingImages)
            await cryptoAvatars.PrevPage(LoadAndDisplayAvatars);
    }
    public async void SearchAvatars(string value)
    {
        if (!isLoading && !isLoadingImages)
            await DebounceSearch(value, 500);
    }
    public void LoadImagesIfNeeded()
    {
        lock (lockObject)
        {
            if (shouldLoadImages)
            {
                LoadImages();
            }
        }
    }
    public void RenderUI()
    {
        lock (lockObject)
        {
            if (!HasTextures())
            {
                return;
            }

            scale = EditorGUILayout.Slider("Image Scale", scale, 0.4f, 2.5f);
            float imageWidth = 150.0f * scale;

            float windowWidth = EditorGUIUtility.currentViewWidth;
            float padding = 6.5f;
            float borderWidth = 1;

            float totalImageAndPaddingWidth = imageWidth + padding + borderWidth;
            int cols = Mathf.Max(1, Mathf.FloorToInt((windowWidth - padding - borderWidth) / totalImageAndPaddingWidth));
            int rowCount = CalculateRowCount(cols);
            float availableWidth = windowWidth - (cols * (padding + borderWidth));
            imageWidth = availableWidth / cols;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);

            GUILayout.Space(5);

            for (int y = 0; y < rowCount; y++)
            {
                RenderRow(y, cols, Mathf.RoundToInt(padding), Mathf.RoundToInt(imageWidth));
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private async Task LoadAvatars(CAModels.SearchAvatarsDto searchParams)
    {
        await cryptoAvatars.GetAvatars(searchParams, LoadAndDisplayAvatars);
    }
    private async Task LoadCollectionAvatars(string collection)
    {
        cryptoAvatars.GetAvatarsByCollectionName(collection, LoadAndDisplayAvatars);
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

        lock (lockObject)
        {
            if (shouldLoadImages)
            {
                textures = new Texture2D[nfts.Count];
                for (int i = 0; i < nfts.Count; i++)
                {
                    int index = i;
                    tasks.Add(cryptoAvatars.GetAvatarPreviewImage(nfts[i].metadata.image, texture =>
                    {
                        lock (lockObject)
                        {
                            textures[index] = texture;
                            OnDataChanged?.Invoke();
                            if (index == nfts.Count - 1)
                            {
                                isLoadingImages = false;
                            }
                        }
                    },placeHolder));
                }
                shouldLoadImages = false;
            }
        }

        await Task.WhenAll(tasks);
    }
    
    private bool HasTextures()
    {
        return textures != null && textures.Length > 0;
    }
    private int CalculateRowCount(int cols)
    {
        return Mathf.CeilToInt((float)nfts.Count / cols);
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
    private void RenderRow(int rowIndex, int cols, int padding, int imageWidth)
    {
        EditorGUILayout.BeginHorizontal();

        for (int x = 0; x < cols; x++)
        {
            int index = rowIndex * cols + x;
            RenderTextureButton(index, padding, imageWidth);
        }

        GUILayout.Space(padding);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(padding);
    }
    private void RenderTextureButton(int index, int padding, int imageWidth)
    {
        if (index >= nfts.Count || index >= textures.Length || textures[index] == null)
        {
            return;
        }

        GUILayout.Space(padding);
        Rect buttonRect = GetButtonRect(imageWidth);

        HandleButtonHover(index, ref buttonRect);

        if (GUI.Button(buttonRect, GUIContent.none))
        {
            HandleButtonClick(index);
        }

        if (isLoading && index == creatingAvatarIndex)
        {
            DrawLoadingSpinner(buttonRect, imageWidth);
        }
        else
        {
            GUI.DrawTexture(buttonRect, textures[index]);
        }
    }
    private Rect GetButtonRect(int imageWidth)
    {
        int width = imageWidth;
        int height = imageWidth;
        return GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));
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
        if (buttonRect.Contains(Event.current.mousePosition))
        {
            hoverIndex = index;

            float expandAmount = 8f;
            buttonRect = new Rect(
                buttonRect.x - expandAmount / 2,
                buttonRect.y - expandAmount / 2,
                buttonRect.width + expandAmount,
                buttonRect.height + expandAmount
            );

            float borderWidth = 3f;
            Rect borderRect = new Rect(
                buttonRect.x - borderWidth,
                buttonRect.y - borderWidth,
                buttonRect.width + 2 * borderWidth,
                buttonRect.height + 2 * borderWidth
            );
            GUI.color = new Color(1f, 1f, 1f, 1f);
            GUI.DrawTexture(borderRect, EditorGUIUtility.whiteTexture);

            if (Event.current.type == EventType.Repaint)
            {
                OnHoverStateChanged?.Invoke();
            }
        }
        else if (hoverIndex == index)
        {
            hoverIndex = -1;
        }
    }
    private void DrawLoadingSpinner(Rect buttonRect, int imageWidth)
    {
        GUI.DrawTexture(buttonRect, textures[creatingAvatarIndex]);

        DrawTransparentBackground(buttonRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));

        float spinnerWidth = imageWidth / 2f;
        float spinnerHeight = imageWidth / 2f;
        Rect spinnerRect = new Rect(buttonRect.x + spinnerWidth / 2, buttonRect.y + spinnerHeight / 2, spinnerWidth, spinnerHeight);

        DrawRotatingTexture(spinnerRect);
    }
    private void DrawTransparentBackground(Rect rect, Color backgroundColor)
    {
        GUI.color = backgroundColor;
        GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
        GUI.color = Color.white;
    }
    private void DrawRotatingTexture(Rect rect)
    {
        Matrix4x4 matrixBackup = GUI.matrix;
        GUIUtility.RotateAroundPivot(rotationAngle, rect.center);
        GUI.DrawTexture(rect, loadingTexture);
        GUI.matrix = matrixBackup;
    }
}