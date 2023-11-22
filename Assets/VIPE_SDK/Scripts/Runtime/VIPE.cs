using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRM;

namespace VIPE_SDK
{
    public class VIPE
    {
        public static readonly string AvatarsResource = "/nfts/avatars";
        private static readonly string collectionsResource = "/collections?containsCC0Nfts=true";

        private Models.SearchAvatarsDto searchAvatarsDto;

        public event Action ModelCreated;

        public string NextPageUrl;
        public string PrevPageUrl;

        public VIPE()
        {
            HttpService.apiKey = SecureDataHandler.LoadAPIKey();
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
            NextPageUrl = next?.Split("/v1")[1];
            PrevPageUrl = prev?.Split("/v1")[1];
        }

        private bool IsSupportedFormat(byte[] imageBytes)
        {
            return Utility.IsPng(imageBytes) || Utility.IsJpg(imageBytes);
        }

        public async Task GetAvatars(Action<Models.NftsArray> onAvatarsResult, Dictionary<string, string> queryParams = null, string wallet = null)
        {
            if (wallet != null)
                wallet = "/" + wallet;

            await GetAvatarsByURL(AvatarsResource + wallet, onAvatarsResult, queryParams);
        }

        public async Task PrevPage(Action<Models.NftsArray> onAvatarsResult, Dictionary<string, string> queryParams = null)
        {
            if (PrevPageUrl != null)
            {
                await GetAvatarsByURL(PrevPageUrl, onAvatarsResult, queryParams);
            }
        }

        public async Task NextPage(Action<Models.NftsArray> onAvatarsResult, Dictionary<string, string> queryParams = null)
        {
            if (NextPageUrl != null)
            {
                await GetAvatarsByURL(NextPageUrl, onAvatarsResult, queryParams);
            }
        }

        public async Task GetAvatarPreviewImage(string imageUrl, Action<Texture2D> onImageLoaded, Texture2D placeholderTexture = null)
        {
            try
            {
                Texture2D texture = await HttpService.Instance().DownloadImageAsync(imageUrl);
                texture = IsSupportedFormat(texture.EncodeToPNG()) ? texture : placeholderTexture;
                onImageLoaded(texture);
            }
            catch (Exception)
            {
                onImageLoaded(placeholderTexture);
            }
        }

        public async Task GetAvatarVRMModel(string urlVrm, Action<GameObject, string> onModelLoaded)
        {
            string localPath = await HttpService.Instance().Download3DModelAsync(urlVrm);
            GameObject avatar = ImportVRM(localPath);
            onModelLoaded(avatar, localPath);
            ModelCreated?.Invoke();
        }

        public async Task GetNFTCollections(Action<Models.NftCollectionsArray> onCollectionsResult)
        {
            var allCollections = new List<Models.NftsCollection>();
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

            var finalResult = new Models.NftCollectionsArray
            {
                nftCollections = allCollections.ToArray()
            };
            onCollectionsResult(finalResult);
        }

        private async Task<Models.NftCollectionsArray> FetchCollectionsPage(string url = null)
        {
            string finalUrl = url ?? HttpService.baseUri + collectionsResource;
            string collectionsResult = await HttpService.Instance().Get(finalUrl);
            Models.NftCollectionsArray collectionsResponse =
                JsonUtility.FromJson<Models.NftCollectionsArray>(collectionsResult);

            if (collectionsResponse.nftCollections == null)
            {
                throw new Exception("Error fetching or parsing collections data.");
            }

            return collectionsResponse;
        }

        public Dictionary<string, string> DefaultQuery(string collectionName)
        {
            return new Dictionary<string, string>
            {
                { "collectionSlug", collectionName},
                { "license", "CC0" }
            };
        }

        public async Task GetAvatarsByURL(string pageUrl, Action<Models.NftsArray> onAvatarsResult, Dictionary<string, string> queryParams = null)
        {
            if (queryParams == null)
            {
                queryParams = new Dictionary<string, string>();
            }

            pageUrl = HttpService.instance.AddOrUpdateParametersInUrl(pageUrl, queryParams);

            string result = await HttpService.Instance().Get(pageUrl);

            var avatarsResponse = JsonUtility.FromJson<Models.NftsArray>(result);
            SetPaginationData(avatarsResponse.next, avatarsResponse.prev);
            onAvatarsResult(avatarsResponse);
        }
    }
}