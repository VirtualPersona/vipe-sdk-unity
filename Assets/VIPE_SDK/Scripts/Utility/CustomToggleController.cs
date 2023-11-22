using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
namespace VIPE_SDK
{

    [RequireComponent(typeof(Toggle))]
    public class CustomToggleController : MonoBehaviour
    {
        private Toggle toggle;

        [Header("Event when Toggle is activated")]
        public UnityEvent onToggleActivated;

        [Header("UI Components")]
        private TextMeshProUGUI textComponent;
        public Image interiorImage;

        [Header("Text Colors")]
        public Color activeColor = Color.white;
        public Color inactiveColor = Color.black;

        void Start()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnToggleValueChanged);

            textComponent = transform.GetComponentInChildren<TextMeshProUGUI>();

            ToggleGroup toggleGroup = GetComponentInParent<ToggleGroup>();
            if (toggleGroup)
            {
                toggle.group = toggleGroup;
            }
        }

        public void OnToggleValueChanged(bool isOn)
        {
            if (isOn && onToggleActivated != null)
            {
                onToggleActivated.Invoke();
            }

            if (interiorImage != null)
                interiorImage.enabled = isOn;

            setToggleColor(isOn);
        }

        private void setToggleColor(bool isOn)
        {
            if (textComponent != null)
            {
                textComponent.color = isOn ? activeColor : inactiveColor;
            }
        }
    }
}