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
    public class CollectionManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject collectionCardPrefab;

        [SerializeField]
        private GameObject collectionsScrollObject;
        private ScrollRect collectionsScroll => collectionsScrollObject.GetComponent<ScrollRect>();
        private ToggleGroup collectionToggleGroup => collectionsScroll.GetComponent<ToggleGroup>();

        /// <summary>
        /// Loads NFT collections and their associated avatars.
        /// </summary>
        public async void DisplayCollections(CancellationTokenSource cts)
        {
            Debug.Log("Displaying collections" + cts.IsCancellationRequested);
            try
            {
                await MainManager.VIPE.GetNFTCollections((Action<Models.NftCollectionsArray>)(async (collections) =>
                {
                    List<string> options = new List<string>();

                    for (int i = 0; i < collections.nftCollections.Length; i++)
                    {
                        //State of the task
                        //cts.Token.ThrowIfCancellationRequested();

                        string slug = collections.nftCollections[i].slug;

                        var parameters = new Dictionary<string, string>
                        {
                            {"license", "CC0" },
                            {"limit","6" },
                            {"collectionSlug", slug},
                        };

                        GameObject avatarCard = Instantiate(
                            collectionCardPrefab,
                            collectionsScroll.content.transform
                        );

                        CardManager cardManager =
                            avatarCard.GetComponent<CardManager>();

                        Debug.Log("Displaying collections loop" + cts.IsCancellationRequested);
                        cardManager.SetCardData(
                                 collections.nftCollections[i].slug,
                                 collections.nftCollections[i].logoImage,
                                 (Action<string>)(async collectionName =>
                                     await MainManager.VIPE.GetAvatars((avatarsResult) => MainManager.avatarManager.DisplayAvatars(avatarsResult, cts), parameters))
                             );

                        cardManager.SetToggleGroup(collectionToggleGroup);

                        Task task = MainManager.VIPE.GetAvatarPreviewImage(
                            collections.nftCollections[i].logoImage,
                            texture => cardManager.LoadCardImage(texture)
                        );
                    }
                }));
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Operaci�n cancelada");
            }
        }

        /// <summary>
        /// Clears the collection scroll view by destroying its child objects.
        /// </summary>
        public void ClearCollectionsDisplay()
        {
            if (collectionsScroll)
            {
                foreach (Transform child in collectionsScroll.content.transform)
                {
                    if (child.gameObject.name != "AllCollectionsCard")
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
        }
    }
}