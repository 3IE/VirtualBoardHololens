using TMPro;
using UnityEngine;

namespace Script.UI
{
    public class FPSCounter : MonoBehaviour
    {
        private float           avgFramerate;
        private TextMeshProUGUI m_Text;

        private void Start()
        {
            m_Text = GetComponent<TextMeshProUGUI>();
            InvokeRepeating(nameof(FPS), 1, 0.1f);
        }

        private void FPS()
        {
            //smoothDeltaTime, deltaTime, fixedDeltaTime
            float timelapse = Time.unscaledDeltaTime;
            avgFramerate = (int) (1f / timelapse);
            m_Text.text  = $"{avgFramerate} FPS";
        }
    }
}
