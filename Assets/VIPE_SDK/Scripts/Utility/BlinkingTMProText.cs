using UnityEngine;
using TMPro;

namespace VIPE_SDK
{
    public class BlinkingTMProText : MonoBehaviour
    {
        private TMP_Text textComponent;
        public float interval = 1f;
        private bool isFadingOut = true;

        private void Start()
        {
            textComponent = GetComponent<TMP_Text>();
            if (textComponent == null)
            {
                Debug.LogError("No TMP_Text component attached.");
                return;
            }
        }

        private void Update()
        {
            float alphaChangeSpeed = 1f / interval * Time.deltaTime;

            if (isFadingOut)
            {
                textComponent.alpha -= alphaChangeSpeed;
                if (textComponent.alpha <= 0)
                {
                    isFadingOut = false;
                }
            }
            else
            {
                textComponent.alpha += alphaChangeSpeed;
                if (textComponent.alpha >= 1)
                {
                    isFadingOut = true;
                }
            }
        }
    }
}
