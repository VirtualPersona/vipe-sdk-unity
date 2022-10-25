using BloodUI;
using CA;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.Utility;
using static Structs;

public class AvatarSelectionSetup : MonoBehaviour
{
    private string collectionNameSelected = "CryptoAvatars";
    private bool userLoggedIn;
    private CryptoAvatars cryptoAvatars;
    private AvatarSelectionWindow avatarSelectionWindow;
    private AvatarPlayableWindow playableAvatarWindow;
    //Configurable
    private int nftPerLoad = 20;
    private int nftsSkipped = 0;
    private int pageCount = 1;
    // 
    private int totalNfts = 0;
    private int totalPages = 0;

    private string nextPage = "";
    private string previousPage = "";
    //
    private GameObject Vrm;
    private GameObject Vrm_Target;
    private GameObject Cam;
    private GameObject camTarget;
    //
    private string licenseType = "CC0";

    private string userWallet = "";
    const string urlServer = "https://api.cryptoavatars.io/v1/";

    private const string API_KEY = "$2b$10$jXmDbzXmgU7YjsshSRuSnOfdlMky/eUX7LPhJ0Y8jAtypyu4vJK1a";
    // UI Toolkit 
    private UIDocument doc;
    private VisualElement root;
    [SerializeField]
    private GameObject LoginPanelUIDoc;

    public void ShowAvatarSelection()
    {
        this.Cam.GetComponent<SmoothFollow>().previewMode = false;
        root.Add(avatarSelectionWindow);
        refreshAvatars();
    }
    public void HideAvatarSelection()
    {
        root.Remove(avatarSelectionWindow);
    }
    private void Awake()
    {
        this.cryptoAvatars = new CryptoAvatars(API_KEY);
        this.Vrm = GameObject.Find("VRM");
        this.Vrm_Target = GameObject.Find("VRM_Target");
        this.Cam = GameObject.Find("Main Camera");
        this.camTarget = GameObject.Find("Camera_Target");
    }

