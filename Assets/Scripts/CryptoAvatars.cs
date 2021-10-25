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
        const string urlServer = "https://api.cryptoavatars.io/";
        this.httpService = new HttpService(apiKey, urlServer);
        this.userLoggedIn = false;

        // Check Key
        Debug.Log("Need check API_KEY");
    }
    
    public IEnumerator UserLogin(string email, string password, System.Action<bool> onLoginResult)
    {
        WWWForm body = new WWWForm();
        body.AddField("email", email);
        body.AddField("password", password);
        return this.httpService.Post("login-pass", body, (string loginResult) => {
            Debug.Log(loginResult);
            Structs.LoginDto loginResponse = JsonUtility.FromJson<Structs.LoginDto>(loginResult);
            this.userLoggedIn = loginResponse.userId != null;
            this.userId = loginResponse.userId;
            onLoginResult(this.userLoggedIn);
        });
    }

    public IEnumerator GetUserAvatars(System.Action<Structs.AvatarsArray> onAvatarsResult)
    {
        if (!userLoggedIn)
            return null;

        return this.httpService.Get("avatars/owner/wallet", (string avatarsResult) => {
            Debug.Log(avatarsResult);
            Structs.AvatarsArray avatarsResponse = JsonUtility.FromJson<Structs.AvatarsArray>(avatarsResult);

            onAvatarsResult(avatarsResponse);
        });
    }

    public IEnumerator GetAvatars(System.Action<Structs.AvatarsArray> onAvatarsResult, int itemsPerPage = 10, int pageNum = 0)
    {
        WWWForm body = new WWWForm();
        return this.httpService.Post("avatars/list", body, (string avatarsResult) => {
            // Unity no soporta jsonArray en la raíz al deserializar, así que lo encapsulamos en un objeto
            string JSONToParse = "{\"avatars\":" + avatarsResult + "}";
            Structs.AvatarsArray avatarsResponse = JsonUtility.FromJson<Structs.AvatarsArray>(JSONToParse);
            onAvatarsResult(avatarsResponse);
        });
    }

    IEnumerator GetAvatarImage(string imageUrl, System.Action<Sprite> onSpriteResult)
    {
        return this.httpService.GetSprite(imageUrl, (sprite) =>
        {
            onSpriteResult(sprite);
        });
    }

    IEnumerator GetAvatarModel(string vrmUrl)
    {
        return null;
    }

}
