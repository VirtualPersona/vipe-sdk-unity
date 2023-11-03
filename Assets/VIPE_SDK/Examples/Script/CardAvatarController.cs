using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VIPE_SDK
{
    public class CardAvatarController : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text avatarName;

        private string vrmUrl;
        /// <summary>
        /// Sets the avatar data on the card, including name, VRM URL, and click action.
        /// </summary>
        /// <param name="name">The name of the avatar.</param>
        /// <param name="vrmUrl">The URL to the VRM model associated with the avatar.</param>
        /// <param name="onClickCardBtn">The action to perform when the card is clicked.</param>
        public void SetAvatarData(string name, string vrmUrl, System.Action<string> onClickCardBtn)
        {
            this.vrmUrl = vrmUrl;
            this.avatarName.text = name;
            this.gameObject.name = name;
            Button btnPreview = GetComponent<Button>();
            btnPreview.onClick.AddListener(() => onClickCardBtn(this.vrmUrl));
        }
        /// <summary>
        /// Loads the avatar's image onto the card.
        /// </summary>
        /// <param name="texture">The avatar's image as a Texture2D.</param>
        public void LoadAvatarImage(Texture2D texture)
        {
            if (!texture)
                return;

            Rect rec = new(0, 0, texture.width, texture.height);
            if (GetComponent<Image>())
            {
                Image imageAvatar = GetComponent<Image>();
                imageAvatar.sprite = Sprite.Create(texture, rec, new Vector2(0, 0), 1);
            }
        }
    }
}
