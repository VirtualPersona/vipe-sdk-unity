using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardAvatarController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text avatarName;

    private string vrmUrl;

    public void SetAvatarData(string name, string vrmUrl, System.Action<string> onClickCardBtn)
    {
        this.vrmUrl = vrmUrl;
        this.avatarName.text = name;
        this.gameObject.name = name;
        Button btnPreview = GetComponent<Button>();
        btnPreview.onClick.AddListener(() => onClickCardBtn(this.vrmUrl));
    }

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
