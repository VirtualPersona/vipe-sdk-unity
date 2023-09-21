using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using System.Collections.Generic;
using VRM;

namespace CA
{
    public class CryptoAvatars
    {
        private static readonly string avatarsResource = "/nfts/avatars";
        private static readonly string collectionsResource = "/collections?containsCC0Nfts=true";

        public MainThreadDispatcher mainThreadDispatcher;
        private CAModels.SearchAvatarsDto searchAvatarsDto;

        public event Action modelCreated;

        public string nextPageUrl;
        public string prevPageUrl;

        public CryptoAvatars(MainThreadDispatcher dispatcher)
        {
            mainThreadDispatcher = dispatcher;
            HttpService.apiKey = ApiKeyManager.GetApiKey();
            HttpService.baseUri = "https://api.cryptoavatars.io/v1";
        }
        private GameObject ImportVRM(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (path.StartsWithUnityAssetPath())
            {
                Debug.LogWarningFormat("disallow import from folder under the Assets");
                return null;
            }
            var data = new GlbFileParser(path).Parse();
            var vrm = new VRMData(data);
            using var context = new VRMImporterContext(vrm);
            var loaded = context.Load();
            loaded.EnableUpdateWhenOffscreen();
            loaded.ShowMeshes();
            data.Dispose();
            return loaded.gameObject;
        }
        private void SetPaginationData(string next, string prev)
        {
            nextPageUrl = next?.Split("/v1")[1];
            prevPageUrl = prev?.Split("/v1")[1];
        }
        private bool IsSupportedFormat(byte[] imageBytes)
        {
            return Utility.IsPng(imageBytes) || Utility.IsJpg(imageBytes);
        }
        public async Task GetAvatars(Action<CAModels.NftsArray> onAvatarsResult, string collectionName = null, string owner = null)
        {
            var searchAvatarsDto = new CAModels.SearchAvatarsDto { collectionName = collectionName, owner = owner };
            await GetAvatarsByURL(avatarsResource, searchAvatarsDto, onAvatarsResult);
        }
        public async Task PrevPage(Action<CAModels.NftsArray> onAvatarsResult)
        {
            if (prevPageUrl != null)
                await GetAvatarsByURL(prevPageUrl, searchAvatarsDto, onAvatarsResult);
        }
        public async Task NextPage(Action<CAModels.NftsArray> onAvatarsResult)
        {
            if (nextPageUrl != null)
                await GetAvatarsByURL(nextPageUrl, searchAvatarsDto, onAvatarsResult);
        }
        public async Task GetAvatarPreviewImage(string imageUrl, Action<Texture2D> onImageLoaded, Texture2D placeholderTexture = null)
        {
            try
            {
                Texture2D texture = await HttpService.Instance().DownloadImageAsync(imageUrl);
                texture = IsSupportedFormat(texture.EncodeToPNG()) ? texture : placeholderTexture;
                onImageLoaded(texture);
            }
            catch (Exception e)
            {
                onImageLoaded(placeholderTexture);
            }
        }
        public async Task GetAvatarVRMModel(string urlVrm, Action<GameObject, string> onModelLoaded)
        {
            string localPath = await HttpService.Instance().Download3DModelAsync(urlVrm);
            GameObject avatar = ImportVRM(localPath);
            onModelLoaded(avatar, localPath);
            modelCreated?.Invoke();
        }
        public async Task GetNFTCollections(Action<CAModels.NftCollectionsArray> onCollectionsResult)
        {
            var allCollections = new List<CAModels.NftsCollection>();
            string nextPageUrl = null;
            int totalPages = 0;

            try
            {
                var firstPageResponse = await FetchCollectionsPage();
                allCollections.AddRange(firstPageResponse.nftCollections);
                nextPageUrl = firstPageResponse.next;
                totalPages = firstPageResponse.totalPages;

                for (int currentPage = 2; currentPage <= totalPages; currentPage++)
                {
                    var response = await FetchCollectionsPage(nextPageUrl);
                    allCollections.AddRange(response.nftCollections);
                    nextPageUrl = response.next;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            var finalResult = new CAModels.NftCollectionsArray
            {
                nftCollections = allCollections.ToArray()
            };
            onCollectionsResult(finalResult);
        }
        private async Task<CAModels.NftCollectionsArray> FetchCollectionsPage(string url = null)
        {
            string finalUrl = url ?? HttpService.baseUri + collectionsResource;
            string collectionsResult = await HttpService.Instance().Get(finalUrl);
            CAModels.NftCollectionsArray collectionsResponse = JsonUtility.FromJson<CAModels.NftCollectionsArray>(collectionsResult);

            if (collectionsResponse.nftCollections == null)
            {
                throw new Exception("Error fetching or parsing collections data.");
            }

            return collectionsResponse;
        }
        private async Task GetAvatarsByURL(string pageUrl, CAModels.SearchAvatarsDto searchAvatarsDto, Action<CAModels.NftsArray> onAvatarsResult)
        {
            this.searchAvatarsDto = searchAvatarsDto;
            var queryParams = new Dictionary<string, string>{
            {"collectionName", searchAvatarsDto.collectionName},
            {"license", "CC0"}
            };
            pageUrl = HttpService.instance.AddOrUpdateParametersInUrl(pageUrl, queryParams);
            string result = await HttpService.Instance().Get(pageUrl);

            MainThreadDispatcher.RunOnMainThread(() =>
            {
                var avatarsResponse = JsonConvert.DeserializeObject<CAModels.NftsArray>(result);
                SetPaginationData(avatarsResponse.next, avatarsResponse.prev);
                onAvatarsResult(avatarsResponse);
            });
        }

    }
}