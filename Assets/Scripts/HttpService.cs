using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HttpService
{

    private readonly string apiKey;
    private readonly string baseUri;

    public HttpService(string apiKey, string baseUri)
    {
        this.apiKey = apiKey;
        this.baseUri = baseUri;
    }

    public IEnumerator Post(string resource, WWWForm body, System.Action<string> callbackResult)
    {
        UnityWebRequest request = UnityWebRequest.Post(this.baseUri + resource, body);
        return this.HttpMethod(request, callbackResult);
    }

    public IEnumerator Get(string resource, System.Action<string> callbackResult)
    {
        UnityWebRequest request = UnityWebRequest.Get(this.baseUri + resource);
        return this.HttpMethod(request, callbackResult);
    }

    public IEnumerator GetSprite(string url, System.Action<Sprite> callbackResult)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            throw new System.Exception("Error requesting to " + request.url + ", error: " + request.error);

        Texture2D tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
        callbackResult(sprite);
    }

    private IEnumerator HttpMethod(UnityWebRequest request, System.Action<string> callbackResult)
    {
        request.SetRequestHeader("API-KEY", apiKey);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
            throw new System.Exception("Error requesting to " + request.url + ", error: " + request.error);

        callbackResult(request.downloadHandler.text);
    }

}
