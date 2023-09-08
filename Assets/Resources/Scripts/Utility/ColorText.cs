using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ColorText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI textComponent;
    public Color defaultColor = Color.black;
    public Color hoverColor = Color.red;
    void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        textComponent.color = hoverColor;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        textComponent.color = defaultColor;
    }
}