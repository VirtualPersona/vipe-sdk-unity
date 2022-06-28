using System.Collections;

using UniGLTF;
using UnityEngine;
using VRM;

public class CryptoAvatars
{

    private HttpService httpService;

    private bool userLoggedIn;
    private string userId;

    public CryptoAvatars(string apiKey)
    {
        const string urlServer = "https://api.cryptoavatars.io/";
        this.httpService = new HttpService(apiKey, urlServer);
        this.userLoggedIn = false;
    }

    /** 
     * onLoginResult -> callback to know if loggin was susscesfully
     */
    public IEnumerator UserLogin(string email, string password, System.Action<bool> onLoginResult)
    {
        Structs.LoginRequestDto loginRequestDto = new Structs.LoginRequestDto();
        loginRequestDto.email = email;
        loginRequestDto.password = password;

        return this.httpService.Post<Structs.LoginRequestDto>("login-pass", loginRequestDto, (string loginResult) => {
            Structs.LoginResponseDto loginResponse = JsonUtility.FromJson<Structs.LoginResponseDto>(loginResult);
            this.userLoggedIn = loginResponse.userId != null;
            this.userId = loginResponse.userId;
            onLoginResult(this.userLoggedIn);
        });
    }

    /** 
     * onAvatarsResult -> callback to get Avatars info
     */
    public IEnumerator GetUserAvatars(System.Action<Structs.AvatarsArray> onAvatarsResult)
    {
        // Mejorar
        if (!userLoggedIn)
            return null;

        return this.httpService.Get("nfts/avatars/owner/wallet", (string avatarsResult) => {
            Structs.AvatarsArray avatarsResponse = JsonUtility.FromJson<Structs.AvatarsArray>(avatarsResult);
            onAvatarsResult(avatarsResponse);
        });
    }

    /** 
     * skip & limit for pagination
     * onAvatarsResult -> callback to get Avatars info
     */
    public IEnumerator GetAvatars(int skip, int limit, string nexPageUrl, System.Action<Structs.NftsArray> onAvatarsResult)
    {
        Structs.SearchAvatarsDto searchAvatarsDto = new Structs.SearchAvatarsDto();
        searchAvatarsDto.skip = skip;
        searchAvatarsDto.limit = limit;

        if(nexPageUrl != "")
        {
            return this.httpService.Post<Structs.SearchAvatarsDto>(nexPageUrl, searchAvatarsDto, (string avatarsResult) => {
                // Unity no soporta jsonArray en la raíz al deserializar, así que lo encapsulamos en un objeto
                //string JSONToParse = "{\"avatars\":" + avatarsResult + "}";

                Structs.NftsArray avatarsResponse = JsonUtility.FromJson<Structs.NftsArray>(avatarsResult);
                Debug.Log(avatarsResult);
                onAvatarsResult(avatarsResponse);
            });

        }

        return this.httpService.Post<Structs.SearchAvatarsDto>($"nfts/avatars/list?skip={searchAvatarsDto.skip}&limit={searchAvatarsDto.limit}", searchAvatarsDto, (string avatarsResult) => {
            // Unity no soporta jsonArray en la raíz al deserializar, así que lo encapsulamos en un objeto
            //string JSONToParse = "{\"avatars\":" + avatarsResult + "}";

            Structs.NftsArray avatarsResponse = JsonUtility.FromJson<Structs.NftsArray>(avatarsResult);
            Debug.Log(avatarsResult);
            onAvatarsResult(avatarsResponse);
        });
    }

    /** 
    * onImage -> callback to get Avatar thumbnail
    */
    public IEnumerator GetAvatarPreviewImage(string imageUrl, System.Action<Texture2D> onImage)
    {
        imageUrl = imageUrl.Replace("gateway.pinata.cloud", "usercollection.mypinata.cloud");
        return this.httpService.GetTexture(imageUrl, (texture) =>
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
}
