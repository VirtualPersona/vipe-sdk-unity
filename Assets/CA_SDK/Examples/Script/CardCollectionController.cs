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
    /// <summary>
    /// Sets the collection data on the card, including name, VRM URL, and click action.
    /// </summary>
    /// <param name="name">The name of the collection.</param>
    /// <param name="vrmUrl">The URL to the VRM model associated with the collection.</param>
    /// <param name="onClickCardBtn">The action to perform when the card is clicked.</param>
    public void SetCollectionData(string name, string vrmUrl, System.Action<string> onClickCardBtn)
    {
        if (!this.textCollection) return;

        this.textCollection.text = name;
        this.gameObject.name = name;
        Button btnPreview = GetComponent<Button>();
        btnPreview.onClick.AddListener(() => onClickCardBtn(vrmUrl));
    }
    /// <summary>
    /// Loads the collection's image onto the card.
    /// </summary>
    /// <param name="texture">The collection's image as a Texture2D.</param>
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
