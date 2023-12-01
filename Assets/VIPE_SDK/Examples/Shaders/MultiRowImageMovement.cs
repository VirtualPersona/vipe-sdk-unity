using UnityEngine;
using UnityEngine.UI;

namespace VIPE_SDK
{
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
            Color color = material.GetColor("_Color");
            color.a = this.alpha;
            offset += speed * Time.deltaTime;
            offset = offset % 1.0f;
            material.SetColor("_Color", color);
            material.SetFloat("_OffsetX", offset);
        }

    }
}