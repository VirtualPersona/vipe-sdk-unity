
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
        public event Action<string> playWithAvatarRequested;
        // Elements
        public Label avatarName; // 160x30
        public Button previewButton;
        public Button playWithAvatarButton; // 160x40
        public Image avatarImage; // Image - 250x350
        public Image avatarOwnedFilter;
        // Data
        private string vrmUrl;
        private int index;
        private Texture2D textureAvatar;
        // Style Values Path
        private const string styleResourcePATH = "UI Toolkit/Styles/AvatarViewStyle";
        private const string ussButtonsContainer = "avatar_window_buttons_container";
        public void SetAvatarData(string name, string vrmUrl, int index, Action<string> onClickPlayWithAvatarButton, Action<string> onClickPreviewButton)
        {
            this.playWithAvatarRequested = onClickPlayWithAvatarButton;
            this.previewAvatarRequested = onClickPreviewButton;
            this.vrmUrl = vrmUrl;
            this.index = index;
            avatarName.text = name;
            playWithAvatarButton.clicked += () => playWithAvatarRequested?.Invoke(this.vrmUrl);
            previewButton.clicked += () => previewAvatarRequested?.Invoke(this.vrmUrl);
            playWithAvatarButton.clicked += ResetButtonsClick;
            previewButton.clicked += ResetButtonsClick;
        }

        private void ResetButtonsClick()
        {
            previewAvatarRequested = null;
            playWithAvatarRequested = null;
        }
        public void LoadAvatarImage(Texture2D texture)
        {
            this.textureAvatar = texture;
            avatarImage.image = this.textureAvatar;
        }
        public void LoadAvatarImage(Texture2D texture, bool owned)
        {
            this.textureAvatar = texture;
            avatarImage.image = this.textureAvatar;
            if (!owned)
                avatarImage.tintColor = new Color(0, 0, 0, 0.4f);
        }
        public AvatarWindow()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResourcePATH));
            avatarName = new Label(text:"Avatar Name");
            avatarImage = new Image();
            playWithAvatarButton = new Button() { text = "Play with Avatar" };
            previewButton = new Button() { text = "Preview 3D" };
            VisualElement cardContainer = new VisualElement();
            // Add to hierarchy
            hierarchy.Add(cardContainer);
            cardContainer.Add(avatarImage);
            cardContainer.Add(avatarName);
            VisualElement buttonsContainer = new VisualElement { name = "buttonsContainer" };
            buttonsContainer.AddToClassList(ussButtonsContainer);
            buttonsContainer.Add(playWithAvatarButton);
            buttonsContainer.Add(previewButton);
            cardContainer.Add(buttonsContainer);
        }
    }
}
