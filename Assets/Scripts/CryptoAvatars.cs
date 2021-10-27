using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CryptoAvatars
{

    private HttpService httpService;

    private bool userLoggedIn;
    private string userId;

    public CryptoAvatars(string apiKey)
    {
        const string urlServer = "http://localhost:3000/";
        this.httpService = new HttpService(apiKey, urlServer);
        this.userLoggedIn = false;

        // Check Key
        Debug.Log("Need check API_KEY");
    }
    
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

    public IEnumerator GetUserAvatars(System.Action<Structs.AvatarsArray> onAvatarsResult)
    {
        // Mejorar
        if (!userLoggedIn)
            return null;

        return this.httpService.Get("avatars/owner/wallet", (string avatarsResult) => {
            Debug.Log(avatarsResult);
            Structs.AvatarsArray avatarsResponse = JsonUtility.FromJson<Structs.AvatarsArray>(avatarsResult);

            onAvatarsResult(avatarsResponse);
        });
    }

    public IEnumerator GetAvatars(int skip, int limit, System.Action<Structs.AvatarsArray> onAvatarsResult)
    {
        Structs.SearchAvatarsDto searchAvatarsDto = new Structs.SearchAvatarsDto();
        searchAvatarsDto.skip = skip;
        searchAvatarsDto.limit = limit;

        return this.httpService.Post<Structs.SearchAvatarsDto>("avatars/list", searchAvatarsDto, (string avatarsResult) => {
            // Unity no soporta jsonArray en la raíz al deserializar, así que lo encapsulamos en un objeto
            string JSONToParse = "{\"avatars\":" + avatarsResult + "}";
            Debug.Log(JSONToParse);
            Structs.AvatarsArray avatarsResponse = JsonUtility.FromJson<Structs.AvatarsArray>(JSONToParse);
            onAvatarsResult(avatarsResponse);
        });
    }

    public IEnumerator GetAvatarPreviewImage(string imageUrl, System.Action<Texture2D> onImage)
    {
        imageUrl = imageUrl.Replace("gateway.pinata.cloud", "ipfs.io");
        return this.httpService.GetTexture(imageUrl, (texture) =>
        {
            onImage(texture);
        });
    }

    IEnumerator GetAvatarModel(string vrmUrl)
    {
        return null;
    }

}
