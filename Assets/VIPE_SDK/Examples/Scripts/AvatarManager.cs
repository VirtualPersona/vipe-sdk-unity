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
    public class AvatarManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject avatarCardPrefab;

        [SerializeField]
        private GameObject avatarsScrollObject;

        private ScrollRect avatarsScroll => avatarsScrollObject.GetComponent<ScrollRect>();

        private ToggleGroup avatarToggleGroup => avatarsScroll.GetComponent<ToggleGroup>();

        [SerializeField]
        private TMP_Text currentPageText;

        /// <summary>Displays a list of avatars in the scroll view./// </summary>
        /// <param name="avatarsResult">The data containing avatars to display.</param>
        public async void DisplayAvatars(Models.NftsArray avatarsResult, CancellationTokenSource cts)
        {
            Debug.Log("Displaying avatars");
            Debug.Log(cts.IsCancellationRequested);
            Debug.Log(cts);
            ClearScrollView();
            await Task.Delay(1);
            //timeout

            try
            {
                Models.Nft[] nfts = avatarsResult.nfts;
                currentPageText.text = avatarsResult.currentPage.ToString() + " | " + avatarsResult.totalPages;

                foreach (var nft in nfts)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    GameObject avatarCard = Instantiate(
                        avatarCardPrefab,
                        avatarsScroll.content.transform
                    );

                    CardManager cardManager =
                        avatarCard.GetComponent<CardManager>();

                    cardManager.SetCardData(
                        nft.metadata.name,
                        nft.metadata.asset,
                        urlVRM => MainManager.vrmManager.LoadVRMModel(urlVRM)
                    );

                    cardManager.SetToggleGroup(avatarToggleGroup);

                    MainManager.VIPE.GetAvatarPreviewImage(
                        nft.metadata.image,
                        texture => cardManager.LoadCardImage(texture)
                    );
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Operation canceled in DisplayAvatars");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error displaying avatars: " + ex.Message);
            }

        }

        /// <summary>
        /// Removes the Cards from the scroll.
        /// </summary>        
        public void ClearScrollView()
        {
            if (avatarsScroll)
                foreach (Transform child in avatarsScroll.content)
                {
                    Destroy(child.gameObject);
                }
        }
    }
}