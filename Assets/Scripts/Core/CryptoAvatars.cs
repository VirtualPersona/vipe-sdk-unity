using Newtonsoft.Json;
using System;
using System.Collections;

using UniGLTF;
using UnityEngine;
using VRM;

namespace CA
{
    public class CryptoAvatars : MonoBehaviour
    {
        [SerializeField]
        private string API_KEY;
        [SerializeField]
        private string API_KEY_TESTNETS;

        [SerializeField]
        private bool useTestnetsEnvironment = false;

        private static readonly string web2LoginResource = "/login-pass";
        private static readonly string web3LoginResource = "/login";
        private static readonly string avatarsResource = "/nfts/avatars/list?skip=0&limit=16";
        private static readonly string collectionsResource = "/collections";

        // login user data
        private string userWallet;

        // Pagination nfts variables
        private CAModels.SearchAvatarsDto searchAvatarsDto;
        private string nextPageUrl;
        private string prevPageUrl;

        public void Start()
        {
            HttpService.apiKey = (useTestnetsEnvironment == true)
                ? API_KEY_TESTNETS
                : API_KEY;

            string urlServer = (useTestnetsEnvironment == true)
                ? "https://api.testnets.cryptoavatars.io:3000/v1"
                : "https://api.cryptoavatars.io/v1";

            HttpService.baseUri = urlServer;
        }

        /** 
         * Login with web2 user data.
         * 
         * email -> user email.
         * password -> user password.
         * onLoginResult -> callback to know if loggin was susscesfully.
         */
        public void Web2Login(string email, string password, Action<CAModels.LoginResponseDto> onLoginResult)
        {
            CAModels.LoginRequestDto loginRequestDto = new() {
                email = email,
                password = password
            };
            Login(web2LoginResource, loginRequestDto, onLoginResult);
        }

        /** 
         * Login with data provided by some external web3 provider as Metamask.
         * 
         * walletAddress -> user wallet addres.
         * signature -> user signature generted by web3 provider signature request.
         * onLoginResult -> callback to know if loggin was susscesfully.
         */
        public void Web3Login(string walletAddress, string signature, Action<CAModels.LoginResponseDto> onLoginResult)
        {
            CAModels.LoginWeb3RequestDto loginRequestDto = new() {
                wallet = walletAddress,
                signature = signature
            };
            Login(web3LoginResource, loginRequestDto, onLoginResult);
        }

        /** 
         * Return avatars metadata with assets urls filtered by searchAvatarsDto.
         * 
         * searchAvatarsDto -> filter to apply on avatars request. There are various filters. Take a look to CAModels.SearchAvatarsDto.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public void GetAvatars(CAModels.SearchAvatarsDto searchAvatarsDto, Action<CAModels.NftsArray> onAvatarsResult)
        {
            GetAvatarsByURL(avatarsResource, searchAvatarsDto, onAvatarsResult);
        }

        /** 
         * Return avatars owned by owner wallet address.
         * 
         * owner -> user wallet.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public void GetUserAvatars(string walletAddress, Action<CAModels.NftsArray> onAvatarsResult)
        {
            CAModels.SearchAvatarsDto searchAvatarsDto = new(){ owner = walletAddress };
            GetAvatars(searchAvatarsDto, onAvatarsResult);
        }

        /** 
         * Return avatars of a specific collection.
         * 
         * collectionName -> name of collection.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public void GetAvatarsByCollectionName(string collectionName, Action<CAModels.NftsArray> onAvatarsResult)
        {
            CAModels.SearchAvatarsDto searchAvatarsDto = new() { collectionName = collectionName };
            GetAvatars(searchAvatarsDto, onAvatarsResult);
        }

        /** 
         * Return user avatars of a specific collection.
         * 
         * collectionName -> name of collection.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public void GetUserAvatarsByCollectionName(string collectionName, string owner, Action<CAModels.NftsArray> onAvatarsResult)
        {
            CAModels.SearchAvatarsDto searchAvatarsDto = new CAModels.SearchAvatarsDto();
            searchAvatarsDto.collectionName = collectionName;
            searchAvatarsDto.owner = owner;
            GetAvatars(searchAvatarsDto, onAvatarsResult);
        }

        /** 
         * Return previous avatar info chunk. Pagination method.
         * 
         * collectionName -> name of collection.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public void PrevPage(Action<CAModels.NftsArray> onAvatarsResult)
        {
            GetAvatarsByURL(prevPageUrl, searchAvatarsDto, onAvatarsResult);
        }

        /** 
         * Return next avatar info chunk. Pagination method.
         * 
         * collectionName -> name of collection.
         * onAvatarsResult -> callback to get Avatars info.
         */
        public void NextPage(Action<CAModels.NftsArray> onAvatarsResult)
        {
            GetAvatarsByURL(nextPageUrl, searchAvatarsDto, onAvatarsResult);
        }

