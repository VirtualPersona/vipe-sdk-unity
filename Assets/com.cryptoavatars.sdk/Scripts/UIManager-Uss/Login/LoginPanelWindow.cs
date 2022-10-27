using System;
using UnityEngine;
using UnityEngine.UIElements;
// Namespace to avoid possible name collisions
namespace BloodUI
{
    public class LoginPanelWindow : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<LoginPanelWindow, UxmlTraits> { }
        public event Action LoginRequested;
        public event Action GuestRequested;
        // Style Path
        private const string styleResourcePATH = "UI Toolkit/Styles/LoginPanelStyle";
        private const string imageResourcePATH = "Logos/logo";
        private const string ussLoginPanel = "login_panel";
        private const string ussLoginPanelButtonContainer = "login_panel_button_container";
        // Elements                 // default dimensions
        public Image logo;                     // 500x100
        public TextField emailTextField;       // 320x43
        public TextField passwordTextField;    // 320x43
        public Button loginButton;             // 200x40
        public Button guestButton;             // 200x40

        public LoginPanelWindow()
        {
            // Elements Initialization
            VisualElement LoginPanelContainer = new VisualElement() { name = "LoginPanelContainer"};
            style.alignSelf = Align.Center;
            style.marginTop = 250;
            style.width = 838;
            style.height = 346;
            LoginPanelContainer.style.width = 838;
            LoginPanelContainer.style.height = 346;
            LoginPanelContainer.AddToClassList(ussLoginPanel);
            logo = new Image();
            emailTextField = new TextField() { label = "Email" };
            passwordTextField = new TextField() { label = "Password", isPasswordField = true };
            loginButton = new Button() { text = "Login" };
            guestButton = new Button() { text = "Enter as Guest" };
            // Resource References
            styleSheets.Add(Resources.Load<StyleSheet>(styleResourcePATH));
            logo.image = Resources.Load<Texture2D>(imageResourcePATH);
            // Add to Hierarchy
            hierarchy.Add(LoginPanelContainer);
            LoginPanelContainer.Add(logo);
            LoginPanelContainer.Add(emailTextField);
            LoginPanelContainer.Add(passwordTextField);
            VisualElement LoginPanelButtonContainer = new VisualElement() { name = "LoginPanelButtonContainer" };
            LoginPanelButtonContainer.AddToClassList(ussLoginPanelButtonContainer);
            LoginPanelContainer.Add(LoginPanelButtonContainer);
            LoginPanelButtonContainer.Add(loginButton);
            LoginPanelButtonContainer.Add(guestButton);
            // Add Functionality
            loginButton.clicked += OnLoginButtonClicked;
            guestButton.clicked += OnGuestButtonClicked;
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

