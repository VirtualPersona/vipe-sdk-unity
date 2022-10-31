using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BloodUI
{
    public class AvatarSelectionWindow : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<AvatarSelectionWindow, UxmlTraits> { }
        public event Action BackToLoginRequested;
        public event Action LoadMoreRequested;
        public event Action LoadPreviousRequested;
        public event Action<string> OnLoadCollectionsRequested;
        public event Action<string> OnSearchAvatarRequested;
        public event Action<string> OnSourceSelectorRequested;
        // Style Path
        private const string styleResourcePATH = "UI Toolkit/Styles/CardAvatarListStyle";
        private const string imageResourcePATH = "Logos/logo";
        private const string ussBackToLoginContainer = "back_to_login_container";
        private const string ussPagination_container = "pagination_container";
        private const string ussSelector_container = "selector_container";
        private const string ussHeader_container = "header_container";
        private const string ussAvatarScrollContainer = "avatar_scroll_container";

        // Elements                 // default dimensions
        public Image logo;                      // 365x132
        public DropdownField collectionSelector;// 290x40
        public DropdownField sourceSelector;    // 290x40
        public TextField paginationTextField;   // 110x35
        public TextField searchAvatarTextField; // 290x40
        public ScrollView scrollView;           // 1600x920
        public Button backToLoginButton;        // 250x30
        public Button loadMoreButton;           // 35x35
        public Button loadPreviousButton;       // 35x35

        public AvatarSelectionWindow()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResourcePATH));
            logo = new Image();
            collectionSelector = new DropdownField()
            {
                label = "Collection",
                choices = new System.Collections.Generic.List<string>() { "CryptoAvatars" },
                value = "CryptoAvatars"
            };
            sourceSelector = new DropdownField()
            {
                label = "Source",
                choices = new System.Collections.Generic.List<string>() { "All", "Owned", "Open Source" },
                value = "All"
            };
            
            searchAvatarTextField = new TextField() { value = "Search Avatar name here" };
            paginationTextField = new TextField();
            scrollView = new ScrollView();
            backToLoginButton = new Button() { text = "Back to Login" };
            loadMoreButton = new Button() { text = ">" };
            loadPreviousButton = new Button() { text = "<" };
            // Resources Referencess
            logo.image = Resources.Load<Texture2D>(imageResourcePATH);
            // Elements default Dimensions
            logo.style.width = 365;
            logo.style.height = 132;
            collectionSelector.style.width = 490;
            collectionSelector.style.height = 40;
            sourceSelector.style.width = 490;
            sourceSelector.style.height = 40;
            searchAvatarTextField.style.width = 320;
            searchAvatarTextField.style.height = 43;
            paginationTextField.style.width = 110;
            paginationTextField.style.height = 35;
            backToLoginButton.style.width = 250;
            backToLoginButton.style.height = 30;
            loadMoreButton.style.width = 35;
            loadMoreButton.style.height = 35;
            loadPreviousButton.style.width = 35;
            loadPreviousButton.style.height = 35;
            // Add to Hierarchy
            VisualElement HeaderContainer = new VisualElement() { name = "Header Container" };
            hierarchy.Add(HeaderContainer);
            HeaderContainer.AddToClassList(ussHeader_container);
            HeaderContainer.Add(logo);
            VisualElement SelectorsContainer = new VisualElement() { name = "Selectors Container" };
            HeaderContainer.Add(SelectorsContainer);
            VisualElement SelectorContainer = new VisualElement() { name = ussSelector_container };
            SelectorContainer.AddToClassList(ussSelector_container);
            SelectorContainer.Add(collectionSelector);
            VisualElement SelectorContainer2 = new VisualElement() { name = ussSelector_container };
            SelectorContainer2.AddToClassList(ussSelector_container);
            SelectorContainer2.Add(sourceSelector);
            SelectorsContainer.Add(SelectorContainer);
            SelectorsContainer.Add(SelectorContainer2);
            VisualElement BackToLoginContainer = new VisualElement() { name = ussBackToLoginContainer };
            VisualElement SearchBarContainer = new VisualElement();
            SearchBarContainer.Add(searchAvatarTextField);
            SearchBarContainer.AddToClassList(ussBackToLoginContainer);
            HeaderContainer.Add(SearchBarContainer);
            BackToLoginContainer.Add(backToLoginButton);
            VisualElement PaginationContainer = new VisualElement() { name = ussPagination_container };
            PaginationContainer.AddToClassList(ussPagination_container);
            BackToLoginContainer.Add(PaginationContainer);
            HeaderContainer.Add(BackToLoginContainer);
            PaginationContainer.Add(loadPreviousButton);
            PaginationContainer.Add(paginationTextField);
            PaginationContainer.Add(loadMoreButton);
            BackToLoginContainer.AddToClassList(ussBackToLoginContainer);
            VisualElement AvatarScrollContainer = new VisualElement() { name = "AvatarScrollContainer" };
            AvatarScrollContainer.Add(scrollView);
            AvatarScrollContainer.AddToClassList(ussAvatarScrollContainer);
            hierarchy.Add(AvatarScrollContainer);
            // Add functionality
            // Buttons
            backToLoginButton.clicked += OnBackToLoginButtonClicked;
            loadMoreButton.clicked += OnLoadMoreButtonClicked;
            loadPreviousButton.clicked += OnLoadPreviousButtonClicked;
            // Dropdown Collection
            collectionSelector.RegisterValueChangedCallback(x => OnLoadCollectionsRequested?.Invoke(collectionSelector.value));
            // Dropdown License
            sourceSelector.RegisterValueChangedCallback(x => OnSourceSelectorRequested?.Invoke(sourceSelector.value));
            // Search Bar
            searchAvatarTextField.RegisterValueChangedCallback(x => OnSearchAvatarRequested?.Invoke(searchAvatarTextField.value));
        }

        private void OnBackToLoginButtonClicked()
        {
            BackToLoginRequested?.Invoke();
        }
        private void OnLoadMoreButtonClicked()
        {
            LoadMoreRequested?.Invoke();
        }

        private void OnLoadPreviousButtonClicked()
        {
            LoadPreviousRequested?.Invoke();
        }

    }
}