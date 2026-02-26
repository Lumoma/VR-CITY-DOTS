namespace OOP_Scripts
{
    /**
     * @file VRFPSCounter.cs
     * @brief Misst und zeigt die aktuelle Framerate (FPS) im VR-Modus an.
     *
     * Dieses Script dient zur Überwachung der Framerate in VR-Anwendungen.
     */
    using UnityEngine;
    using TMPro;

    /// <summary>
    /// Zeigt die aktuelle Framerate (FPS) im VR-Modus an und färbt den Text je nach Performance.
    /// </summary>
    public class VRFPSCounter : MonoBehaviour
    {
        /// <summary>
        /// TextMeshPro-Objekt zur Anzeige der FPS.
        /// </summary>
        [Header("Einstellungen")]
        [Tooltip("Ziehe hier dein TextMeshPro Objekt rein")]
        [SerializeField] private TMP_Text fpsText;
        
        /// <summary>
        /// Wie oft die Anzeige aktualisiert wird (in Sekunden).
        /// </summary>
        [Tooltip("Wie oft soll die Anzeige aktualisiert werden? (in Sekunden)")]
        [SerializeField] private float updateInterval = 0.5f;

        private float _timer;
        private int _frameCount;

        /// <summary>
        /// Zählt die Frames und aktualisiert die Anzeige in festen Intervallen.
        /// </summary>
        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            _frameCount++;

            if (_timer >= updateInterval)
            {
                float fps = _frameCount / _timer;
                fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
                fpsText.color = fps < 72 ? Color.red : Color.green;
                _timer = 0.0f;
                _frameCount = 0;
            }
        }
    }
}