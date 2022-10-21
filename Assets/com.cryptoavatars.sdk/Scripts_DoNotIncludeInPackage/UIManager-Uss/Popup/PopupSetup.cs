using UnityEngine.UIElements;
using UnityEngine;

namespace BloodUI
{
    public class PopupSetup : MonoBehaviour
    {
        UIDocument doc;
        VisualElement root;
        VisualElement popupContainer;
        PopupWindow popup;
        [SerializeField]
        UnityEngine.UI.Image cryptoAvatarsImage;

        private void Start()
        {
            doc = GetComponent<UIDocument>();
            root = doc.rootVisualElement;
        }
        public void Popup_Instantiate()
        {
            popup = new PopupWindow();
            if (popupContainer == null)
                popupContainer = root.Query<VisualElement>("PopupContainer").First();
            popupContainer.Add(popup);
            popup.LoginRequested += () => Debug.Log("Login in");
            popup.GuestRequested += () => Debug.Log("Enter as guest");
        }

        public void Popup_Hide()
        {
            if (popup != null)
                popupContainer.Remove(popup);
            cryptoAvatarsImage.gameObject.SetActive(true);
        }
    }
}

