using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Threading;

namespace VIPE_SDK
{
    public class AvatarPresenter
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        public VIPE Vipe;
        private List<Models.Nft> nfts = new List<Models.Nft>();
        private Texture2D[] textures;

        public int CurrentPage;
        public int TotalPages;
        private int hoverIndex = -1;
        private Vector2 scrollPosition;
        private object lockObject = new object();
        private bool shouldLoadImages = false;
        public bool IsLoading = false;
        public bool IsLoadingImages = false;
        public bool IslLoadingCollections = false;
        public event Action OnDataChanged;
        public event Action<string> OnVRMModelClicked;
        public Texture2D LoadingTexture;
        private Texture2D placeHolder;
        private int creatingAvatarIndex = -1;
        public float RotationAngle = 0f;
        private float scale = 1.0f;
        private const float rectExpansion = 8f;
        private const float borderThickness = 3f;
        public string CurrentlyLoadingCollection = null;

        private Dictionary<string, string> parameters = new Dictionary<string, string>();
        private async Task ExecuteLoad(Func<Task> action)
        {
            if (!IsLoading && !IsLoadingImages) await action();
        }
        public AvatarPresenter(VIPE vipe)
        {
            this.Vipe = vipe;
            LoadingTexture = Resources.Load<Texture2D>("LoadingBar/spinner_main");
            placeHolder = Resources.Load<Texture2D>("Visuals/UI/Icons/dummy_pfp");
        }
        public async void OnGuestEnter() => await ExecuteLoad(() => Vipe.GetAvatars(LoadAndDisplayAvatars, Vipe.DefaultQuery("")));
        public async void OnOwnerEnter() => await ExecuteLoad(() => Vipe.GetAvatars(LoadAndDisplayAvatars, null, SecureDataHandler.LoadWallet()));
        public async void LoadColletionAvatars(string collection)
        {
            IslLoadingCollections = true;
            CurrentlyLoadingCollection = collection;
            await ExecuteLoad(() => LoadCollectionAvatars(collection));
            IslLoadingCollections = false;
            CurrentlyLoadingCollection = null;
        }
        public async void LoadMoreNfts() => await ExecuteLoad(() => Vipe.NextPage(LoadAndDisplayAvatars));
        public async void LoadPreviousNfts() => await ExecuteLoad(() => Vipe.PrevPage(LoadAndDisplayAvatars));
        public async void SearchAvatars(string value) => await ExecuteLoad(() => DebounceSearch(value, 500));
        private async Task LoadAvatars(Models.SearchAvatarsDto searchParams) => await Vipe.GetAvatars(LoadAndDisplayAvatars, parameters);
        private async Task LoadCollectionAvatars(string collection) => await Vipe.GetAvatars(LoadAndDisplayAvatars, Vipe.DefaultQuery(collection));
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
        private void LoadAndDisplayAvatars(Models.NftsArray onAvatarsResult)
        {
            lock (lockObject)
            {
                nfts.Clear();
                foreach (var nft in onAvatarsResult.nfts)
                {
                    nfts.Add(nft);
                }
                CurrentPage = onAvatarsResult.currentPage;
                TotalPages = onAvatarsResult.totalPages;
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
                GUI.DrawTexture(buttonRect, textures[index]);

                if (IsLoading && index == creatingAvatarIndex)
                {
                    EditorUIHelpers.DrawLoadingSpinner(buttonRect, LoadingTexture, RotationAngle, 10);
                }
            }
        }
        public void HandleButtonClick(int index)
        {
            if (IsLoading) return;

            IsLoading = true;
            creatingAvatarIndex = index;
            OnVRMModelClicked?.Invoke(nfts[index].metadata.asset);
        }
        private void HandleButtonHover(int index, ref Rect buttonRect)
        {
            bool isHovering = buttonRect.Contains(Event.current.mousePosition);

            if (isHovering)
            {
                hoverIndex = index;
                buttonRect = EditorUIHelpers.ExpandRect(buttonRect, rectExpansion);
                EditorUIHelpers.DrawBorder(buttonRect, borderThickness, Color.white);
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
            Models.SearchAvatarsDto searchAvatar = new Models.SearchAvatarsDto { name = value };
            parameters = new Dictionary<string, string>
        {
            { "name", value },
            {"license","CC0" }
        };
            await Task.Run(() => MainThreadDispatcher.RunOnMainThread(async () => await LoadAvatars(searchAvatar)), cts.Token);
        }
        private async Task LoadImages()
        {
            List<Task> tasks = new List<Task>();

            if (shouldLoadImages)
            {
                textures = new Texture2D[nfts.Count];

                for (int i = 0; i < textures.Length; i++)
                {
                    textures[i] = placeHolder;
                }

                for (int i = 0; i < nfts.Count; i++)
                {
                    int index = i;
                    tasks.Add(Vipe.GetAvatarPreviewImage(nfts[i].metadata.image, texture =>
                    {
                        textures[index] = texture;
                        OnDataChanged?.Invoke();
                        if (index == nfts.Count - 1)
                        {
                            IsLoadingImages = false;
                        }

                    }, placeHolder));
                }
                shouldLoadImages = false;
            }

            await Task.WhenAll(tasks);
        }
    }
}