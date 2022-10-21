using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

 namespace BloodUI
{
    // If creating a new Control it should inherit from Visual Element
    public class PopupWindow : VisualElement
    {
        // This line will enable our namespace been visible in the UI Toolkit Editor
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<PopupWindow, UxmlTraits> { }
        public event Action LoginRequested;
        public event Action GuestRequested;
        private const string styleResourcePATH = "UI Toolkit/Styles/PopupStyle";
        private const string ussPopupClass = "popup_window";
        private const string ussPopupContainerClass = "popup_container";
        private const string ussPopupHorizontalContainer = "horizontal_container";
        private const string ussPopupMessage = "popup_message";
        public PopupWindow()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResourcePATH));
            AddToClassList(ussPopupContainerClass);
            // The constructor is called when the control is created
            //VisualElement window = new VisualElement();
            //// Add class list to the VisualElement
            //window.AddToClassList(ussPopupClass);
            //hierarchy.Add(window);
            // Texts
            //VisualElement horizontalContainerText = new VisualElement();
            //horizontalContainerText.AddToClassList(ussPopupHorizontalContainer);
            //window.Add(horizontalContainerText);
            //Label msgLabel = new Label();
            //msgLabel.text = "Login";
            //msgLabel.AddToClassList(ussPopupMessage);
            //horizontalContainerText.Add(msgLabel);
            TextField EmailTextField = new TextField() { label = "Email" };
            TextField PasswordTextField = new TextField() { label = "Password", isPasswordField = true};
            Button LoginButton = new Button() { text = "Login" };
            Button GuestButton = new Button() { text = "Enter as Guest" };
            //Button LoginButton = this.Query<Button>("VisualElementsButton").Children<Button>().First();
            //Button GuestButton = this.Query<Button>("VisualElementsButton").Children<Button>().AtIndex(1);
            //if (LoginButton != null)
            //{
            //    LoginButton.clicked += () => LoginRequested?.Invoke();
            //}
            //if (GuestButton != null)
            //{
            //    GuestButton.clicked += () => GuestRequested?.Invoke();
            //}
            hierarchy.Add(EmailTextField);
            hierarchy.Add(PasswordTextField);

            hierarchy.Add(LoginButton);
            hierarchy.Add(GuestButton);
            LoginButton.clicked += OnLoginButtonClicked;
            GuestButton.clicked += OnGuestButtonClicked;

        }
        private void OnLoginButtonClicked()
        {
            Debug.Log("Login Button Clicked");
            LoginRequested?.Invoke();
        }
        private void OnGuestButtonClicked()
        {
            Debug.Log("Guest Button Clicked");
            GuestRequested?.Invoke();
        }
    }
}
