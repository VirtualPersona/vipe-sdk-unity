using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Threading.Tasks;
using System;

namespace CA
{
    public class HttpService
    {
        private static HttpService instance;
        public static string accessToken;
        public static string apiKey;
        public static string baseUri;
        private HttpService() { }
        public static HttpService Instance()
        {
            if (baseUri == null || apiKey == null)
                throw new System.Exception("Initialize apiKey and baseUri values before use this service");

            if (instance == null)
                instance = new HttpService();

            return instance;
        }
        public async Task<Texture2D> DownloadImageAsync(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                //throw new ArgumentException("URL ERROR");
                return null;
            }

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += (op) => tcs.SetResult(true);
            await tcs.Task;

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                throw new Exception("Error downloading image from " + request.url + ", error: " + request.error);
            }

            Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
            request.Dispose();
            return tex;
        }
        public async Task<string> Download3DModelAsync(string url)
        {
            UnityWebRequest request = new UnityWebRequest(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += (op) => tcs.SetResult(true);
            await tcs.Task;

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                throw new Exception("Error downloading 3D model from " + request.url + ", error: " + request.error);
            }

            byte[] results = request.downloadHandler.data;

            string dirPath = Path.Combine(Application.persistentDataPath, "cryptoavatars");

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            string path = Path.Combine(dirPath, "avatarDownloaded.vrm");
            File.WriteAllBytes(path, results);
            request.Dispose();
            return path;
        }
        public Task<string> GetAsync(string resource)
        {
            var tcs = new TaskCompletionSource<string>();

            UnityWebRequest request = UnityWebRequest.Get(baseUri + resource);
            if (accessToken == null)
                request.SetRequestHeader("API-KEY", apiKey);
            else
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            var operation = request.SendWebRequest();

            operation.completed += op =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    tcs.SetException(new Exception(request.error));
                }
                else
                {
                    tcs.SetResult(request.downloadHandler.text);
                }
            };

            return tcs.Task;
        }
    }
}