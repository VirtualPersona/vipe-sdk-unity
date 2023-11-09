using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VIPE_SDK
{
    public class CardManager : MonoBehaviour
    {
        private Image cardImage;
        private TextMeshProUGUI cardText;
        private Button cardButton;

        private void Awake()
        {
            cardImage = GetComponent<Image>();
            cardText = GetComponentInChildren<TextMeshProUGUI>();
            cardButton = GetComponent<Button>();
        }


        /// <summary>
        /// Sets the avatar data on the card, including name, VRM URL, and click action.
        /// </summary>
        /// <param name="name">The name of the card.</param>
        /// <param name="cardUrl">The URL associated with the card.</param>
        /// <param name="onClickCardBtn">The action to perform when the card is clicked.</param>
        public void SetCardData(string name, string cardUrl, System.Action<string> onClickCardBtn)
        {
            cardText.text = name;
            gameObject.name = name;
            cardButton.onClick.AddListener(() => onClickCardBtn(cardUrl));
        }

        /// <summary>
        /// Loads the avatar's image onto the card.
        /// </summary>
        /// <param name="texture">The avatar's image as a Texture2D.</param>
        public void LoadCardImage(Texture2D texture)
        {
            if (!texture)
                return;

            Rect rec = new(0, 0, texture.width, texture.height);
            if (GetComponent<Image>())
            {
                cardImage.sprite = Sprite.Create(texture, rec, new Vector2(0, 0), 1);
            }
        }
    }
}
