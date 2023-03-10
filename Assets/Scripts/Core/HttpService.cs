using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

namespace CA
{
    public class HttpService
    {
        private static HttpService instance;

        public static string accessToken;
        public static string apiKey;
        public static string baseUri;

        private HttpService()
        {
        }

        public static HttpService Instance()
        {
            if (baseUri == null || apiKey == null)
                throw new System.Exception("Initialize apiKey and baseUri values before use this service");

            if (instance == null)
                instance = new HttpService();

            return instance;
        }

        public IEnumerator Post<T>(string resource, T body, System.Action<string> callbackResult)
        {
            string json = JsonConvert.SerializeObject(body, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            UnityWebRequest request = new (baseUri + resource, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            return this.HttpMethod(request, callbackResult);
        }

        public IEnumerator Get(string resource, System.Action<string> callbackResult)
        {
            UnityWebRequest request = UnityWebRequest.Get(baseUri + resource);
            return this.HttpMethod(request, callbackResult);
        }

        public IEnumerator DownloadImage(string url, System.Action<Texture2D> callbackResult)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                throw new System.Exception("Error requesting to " + request.url + ", error: " + request.error);

            Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
            callbackResult(tex);
            request.Dispose();
        }

        public IEnumerator Download3DModel(string url, System.Action<string> callbackResult)
        {
            UnityWebRequest request = new (url);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                throw new System.Exception("Error requesting to " + request.url + ", error: " + request.error);
            }
            else
            {
                byte[] results = request.downloadHandler.data;

                string dirPath = Path.Combine(Application.persistentDataPath, "cryptoavatars");

                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                string path = Path.Combine(dirPath, "avatarDownloaded.vrm");
                File.WriteAllBytes(path, results);
                callbackResult(path);
                request.Dispose();
            }
        }

        private IEnumerator HttpMethod(UnityWebRequest request, System.Action<string> callbackResult)
        {
            if (accessToken == null)
                request.SetRequestHeader("API-KEY", apiKey);
            else
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                throw new System.Exception("Error requesting to " + request.url + ", error: " + request.error);

            callbackResult(request.downloadHandler.text);
            request.Dispose();
        }

    }
}