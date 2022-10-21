using System;
using System.Collections;

using UniGLTF;
using UnityEngine;
using VRM;

namespace CA
{
    public class CryptoAvatars
    {

        private HttpService httpService;

        //private bool userLoggedIn;
        private string userId;

        public CryptoAvatars(string apiKey)
        {
            const string urlServer = "https://api.cryptoavatars.io/v1/";
            this.httpService = new HttpService(apiKey, urlServer);
            //this.userLoggedIn = false;
        }

        /** 
         * onLoginResult -> callback to know if loggin was susscesfully
         */
        public IEnumerator UserLogin(string email, string password, System.Action<Structs.LoginResponseDto> onLoginResult)
        {
            Structs.LoginRequestDto loginRequestDto = new Structs.LoginRequestDto();
            loginRequestDto.email = email;
            loginRequestDto.password = password;

            return this.httpService.Post<Structs.LoginRequestDto>("login-pass", loginRequestDto, (string loginResult) => {
                Structs.LoginResponseDto loginResponse = JsonUtility.FromJson<Structs.LoginResponseDto>(loginResult);
                onLoginResult(loginResponse);
            });
        }

        /** 
         * onAvatarsResult -> callback to get Avatars info
         */
        public IEnumerator GetUserAvatars(string collectionAddress, string owner, string pageUrl, System.Action<Structs.NftsArray> onAvatarsResult)
        {
            // Mejorar
            //if (!userLoggedIn)
            //    return null;

            Structs.OwnerAvatarsDto searchAvatarsDto = new Structs.OwnerAvatarsDto();
            searchAvatarsDto.collectionAddress = collectionAddress;
            searchAvatarsDto.owner = owner;

            return this.httpService.Post<Structs.OwnerAvatarsDto>(pageUrl, searchAvatarsDto, (string avatarsResult) => {

                Structs.NftsArray avatarsResponse = JsonUtility.FromJson<Structs.NftsArray>(avatarsResult);
                onAvatarsResult(avatarsResponse);
            });
        }
        /** 
         * skip & limit for pagination
         * onAvatarsResult -> callback to get Avatars info
         */
        public IEnumerator GetAvatars(string collectionAddress, string licenseType, string pageUrl, System.Action<Structs.NftsArray> onAvatarsResult)
        {
            Structs.DefaultSearchAvatarsDto searchAvatarsDto = new Structs.DefaultSearchAvatarsDto();
            searchAvatarsDto.collectionAddress = collectionAddress;
            searchAvatarsDto.license = licenseType;

            return this.httpService.Post<Structs.DefaultSearchAvatarsDto>(pageUrl, searchAvatarsDto, (string avatarsResult) => {

                Structs.NftsArray avatarsResponse = JsonUtility.FromJson<Structs.NftsArray>(avatarsResult);
                onAvatarsResult(avatarsResponse);
            });
        }
    
        public IEnumerator GetAvatarsByCollectionName(string collectionName, string licenseType, string pageUrl, System.Action<Structs.NftsArray> onAvatarsResult)
        {
            Structs.DefaultSearchAvatarsDtoCollectionName searchAvatarsDto = new Structs.DefaultSearchAvatarsDtoCollectionName();
            searchAvatarsDto.collectionName = collectionName;
            searchAvatarsDto.license = licenseType;
            return this.httpService.Post<Structs.DefaultSearchAvatarsDtoCollectionName>(pageUrl, searchAvatarsDto, (string avatarsResult) =>
            {
                Structs.NftsArray avatarsResponse = JsonUtility.FromJson<Structs.NftsArray>(avatarsResult);
                
                onAvatarsResult(avatarsResponse);
            });
        }
        public IEnumerator GetUserAvatarsByCollectionName(string collectionName,string owner, string pageUrl, System.Action<Structs.NftsArray> onAvatarsResult)
        {
            // Mejorar
            //if (!userLoggedIn)
            //    return null;
            Debug.Log("Entro");
            Structs.OwnerAvatarsDtoCollectionName searchAvatarsDto = new Structs.OwnerAvatarsDtoCollectionName();
            searchAvatarsDto.collectionName = collectionName;
            searchAvatarsDto.owner = owner;

            return this.httpService.Post<Structs.OwnerAvatarsDtoCollectionName>(pageUrl, searchAvatarsDto, (string avatarsResult) =>
            {
                Structs.NftsArray avatarsResponse = JsonUtility.FromJson<Structs.NftsArray>(avatarsResult);
                onAvatarsResult(avatarsResponse);
            });
        }
        /** 
        * onImage -> callback to get Avatar thumbnail
        */
        public IEnumerator GetAvatarPreviewImage(string imageUrl, System.Action<Texture2D> onImage)
        {
            imageUrl = imageUrl.Replace("gateway.pinata.cloud", "usercollection.mypinata.cloud");
            return this.httpService.DownloadImage(imageUrl, (texture) =>
            {
                onImage(texture);
            });
        }

        /** 
        * onImage -> callback to get Avatar thumbnail
        */
        public IEnumerator GetAvatarVRMModel(string urlVrm, System.Action<GameObject> onModelResult)
        {
            urlVrm = urlVrm.Replace("gateway.pinata.cloud", "usercollection.mypinata.cloud");
            return this.httpService.Download3DModel(urlVrm, (localPath) =>
            {
                GameObject avatar = ImportVRM(localPath);
                onModelResult(avatar);
            });
        }

        private GameObject ImportVRM(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (Application.isPlaying)
                return ImportRuntime(path);

            if (path.StartsWithUnityAssetPath())
            {
                Debug.LogWarningFormat("disallow import from folder under the Assets");
                return null;
            }

            return null;
        }

        private GameObject ImportRuntime(string path)
        {
            // load into scene
            var data = new GlbFileParser(path).Parse();
            // VRM extension
            var vrm = new VRMData(data);
            using (var context = new VRMImporterContext(vrm))
            {
                var loaded = context.Load();
                loaded.EnableUpdateWhenOffscreen();
                loaded.ShowMeshes();
                return loaded.gameObject;
            }
        }
        /** 
        * GetNFTsCollections -> callback to get dynamically all the collections
        */
        public IEnumerator GetNFTsCollections(System.Action<Structs.NftsCollectionsArray> onCollectionsResult, string pageUrl)
        {
            string body = "{}";
            return this.httpService.Post(pageUrl, body, (string collectionsResult) =>
            {
                Debug.Log("STRING -> " + collectionsResult);
                Structs.NftsCollectionsArray collectionsResponse = JsonUtility.FromJson<Structs.NftsCollectionsArray>("{\"nftsCollections\":" + collectionsResult + "}");
                onCollectionsResult(collectionsResponse);
            });
        }
    }
}