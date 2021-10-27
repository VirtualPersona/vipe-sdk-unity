using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardAvatarController : MonoBehaviour
{

    private string vrmUrl;
    private int index;
    private Texture2D textureAvatar;

    private System.Action<string> onClickCardBtn;

    public void SetAvatarData(string name, string vrmUrl, int index, System.Action<string> onClickCardBtn)
    {
        this.onClickCardBtn = onClickCardBtn;
        this.vrmUrl = vrmUrl;
        this.index = index;

        GetComponentInChildren<Text>().text = name;
        Button btnPreview = GetComponentInChildren<Button>();
        btnPreview.onClick.AddListener(ClickPreview);
    }

    public void LoadAvatarImage(Texture2D texture)
    {
        this.textureAvatar = texture;
        RawImage imageAvatar = GetComponentInChildren<RawImage>();
        imageAvatar.texture = this.textureAvatar;
    }

    private void ClickPreview()
    {
        this.onClickCardBtn(this.vrmUrl);
    }

}