        public bool HasNextPage()
        {
            return nextPageUrl != null;
        }

        public bool HasPrevPage()
        {
            return prevPageUrl != null;
        }

        /** 
         * Return Textre2D from an Avatar thumbnail url.
         * 
         * imageUrl -> url where its located the image file.
         * onImage -> callback to get Avatar thumbnail.
         */
        public void GetAvatarPreviewImage(string imageUrl, Action<Texture2D> onImageLoaded)
        {
            IEnumerator downloadImage = HttpService.Instance().DownloadImage(imageUrl, (texture) => {
                onImageLoaded(texture);
            });
            StartCoroutine(downloadImage);
        }

        /** 
         * Return the avatar 3D model from an Avatar VRM url and the local path where its saved after download.
         * 
         * urlVrm -> url where its located the vrm file.
         * onModelResult -> callback to get Avatar thumbnail.
         */
        public void GetAvatarVRMModel(string urlVrm, Action<GameObject, string> onModelLoaded)
        {
            IEnumerator download3DModel = HttpService.Instance().Download3DModel(urlVrm, (localPath) => {
                GameObject avatar = ImportVRM(localPath);
                onModelLoaded(avatar, localPath);
            });
            StartCoroutine(download3DModel);
        }

        /** 
         * Return the NFT collections from CA Platform.
         * 
         * onCollectionsResult -> callback to get dynamically all the collections.
         */
        public void GetNFTsCollections(Action<CAModels.NftsCollectionsArray> onCollectionsResult)
        {
            IEnumerator getNFTCollections = HttpService.Instance().Post(collectionsResource, "{}", (string collectionsResult) => {
                CAModels.NftsCollectionsArray collectionsResponse = JsonUtility.FromJson<CAModels.NftsCollectionsArray>("{\"nftsCollections\":" + collectionsResult + "}");
                onCollectionsResult(collectionsResponse);
            });
            StartCoroutine(getNFTCollections);
        }

        private void Login<T>(string resource, T loginRequestDTO, Action<CAModels.LoginResponseDto> onLoginResult)
        {
            IEnumerator login = HttpService.Instance().Post(resource, loginRequestDTO, (string loginResult) => {
                CAModels.LoginResponseDto loginResponse = JsonUtility.FromJson<CAModels.LoginResponseDto>(loginResult);
                userWallet = loginResponse.wallet;
                HttpService.accessToken = loginResponse.accessToken;
                loginResponse.accessToken = null;
                onLoginResult(loginResponse);
            });
            StartCoroutine(login);
        }

        /** 
         * Return avatars metadata with assets urls filtered by searchAvatarsDto. Manage pagination by NextPage and PrevPage methods.
         * 
         * pageUrl -> page url to get the next o prev chunk of avatars.
         * searchAvatarsDto -> filter to apply on avatars request. There are various filters. Take a look to CAModels.SearchAvatarsDto.
         * onAvatarsResult -> callback to get avatars info.
         */
        private void GetAvatarsByURL(string pageUrl, CAModels.SearchAvatarsDto searchAvatarsDto, Action<CAModels.NftsArray> onAvatarsResult)
        {
            this.searchAvatarsDto = searchAvatarsDto;

            IEnumerator getAvatars = HttpService.Instance().Post(pageUrl, searchAvatarsDto, (string avatarsResult) => {
                CAModels.NftsArray avatarsResponse = JsonConvert.DeserializeObject<CAModels.NftsArray>(avatarsResult);
                SetPaginationData(avatarsResponse.next, avatarsResponse.prev);
                onAvatarsResult(avatarsResponse);
            });

            StartCoroutine(getAvatars);
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

            if (Application.isPlaying)
            {
                // load into scene
                var data = new GlbFileParser(path).Parse();
                // VRM extension
                var vrm = new VRMData(data);
                using var context = new VRMImporterContext(vrm);
                var loaded = context.Load();
                loaded.EnableUpdateWhenOffscreen();
                loaded.ShowMeshes();
                data.Dispose();
                return loaded.gameObject;
            }

            return null;
        }

        private void SetPaginationData(string next, string prev)
        {
            nextPageUrl = next?.Split("/v1")[1];
            prevPageUrl = prev?.Split("/v1")[1];
            Debug.Log("pagination url pages: " + prevPageUrl + " | " + nextPageUrl);
        }
    }
}