using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardCollectionController : MonoBehaviour
{
    public Image imageCollection;
    public TextMeshProUGUI textCollection;

    private void Awake()
    {
        imageCollection = GetComponent<Image>();
        textCollection = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetCollectionData(string name, string vrmUrl, System.Action<string> onClickCardBtn)
    {
        this.textCollection.text = name;
        this.gameObject.name = name;
        Button btnPreview = GetComponent<Button>();
        btnPreview.onClick.AddListener(() => onClickCardBtn(vrmUrl));
    }

    public void LoadCollectionImage(Texture2D texture)
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
