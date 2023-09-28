using UnityEngine;
using UnityEngine.UI;

public class MultiRowImageMovement : MonoBehaviour
{
    public float speed = 1.0f;
    public float alpha = 1.0f;
    private Material material;

    private void Start()
    {
        Image image = GetComponent<Image>();
        material = Instantiate(image.material);
        image.material = material;
    }

    private void LateUpdate()
    {
        float offset = material.GetFloat("_OffsetX");

        // 1. Obtener el color actual
        Color color = material.GetColor("_Color");

        // 2. Modificar el componente alfa
        color.a = this.alpha;

        offset += speed * Time.deltaTime;
        offset = offset % 1.0f;

        // 3. Establecer el color modificado de nuevo en el material
        material.SetColor("_Color", color);

        material.SetFloat("_OffsetX", offset);
    }

}