    private void OnEnable()
    {
        TryGetComponent(out doc);
        root = doc.rootVisualElement;
        this.downloadCollections($"collections/list?skip=0&limit={nftPerLoad}");
        this.downloadAvatars($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        avatarSelectionWindow = new AvatarSelectionWindow();
        playableAvatarWindow = new AvatarPlayableWindow();
        playableAvatarWindow.BackToAvatarSelectionRequested += () => HidePlayableWindow();
        playableAvatarWindow.BackToAvatarSelectionRequested += () => DisableAvatarMovement();
        playableAvatarWindow.BackToAvatarSelectionRequested += () => ShowAvatarSelection();
        avatarSelectionWindow.BackToLoginRequested += backToLogin;
        avatarSelectionWindow.LoadMoreRequested += loadMoreNfts;
        avatarSelectionWindow.LoadPreviousRequested += loadPreviousNfts;
        avatarSelectionWindow.OnLoadCollectionsRequested += changeCollection;
        avatarSelectionWindow.OnLoadOpenSourceRequested += refreshAvatars;
        root.Add(avatarSelectionWindow);
    }

    private void changeCollection(string value)
    {
        this.collectionNameSelected = value;
        removeCurrentAvatarsCards();
        if (this.userWallet != "")
        {
            this.downloadAvatarsUsers($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        }
        else
        {
            this.downloadAvatars($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        }
    }

    private void refreshAvatars()
    {
        this.pageCount = 1;
        if (this.userLoggedIn)
        {
            this.downloadAvatarsUsers($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        }
        else
        {
            this.downloadAvatars($"nfts/avatars/list?skip=0&limit={nftPerLoad}");
        }
    }

    private void backToLogin()
    {
        HideAvatarSelection();
        LoginPanelUIDoc.GetComponent<LoginPanelSetup>().LoginShow();
        if (avatarSelectionWindow.scrollView.contentViewport.childCount > 0)
        {
            removeCurrentAvatarsCards();
        }
        DisableAvatarMovement();
    }

    private void DisableAvatarMovement()
    {
        this.Vrm = GameObject.Find("VRM");
        if (Vrm)
        {
            this.Vrm.transform.position = this.Vrm_Target.transform.position;
            this.Vrm.transform.rotation = this.Vrm_Target.transform.rotation;
            this.Vrm.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Destroy(this.Vrm.GetComponent<CharacterController>());
            Destroy(this.Vrm.GetComponent<ThirdPersonUserControl>());
            Destroy(this.Vrm.GetComponent<ThirdPersonCharacter>());
            this.Vrm.GetComponent<Animator>().SetBool("OnGround", true);
            this.Vrm.GetComponent<Animator>().SetBool("Crouch", false);
            this.Vrm.GetComponent<Animator>().SetFloat("Forward", 0);
            this.Vrm.GetComponent<Animator>().SetFloat("Turn", 0);
            this.Vrm.GetComponent<Animator>().SetFloat("Jump", 0);
            this.Vrm.GetComponent<Animator>().SetFloat("JumpLeg", 0);
            this.Cam.transform.position = this.camTarget.transform.position;
            this.Cam.transform.rotation = this.camTarget.transform.rotation;
            this.Cam.GetComponent<SmoothFollow>().target = null;
        }
    }
    private IEnumerator MoveCameraPosition(Transform initialCameraTransform, Transform destinationCameraTransform, float speed)
    {
        // check the distance and see if we still need to move towards the destination ?
        while (Vector3.Distance(initialCameraTransform.position, destinationCameraTransform.transform.position) > 1.0f)
        {
            initialCameraTransform.position = Vector3.Lerp(initialCameraTransform.position, destinationCameraTransform.position, Time.deltaTime * speed);
            yield return null;
        }
    }
    private IEnumerator MoveCameraRotation(Transform initialCameraTransform, Transform destinationCameraTransform, float speed)
    {
        while (Quaternion.Angle(initialCameraTransform.rotation, destinationCameraTransform.transform.rotation) > Mathf.Epsilon)
        {
            initialCameraTransform.rotation = Quaternion.Slerp(initialCameraTransform.rotation, destinationCameraTransform.rotation, speed * Time.deltaTime);
            yield return null;
        }
    }

    private void removeCurrentAvatarsCards()
    {
        if (avatarSelectionWindow != null)
            avatarSelectionWindow.scrollView.Clear();
    }

    private void loadMoreNfts()
    {
        this.nftsSkipped += this.nftPerLoad;
        if (this.nftsSkipped >= (this.totalNfts - this.nftPerLoad))
            this.nftsSkipped = this.totalNfts - this.nftPerLoad;

        if (this.pageCount > this.totalPages)
            this.pageCount = this.totalPages;

        //Load the new avatars with a corroutine
        if (this.nextPage != "")
        {
            this.pageCount += 1;
            avatarSelectionWindow.paginationTextField.value = (this.pageCount.ToString());
            downloadAvatars(this.nextPage);
        }
    }
    private void loadPreviousNfts()
    {
        this.nftsSkipped -= this.nftPerLoad;
        if (this.nftsSkipped < 0)
            this.nftsSkipped = 0;
        if (this.pageCount < 0)
            this.pageCount = 0;

        //Load the new avatars
        if (this.previousPage != "")
        {
            this.pageCount -= 1;
            avatarSelectionWindow.paginationTextField.value = (this.pageCount.ToString());
            downloadAvatars(this.previousPage);
        }

    }

    private void displayAndLoadAvatars(NftsArray onAvatarsResult)
    {
        Structs.Nft[] nfts = onAvatarsResult.nfts;
        this.totalNfts = onAvatarsResult.totalNfts;
        this.totalPages = (totalNfts / nftPerLoad) + 1;
        avatarSelectionWindow.paginationTextField.value = $"{pageCount.ToString()}/{this.totalPages.ToString()}";

        int pos = urlServer.IndexOf(".io/v1/");

        if (onAvatarsResult.next != null && onAvatarsResult.next != "Null" && onAvatarsResult.next != "")
            this.nextPage = onAvatarsResult.next.Substring(pos + 7);
        else
            this.nextPage = "";

        if (onAvatarsResult.prev != null && onAvatarsResult.prev != "Null" && onAvatarsResult.prev != "")
            this.previousPage = onAvatarsResult.prev.Substring(pos + 7);
        else
            this.previousPage = "";

        for (int i = 0; i < nfts.Length; i++)
        {
            // Create panel layout for each avatar
            Structs.Nft nft = nfts[i];
            // Crear UI Elements para cada carta
            AvatarWindow cardAvatar = new AvatarWindow();
            avatarSelectionWindow.scrollView.Add(cardAvatar);

            cardAvatar.SetAvatarData(nft.metadata.name, nft.metadata.asset, i, urlVrm =>
            {

                if (GameObject.Find("VRM"))
                    Destroy(GameObject.Find("VRM"));

                IEnumerator downloadVRM = this.cryptoAvatars.GetAvatarVRMModel(urlVrm, (model) =>
                {
                    SetupModelAnimations(model);
                    SetupAvatarController(model);

                    //si ya esta el VRM en escena, lo seleccionamos como target de nuestro follow script que se encuentra en la camara
                    if (this.Cam)
                    {
                        var child = new GameObject();
                        child.name = "VRM_Child";
                        child.transform.localPosition = new Vector3(0, 1, 0);
                        child.transform.localRotation = Quaternion.Euler(0, -180, 0);
                        child.transform.parent = model.transform;
                        this.Cam.GetComponent<SmoothFollow>().target = child.transform;
                    }
                });
                StartCoroutine(downloadVRM);
                HideAvatarSelection();
                ShowPlayableWindow();
            }, urlVrm =>
            {
                if (GameObject.Find("VRM"))
                    Destroy(GameObject.Find("VRM"));
                IEnumerator downloadVRM = this.cryptoAvatars.GetAvatarVRMModel(urlVrm, (model) =>
                {
                    SetupModelAnimations(model);
                    this.Cam.transform.position = new Vector3(this.Cam.transform.position.x, this.Cam.transform.position.y + 2, this.Cam.transform.position.z);
                    this.Cam.GetComponent<SmoothFollow>().previewMode = true;
                    StartCoroutine(RotateAroundAvatar(model, 40f));
                });
                StartCoroutine(downloadVRM);
                HideAvatarSelection();
                ShowPlayableWindow();
            });

            IEnumerator loadAvatarPreviewImage = this.cryptoAvatars.GetAvatarPreviewImage(nft.metadata.image, texture =>
            {
                cardAvatar.LoadAvatarImage(texture);
            });

            StartCoroutine(loadAvatarPreviewImage);
        }

    }

    private static void SetupAvatarController(GameObject model)
    {
        model.AddComponent<ThirdPersonCharacter>();
        model.GetComponent<ThirdPersonCharacter>().m_JumpPower = 5.5f;
        model.GetComponent<ThirdPersonCharacter>().m_GroundCheckDistance = 0.4f;
        model.AddComponent<ThirdPersonUserControl>();
    }

    private static void SetupModelAnimations(GameObject model)
    {
        model.transform.Rotate(new Vector3(0, 180, 0));
        model.GetComponent<Animator>().runtimeAnimatorController = Resources.Load("Anims/Animator/ThirdPersonAnimatorController") as RuntimeAnimatorController;

        //Adjust axis (It comes with Y and Z flipped) (Blender)

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0, v);
        model.transform.InverseTransformDirection(dir);

        //model.transform.position += new Vector3(0, GameObject.Find("Cylinder").transform.localScale.y, 0);

        ////STANDARD ASSETS

        SkinnedMeshRenderer[] comps = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        Vector3 totalSize = new Vector3(0, 0, 0);
        for (int j = 0; j < comps.Length; j++)
        {
            totalSize += comps[j].bounds.size;
        }
        //Debug.Log("Avatar Size: ");
        //Debug.Log(totalSize);

        model.AddComponent<CapsuleCollider>();
        model.GetComponent<CapsuleCollider>().radius = 0.2f;
        model.GetComponent<CapsuleCollider>().height = totalSize.y;
        model.GetComponent<CapsuleCollider>().center = new Vector3(0.0f, totalSize.y / 2.0f, 0.0f);
        model.AddComponent<Rigidbody>().useGravity = true;
    }

    IEnumerator disablePageButton(float seconds)
    {
        avatarSelectionWindow.BackToLoginRequested -= backToLogin;
        avatarSelectionWindow.LoadPreviousRequested -= loadPreviousNfts;
        avatarSelectionWindow.LoadMoreRequested -= loadMoreNfts;
        yield return new WaitForSeconds(seconds);
        avatarSelectionWindow.BackToLoginRequested += backToLogin;
        avatarSelectionWindow.LoadPreviousRequested += loadPreviousNfts;
        avatarSelectionWindow.LoadMoreRequested += loadMoreNfts;
    }
    private IEnumerator RotateAroundAvatar(GameObject model, float speed)
    {
        for (; ; )
        {
            Cam.transform.LookAt(model.transform);
            Cam.transform.RotateAround(model.transform.position, Vector3.up, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
    private void ShowPlayableWindow()
    {
        root.Add(playableAvatarWindow);
    }
    private void HidePlayableWindow()
    {
        root.Remove(playableAvatarWindow);
    }
    private void downloadAvatars(string pageUrl)
    {
        removeCurrentAvatarsCards();
        IEnumerator getAvatars = cryptoAvatars.GetAvatarsByCollectionName(this.collectionNameSelected, this.licenseType, pageUrl, onAvatarsResult => displayAndLoadAvatars(onAvatarsResult));
        StartCoroutine(getAvatars);
    }
    private void downloadCollections(string pageUrl)
    {
        // Get Collections and Generate Options based on that
        IEnumerator getCollections = cryptoAvatars.GetNFTsCollections(onCollectionsResult => displayAndLoadCollections(onCollectionsResult), pageUrl);
        StartCoroutine(getCollections);
    }

    private void displayAndLoadCollections(NftsCollectionsArray onCollectionsResult)
    {
        List<string> choices = new List<string>();
        foreach (NftsCollection collection in onCollectionsResult.nftsCollections)
        {
            Debug.Log("Collection: " + $"{collection.name}");
            choices.Add(collection.name);
        }
        avatarSelectionWindow.collectionSelector.choices = choices;
        //downloadAvatars("");
    }

    private void downloadAvatarsUsers(string pageUrl)
    {
        removeCurrentAvatarsCards();
        if (avatarSelectionWindow.openSourceToggle.value)
        {
            IEnumerator getAvatars = cryptoAvatars.GetAvatarsByCollectionName(this.collectionNameSelected, this.licenseType, pageUrl, onAvatarsResult => displayAndLoadAvatars(onAvatarsResult));
            StartCoroutine(getAvatars);
        }
        //Use userWallet
        IEnumerator getAvatarsUser = cryptoAvatars.GetUserAvatarsByCollectionName(this.collectionNameSelected, this.userWallet, pageUrl, onAvatarsResult => displayAndLoadAvatars(onAvatarsResult));
        StartCoroutine(getAvatarsUser);

    }
}
