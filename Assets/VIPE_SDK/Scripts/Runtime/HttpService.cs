using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Web;

namespace VIPE_SDK
{
    public class HttpService
    {
        public static HttpService instance;
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
        public string AddOrUpdateParametersInUrl(string pageUrl, Dictionary<string, string> queryParams)
        {
            if (string.IsNullOrEmpty(pageUrl))
            {
                throw new ArgumentException("pageUrl cannot be null or empty.");
            }

            try
            {
                var uriBuilder = new UriBuilder(HttpService.baseUri + pageUrl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                foreach (var keyValuePair in queryParams)
                {
                    query[keyValuePair.Key] = keyValuePair.Value;
                }

                uriBuilder.Query = query.ToString();
                return uriBuilder.Uri.ToString();
            }
            catch (UriFormatException e)
            {
                throw new ArgumentException("Invalid URL format.", e);
            }
        }
        public Task<string> Get(string resource)
        {
            var tcs = new TaskCompletionSource<string>();

            UnityWebRequest request = UnityWebRequest.Get(resource);
            if (accessToken == null)
                request.SetRequestHeader("API-KEY", apiKey);
            else
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            var operation = request.SendWebRequest();
            Debug.Log("resource:" + resource);
            Debug.Log("operation:" + operation);
            operation.completed += op =>
            {
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
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