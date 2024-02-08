using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

namespace VIPE_SDK
{
    public class VRMManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject Avatar;

        [SerializeField]
        private GameObject loadingSpinner;

        /// <summary>Displays a list of avatars in the scroll view. </summary>
        /// <param name="onAvatarsResult">The data containing avatars to display.</param>
        private void ReplaceVRMModel(GameObject model)
        {
            Vector3 avatarPos = Avatar.transform.position;
            Quaternion avatarRot = Avatar.transform.rotation;
            Destroy(Avatar);
            Avatar = model;
            Avatar.name = "AVATAR";
            Avatar.transform.SetPositionAndRotation(avatarPos, avatarRot);
            Avatar.GetComponent<Animator>().runtimeAnimatorController =
                Resources.Load<RuntimeAnimatorController>(
                    "Anims/Animator/ThirdPersonAnimatorController"
                );
            Avatar.AddComponent<ResizeCapsuleCollider>();
            Avatar.AddComponent<ThirdPersonUserControl>();
            Camera.main.GetComponent<OrbitCamera>().targetPosition = Avatar.transform;
        }


        /// <summary>Loads a VRM model based on a URL.</summary>
        /// <param name="urlVRM">The URL of the VRM model to load.</param>
        public async void LoadVRMModel(string urlVRM)
        {
            loadingSpinner.SetActive(true);
            await MainManager.VIPE.GetAvatarVRMModel(
                urlVRM,
                (model, path) =>
                {
                    ReplaceVRMModel(model);
                    loadingSpinner.SetActive(false);
                }
            );
        }
    }
}