using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace BloodUI
{
    public class AvatarPlayableWindow : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<AvatarPlayableWindow, UxmlTraits> { }
        public event Action BackToAvatarSelectionRequested;

        //Elements
        public Image logo;
        public Button backToAvatarSelectionButton;

        public AvatarPlayableWindow()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("UI Toolkit/Styles/PlayableUIStyle"));
            logo = new Image();
            const string ussPlayableUIContainer = "playable_ui_container";
            VisualElement PlayableUIContainer = new VisualElement() { name = ussPlayableUIContainer };
            PlayableUIContainer.AddToClassList(ussPlayableUIContainer);
            backToAvatarSelectionButton = new Button() { text = "Back to Avatar Selection" };
            // Resources References
            logo.image = Resources.Load<Texture2D>("Logos/logo");
            // Elements Hierarchy
            PlayableUIContainer.Add(logo);
            PlayableUIContainer.Add(backToAvatarSelectionButton);
            hierarchy.Add(PlayableUIContainer);
            // Elements Events
            backToAvatarSelectionButton.clicked += () => BackToAvatarSelectionRequested?.Invoke();
        }
    }
}
