using UnityEngine;
using TMPro; // Wichtig: Namespace für TextMeshPro

public class VRFPSCounter : MonoBehaviour
{
    [Header("Einstellungen")]
    [Tooltip("Ziehe hier dein TextMeshPro Objekt rein")]
    [SerializeField] private TMP_Text fpsText;
    
    [Tooltip("Wie oft soll die Anzeige aktualisiert werden? (in Sekunden)")]
    [SerializeField] private float updateInterval = 0.5f;

    private float timer = 0.0f;
    private int frameCount = 0;

    void Update()
    {
        // Addiere die vergangene Zeit (unabhängig von TimeScale)
        timer += Time.unscaledDeltaTime;
        frameCount++;

        // Wenn das Intervall erreicht ist, aktualisiere den Text
        if (timer >= updateInterval)
        {
            // Berechnung der FPS: Anzahl Frames / vergangene Zeit
            float fps = frameCount / timer;

            // Text aktualisieren (Mathf.Ceil rundet zur nächsten Ganzzahl auf)
            fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
            
            // Optional: Farbe ändern, wenn FPS niedrig sind (für VR wichtig)
            if (fps < 72) // 72Hz ist oft das Minimum für Quest/VR
                fpsText.color = Color.red;
            else
                fpsText.color = Color.green;

            // Timer und Counter zurücksetzen
            timer = 0.0f;
            frameCount = 0;
        }
    }
}