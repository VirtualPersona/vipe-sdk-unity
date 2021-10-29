using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
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

    public IEnumerator downloadVRM(string url)
    {
        UnityWebRequest www = new UnityWebRequest(url);
        //string path = Path.Combine(Application.dataPath, "test.vrm");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;

            File.WriteAllBytes(Path.Combine(Application.dataPath, "test.vrm"), results);
            if (GameObject.Find("VRM"))
            {
                MonoBehaviour.Destroy(GameObject.Find("VRM"));
            }
            GameObject avatar = ImportVRM(Path.Combine(Application.dataPath, "test.vrm"));

            avatar.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Anims/VRM") as RuntimeAnimatorController;
            avatar.transform.eulerAngles += new Vector3(0, 180, 0);
            avatar.transform.position += new Vector3(0, GameObject.Find("Cylinder").transform.localScale.y, 0);
        }
    }
    private GameObject ImportVRM(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (Application.isPlaying)
        {
            return ImportRuntime(path);
        }

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
        // VRM extension を parse します
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
