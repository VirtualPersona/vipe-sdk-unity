using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class AvatarPlayMode : MonoBehaviour
{
    public Button playStopButton;
    public TMP_Text buttonText;
    public GameObject userInterfaceCanvas;
    private bool isPlaying = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private RuntimeAnimatorController initialAnimatorController;

    public delegate void PlayModeChanged(bool isPlaying, GameObject VRM);
    public static event PlayModeChanged OnPlayModeChanged;

    public void TogglePlayStop()
    {
        GameObject VRM = GameObject.Find("AVATAR");

        if (!isPlaying)
        {
            initialPosition = VRM.transform.position;
            initialRotation = VRM.transform.rotation;
            initialAnimatorController = VRM.GetComponent<Animator>().runtimeAnimatorController;
            SetUpPlayMode(VRM);
            buttonText.text = "Stop";
            userInterfaceCanvas.SetActive(false);
        }                                
        else                             
        {                                
            ResetAvatar(VRM);            
            buttonText.text = "Play";
            userInterfaceCanvas.SetActive(true);
        }

        isPlaying = !isPlaying;
        OnPlayModeChanged?.Invoke(isPlaying, VRM);
    }

    private void SetUpPlayMode(GameObject VRM)
    {
        SetUpAnimator(VRM);
        SetUpCapsuleCollider(VRM);
        SetUpRigidbody(VRM);
        SetUpThirdPersonCharacter(VRM);
        SetUpThirdPersonUserControl(VRM);
    }

    private void ResetAvatar(GameObject VRM)
    {
        VRM.transform.position = initialPosition;
        VRM.transform.rotation = initialRotation;
        VRM.GetComponent<Animator>().runtimeAnimatorController = initialAnimatorController;

        VRM.GetComponent<ThirdPersonUserControl>().enabled = false;
        VRM.GetComponent<ThirdPersonCharacter>().enabled = false;
        VRM.GetComponent<Rigidbody>().isKinematic = true;
        VRM.GetComponent<CapsuleCollider>().enabled = false;
    }

    private void SetUpAnimator(GameObject VRM)
    {
        Animator animator = VRM.GetComponent<Animator>();
        animator.runtimeAnimatorController = Resources.Load("Anims/Animator/ThirdPersonAnimatorController") as RuntimeAnimatorController;
    }

    private void SetUpCapsuleCollider(GameObject VRM)
    {
        CapsuleCollider capsuleCollider = VRM.GetComponent<CapsuleCollider>();
        if (capsuleCollider == null)
        {
            capsuleCollider = VRM.AddComponent<CapsuleCollider>();
        }
        capsuleCollider.radius = 0.2f;
        capsuleCollider.height = 1.8f;
        capsuleCollider.center = new Vector3(0, 0.9f, 0);
        capsuleCollider.enabled = true;
    }
    private void SetUpRigidbody(GameObject VRM)
    {
        Rigidbody rb = VRM.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = VRM.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;
    }

    private void SetUpThirdPersonCharacter(GameObject VRM)
    {
        ThirdPersonCharacter thirdPersonCharacter = VRM.GetComponent<ThirdPersonCharacter>();
        if (thirdPersonCharacter == null)
        {
            thirdPersonCharacter = VRM.AddComponent<ThirdPersonCharacter>();
        }
        thirdPersonCharacter.enabled = true;
    }

    private void SetUpThirdPersonUserControl(GameObject VRM)
    {
        ThirdPersonUserControl thirdPersonUserControl = VRM.GetComponent<ThirdPersonUserControl>();
        if (thirdPersonUserControl == null)
        {
            thirdPersonUserControl = VRM.AddComponent<ThirdPersonUserControl>();
        }
        thirdPersonUserControl.enabled = true;
    }
}
