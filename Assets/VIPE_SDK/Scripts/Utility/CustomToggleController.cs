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
        public Image interiorButton;

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

        private void OnToggleValueChanged(bool isOn)
        {
            if (isOn && onToggleActivated != null)
            {
                onToggleActivated.Invoke();
            }

            if (interiorButton != null)
                interiorButton.enabled = isOn;
            if (textComponent != null)
                textComponent.color = isOn ? activeColor : inactiveColor;
        }
    }
}