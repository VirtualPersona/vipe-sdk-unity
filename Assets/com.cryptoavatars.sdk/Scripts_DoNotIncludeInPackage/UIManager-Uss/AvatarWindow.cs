
using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace BloodUI
{
    public class AvatarWindow : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<AvatarWindow, UxmlTraits> { };
        public event Action<string> previewAvatarRequested;
        // Elements
        public Label avatarName; // 160x30
        public Button preview3DButton; // 160x40
        public Image avatarImage; // Image - 250x350
        // Data
        private string vrmUrl;
        private int index;
        private Texture2D textureAvatar;
        // Style Values Path
        private const string styleResourcePATH = "UI Toolkit/Styles/AvatarViewStyle";

        public void SetAvatarData(string name, string vrmUrl, int index, Action<string> onClickCardBtn)
        {
            this.previewAvatarRequested = onClickCardBtn;
            this.vrmUrl = vrmUrl;
            this.index = index;
            avatarName.text = name;
            preview3DButton.clicked += () => previewAvatarRequested?.Invoke(this.vrmUrl);
            preview3DButton.clicked += ResetButtonClick;
        }
        private void ResetButtonClick()
        {
            previewAvatarRequested = null;
        }
        public void LoadAvatarImage(Texture2D texture)
        {
            this.textureAvatar = texture;
            avatarImage.image = this.textureAvatar;
        }
        public AvatarWindow()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResourcePATH));
            avatarName = new Label(text:"Avatar Name");
            avatarImage = new Image();
            preview3DButton = new Button() { text = "Preview 3D" };
            VisualElement cardContainer = new VisualElement();
            // Add to hierarchy
            hierarchy.Add(cardContainer);
            cardContainer.Add(avatarImage);
            cardContainer.Add(avatarName);
            cardContainer.Add(preview3DButton);
        }
    }
}
