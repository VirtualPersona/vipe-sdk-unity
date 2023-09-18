using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI textComponent;
    public Color defaultColor = Color.black;
    public Color hoverColor = Color.red;

    public Image interiorButton;
    void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        textComponent.color = hoverColor;
        interiorButton.enabled = !interiorButton.enabled;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        textComponent.color = defaultColor;
        interiorButton.enabled = !interiorButton.enabled;
    }
}