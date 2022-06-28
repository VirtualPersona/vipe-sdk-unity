using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;

public class HttpService
{

    private readonly string apiKey;
    private readonly string baseUri;

    public HttpService(string apiKey, string baseUri)
    {
        this.apiKey = apiKey;
        this.baseUri = baseUri;
    }

    public IEnumerator Post<T>(string resource, T body, System.Action<string> callbackResult)
    {
        string json = JsonUtility.ToJson(body);
        UnityWebRequest request = new UnityWebRequest(this.baseUri + resource, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        return this.HttpMethod(request, callbackResult);
    }

    public IEnumerator Get(string resource, System.Action<string> callbackResult)
    {
        UnityWebRequest request = UnityWebRequest.Get(this.baseUri + resource);
        return this.HttpMethod(request, callbackResult);
    }

    // Pensar en refactorizar este método para que utilice HttpMethod y evitar duplicar código
    public IEnumerator GetTexture(string url, System.Action<Texture2D> callbackResult)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            throw new System.Exception("Error requesting to " + request.url + ", error: " + request.error);

        Texture2D tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
        callbackResult(tex);
    }

    public IEnumerator Download3DModel(string url, System.Action<string> callbackResult)
    {
        UnityWebRequest request = new UnityWebRequest(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            throw new System.Exception("Error requesting to " + request.url + ", error: " + request.error);
        }
        else
        {
            byte[] results = request.downloadHandler.data;
            string dirPath = Path.Combine(Application.dataPath, "cryptoavatars");
            Debug.Log("DIR PATH: " + dirPath);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            
            string path = Path.Combine(dirPath, "avatarDownloaded.vrm");
            File.WriteAllBytes(path, results);
            callbackResult(path);
        }
    }

    private IEnumerator HttpMethod(UnityWebRequest request, System.Action<string> callbackResult)
    {
        request.SetRequestHeader("API-KEY", apiKey);
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        Debug.Log(request.url);
        Debug.Log(request.isDone);
        Debug.Log(request.downloadedBytes);

        if (request.isNetworkError || request.isHttpError)
            throw new System.Exception("Error requesting to " + request.url + ", error: " + request.error + " ");

        callbackResult(request.downloadHandler.text);
    }

}
