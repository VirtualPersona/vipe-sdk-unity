using UnityEngine.UIElements;
using UnityEngine;
using Unity.VectorGraphics;

namespace BloodUI
{
    public class PopupSetup : MonoBehaviour
    {
        private UIDocument doc;
        private VisualElement root;
        private PopupWindow popup;
        private SVGImage cryptoAvatarsImage;

        private void OnEnable()
        {
            doc = GetComponent<UIDocument>();
            root = doc.rootVisualElement;
            TryGetComponent(out cryptoAvatarsImage);
        }
       
        public void Popup_Instantiate()
        {
            if(popup == null)
            {
                popup = new PopupWindow();
                popup.LoginRequested += () => Debug.Log("Login in");
                popup.GuestRequested += () => Debug.Log("Enter as guest");
            }
            Debug.Log("Enter");
            root.Add(popup);
            cryptoAvatarsImage.color = new Color(0,0,0,0);
        }

        public void Popup_Hide()
        {
            Debug.Log("EXit");
            if (popup != null)
                root.Remove(popup);
            
            cryptoAvatarsImage.color = Color.white;
            cryptoAvatarsImage.enabled = true;
        }
    }
}

