using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using System.Collections.Generic;
using VRM;

namespace CA
{
    public class CryptoAvatars
    {
        private static readonly string web2LoginResource = "/login-pass";
        private static readonly string web3LoginResource = "/login";
        private static readonly string avatarsResource = "/nfts/avatars";
        private static readonly string collectionsResource = "/collections?containsCC0Nfts=true&license=CC0";
        public MainThreadDispatcher mainThreadDispatcher;
        public event Action modelCreated;

        // login user data
        private string userWallet;

        // Pagination nfts variables
        private CAModels.SearchAvatarsDto searchAvatarsDto;
        public string nextPageUrl;
        public string prevPageUrl;

        public CryptoAvatars(MainThreadDispatcher dispatcher)
        {
            this.mainThreadDispatcher = dispatcher;
            Set();
        }
        public void Set()
        {
            HttpService.apiKey = ApiKeyManager.GetApiKey();
            HttpService.baseUri = "https://api.cryptoavatars.io/v1";
        }
        private async Task Login(string resource, Action<CAModels.LoginResponseDto> onLoginResult)
        {
            string result = await HttpService.Instance().GetAsync(resource);

            CAModels.LoginResponseDto loginResponse = JsonUtility.FromJson<CAModels.LoginResponseDto>(result);
            userWallet = loginResponse.wallet;
            HttpService.accessToken = loginResponse.accessToken;
            loginResponse.accessToken = null;
            onLoginResult(loginResponse);
        }
        public bool HasNextPage()
        {
            return nextPageUrl != null;
        }
        public bool HasPrevPage()
        {
            return prevPageUrl != null;
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
        /** 
         * Return avatars of a specific collection.
         * 
         * collectionName -> name of collection.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public async void GetAvatarsByCollectionName(string collectionName, Action<CAModels.NftsArray> onAvatarsResult)
        {
            CAModels.SearchAvatarsDto searchAvatarsDto = new() { collectionName = collectionName };
            await GetAvatars(searchAvatarsDto, onAvatarsResult);
        }
        /** 
         * Return user avatars of a specific collection.
         * 
         * collectionName -> name of collection.
         * onAvatarsResult -> callback to get Avatars info.
         */
        private bool IsSupportedFormat(byte[] imageBytes)
        {
            // Para PNG, los primeros 8 bytes son: 137, 80, 78, 71, 13, 10, 26, 10
            byte[] pngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

            // Para JPG, los primeros 2 bytes son: 255, 216 y los últimos 2 son: 255, 217
            byte[] jpgStartSignature = { 255, 216 };
            byte[] jpgEndSignature = { 255, 217 };

            bool isPng = true;
            for (int i = 0; i < pngSignature.Length; i++)
            {
                if (imageBytes[i] != pngSignature[i])
                {
                    isPng = false;
                    break;
                }
            }

            bool isJpg = imageBytes[0] == jpgStartSignature[0] && imageBytes[1] == jpgStartSignature[1] &&
                         imageBytes[imageBytes.Length - 2] == jpgEndSignature[0] && imageBytes[imageBytes.Length - 1] == jpgEndSignature[1];

            return isPng || isJpg;
        }
        /** 
         * Login with web2 user data.
         * 
         * email -> user email.
         * password -> user password.
         * onLoginResult -> callback to know if loggin was susscesfully.
         */
        public async Task Web2Login(string email, string password, Action<CAModels.LoginResponseDto> onLoginResult)
        {
            CAModels.LoginRequestDto loginRequestDto = new()
            {
                email = email,
                password = password
            };
            await Login(web2LoginResource, onLoginResult);
        }
        /** 
         * Login with data provided by some external web3 provider as Metamask.
         * 
         * walletAddress -> user wallet addres.
         * signature -> user signature generted by web3 provider signature request.
         * onLoginResult -> callback to know if loggin was susscesfully.
         */
        public async Task Web3Login(string walletAddress, string signature, Action<CAModels.LoginResponseDto> onLoginResult)
        {
            CAModels.LoginWeb3RequestDto loginRequestDto = new()
            {
                wallet = walletAddress,
                signature = signature
            };
            await Login(web3LoginResource, onLoginResult);
        }
        /** 
         * Return avatars metadata with assets urls filtered by searchAvatarsDto.
         * 
         * searchAvatarsDto -> filter to apply on avatars request. There are various filters. Take a look to CAModels.SearchAvatarsDto.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public async Task GetAvatars(CAModels.SearchAvatarsDto searchAvatarsDto, Action<CAModels.NftsArray> onAvatarsResult)
        {
            await GetAvatarsByURL(avatarsResource, searchAvatarsDto, onAvatarsResult);
        }
        /** 
         * Return avatars owned by owner wallet address.
         * 
         * owner -> user wallet.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public async Task GetUserAvatars(string walletAddress, Action<CAModels.NftsArray> onAvatarsResult)
        {
            CAModels.SearchAvatarsDto searchAvatarsDto = new() { owner = walletAddress };
            await GetAvatars(searchAvatarsDto, onAvatarsResult);
        }
        /** 
         * Return previous avatar info chunk. Pagination method.
         * 
         * collectionName -> name of collection.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public async Task PrevPage(Action<CAModels.NftsArray> onAvatarsResult)
        {
            if(prevPageUrl != null)
            await GetAvatarsByURL(prevPageUrl, searchAvatarsDto, onAvatarsResult);
        }
        /** 
         * Return next avatar info chunk. Pagination method.
         * 
         * collectionName -> name of collection.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public async Task NextPage(Action<CAModels.NftsArray> onAvatarsResult)
        {
            if(nextPageUrl != null)
            await GetAvatarsByURL(nextPageUrl, searchAvatarsDto, onAvatarsResult);
        }
        /** 
         * Return Textre2D from an Avatar thumbnail url.
         * 
         * imageUrl -> url where its located the image file.
         * onImage -> callback to get Avatar thumbnail.
         */
        public async Task GetAvatarPreviewImage(string imageUrl, Action<Texture2D> onImageLoaded)
        {
            Texture2D texture = await HttpService.Instance().DownloadImageAsync(imageUrl);
            onImageLoaded(texture);
        }
        public async Task GetAvatarPreviewImage(string imageUrl, Action<Texture2D> onImageLoaded, Texture2D placeholderTexture)
        {
            try
            {
                Texture2D texture = await HttpService.Instance().DownloadImageAsync(imageUrl);
                texture = IsSupportedFormat(texture.EncodeToPNG()) ? texture : placeholderTexture;
                onImageLoaded(texture);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error al cargar la imagen: {e.Message}");
                onImageLoaded(placeholderTexture);
            }
        }
        /** 
         * Return the avatar 3D model from an Avatar VRM url and the local path where its saved after download.
         * 
         * urlVrm -> url where its located the vrm file.
         * onModelResult -> callback to get Avatar thumbnail.
         */
        public async Task GetAvatarVRMModel(string urlVrm, Action<GameObject, string> onModelLoaded)
        {
            string localPath = await HttpService.Instance().Download3DModelAsync(urlVrm);
            GameObject avatar = ImportVRM(localPath);
            onModelLoaded(avatar, localPath);
            modelCreated?.Invoke();
        }
        /** 
         * Return the NFT collections from CA Platform.
         * 
         * onCollectionsResult -> callback to get dynamically all the collections.
         */
        public async Task GetNFTCollections(Action<CAModels.NftCollectionsArray> onCollectionsResult)
        {
            const int limit = 30;
            int totalItemsCount;
            var allCollections = new List<CAModels.NftsCollection>();

            async Task<int> FetchAndAddCollections(int skip)
            {
                var response = await FetchCollectionsPage(skip, limit);
                allCollections.AddRange(response.nftCollections);
                return response.totalItemsCount;
            }

            try
            {
                totalItemsCount = await FetchAndAddCollections(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            int totalPages = (int)Math.Ceiling((double)totalItemsCount / limit);
            string nextPageUrl = null;
            string prevPageUrl = null;

            for (int page = 2; page <= totalPages; page++)
            {
                int skip = (page - 1) * limit;
                try
                {
                    var response = await FetchCollectionsPage(skip, limit);
                    prevPageUrl = response.prev;
                    nextPageUrl = response.next;
                    Debug.Log($"Page {nextPageUrl} of {totalPages} fetched.");
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    break;
                }
            }

            var finalResult = new CAModels.NftCollectionsArray
            {
                nftCollections = allCollections.ToArray(),
                currentPage = totalPages,
                totalPages = totalPages,
                totalItemsCount = totalItemsCount,
                next = nextPageUrl,
                prev = prevPageUrl
            };

            onCollectionsResult(finalResult);
        }
        private async Task<CAModels.NftCollectionsArray> FetchCollectionsPage(int skip, int limit)
        {
            string collectionsResourceWithPagination = $"{collectionsResource}&Skip={skip}&Limit={limit}";
            string collectionsResult = await HttpService.Instance().GetAsync(collectionsResourceWithPagination);
            CAModels.NftCollectionsArray collectionsResponse = JsonUtility.FromJson<CAModels.NftCollectionsArray>(collectionsResult);

            if (collectionsResponse.nftCollections == null || collectionsResponse.nftCollections.Length == 0)
            {
                throw new Exception("Error fetching or parsing collections data.");
            }

            return collectionsResponse;
        }
        /** 
         * Return avatars metadata with assets urls filtered by searchAvatarsDto. Manage pagination by NextPage and PrevPage methods.
         * 
         * pageUrl -> page url to get the next o prev chunk of avatars.
         * searchAvatarsDto -> filter to apply on avatars request. There are various filters. Take a look to CAModels.SearchAvatarsDto.
         * onAvatarsResult -> callback to get avatars info.
         */
        private async Task GetAvatarsByURL(string pageUrl, CAModels.SearchAvatarsDto searchAvatarsDto, Action<CAModels.NftsArray> onAvatarsResult)
        {
            this.searchAvatarsDto = searchAvatarsDto;
            Dictionary<string, string> queryParams = new Dictionary<string, string>
            {
                {"collectionName", searchAvatarsDto.collectionName},
                {"license", "CC0"}
            };

            pageUrl = AddOrUpdateParametersInUrl(pageUrl, queryParams);

            string result = await HttpService.Instance().GetAsync(pageUrl);
            CAModels.NftsArray avatarsResponse = JsonConvert.DeserializeObject<CAModels.NftsArray>(result);

            avatarsResponse.nfts = avatarsResponse.nfts.Where(avatar => avatar.metadata.licenses.license == "CC0").ToArray();

            SetPaginationData(avatarsResponse.next, avatarsResponse.prev);
            onAvatarsResult(avatarsResponse);
        }
        public string AddOrUpdateCollectionNameInUrl(string pageUrl, string collectionName)
        {
            if (pageUrl.Contains("?"))
            {
                string[] parts = pageUrl.Split('?');
                string baseUrl = parts[0];
                string[] parameters = parts[1].Split('&');

                bool found = false;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].StartsWith("collectionName="))
                    {
                        parameters[i] = $"collectionName={collectionName}";
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return $"{pageUrl}&collectionName={collectionName}";
                }
                else
                {
                    return $"{baseUrl}?{string.Join("&", parameters)}";
                }
            }
            else
            {
                return $"{pageUrl}?collectionName={collectionName}";
            }
        }
        public string AddOrUpdateParametersInUrl(string pageUrl, Dictionary<string, string> queryParams)
        {
            if (pageUrl.Contains("?"))
            {
                string[] parts = pageUrl.Split('?');
                string baseUrl = parts[0];
                string[] parameters = parts[1].Split('&');

                foreach (var queryParam in queryParams)
                {
                    bool found = false;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].StartsWith($"{queryParam.Key}="))
                        {
                            parameters[i] = $"{queryParam.Key}={queryParam.Value}";
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        List<string> paramsList = parameters.ToList();
                        paramsList.Add($"{queryParam.Key}={queryParam.Value}");
                        parameters = paramsList.ToArray();
                    }
                }

                return $"{baseUrl}?{string.Join("&", parameters)}";
            }
            else
            {
                return $"{pageUrl}?{string.Join("&", queryParams.Select(qp => $"{qp.Key}={qp.Value}"))}";
            }
        }
    }
}