using System;
using UnityEngine;
using TMPro;
using UnityEngine.XR;

public class FPSCounterAuto : MonoBehaviour
{
    private TextMeshProUGUI fpsText;
    private float deltaTime = 0.0f;
    private Canvas canvas;
    private Camera xrCamera;

    void Awake()
    {
        // XR Camera finden (typischerweise in XR Origin -> CameraOffset -> Main Camera)
        xrCamera = FindXRCamera();

        // Falls keine XR Camera gefunden, Standard Camera verwenden
        if (xrCamera == null)
        {
            xrCamera = Camera.main;
        }

        // --- Canvas erstellen ---
        GameObject canvasObj = new GameObject("FPSCanvas");
        canvas = canvasObj.AddComponent<Canvas>();

        // Für XR: WorldSpace verwenden statt ScreenSpaceOverlay
        if (XRSettings.enabled)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = xrCamera;

            // Canvas als Child der Camera positionieren
            if (xrCamera != null)
            {
                canvasObj.transform.SetParent(xrCamera.transform, false);
                // Position vor der Kamera (z.B. 1 Meter voraus, leicht oben)
                canvasObj.transform.localPosition = new Vector3(0, 0.2f, 1f);
                canvasObj.transform.localRotation = Quaternion.identity;

                // Skalierung für WorldSpace (größer als 0.001, sonst ist Text praktisch unsichtbar)
                canvasObj.transform.localScale = Vector3.one * 0.01f;

                // Canvas oben halten
                canvas.overrideSorting = true;
                canvas.sortingOrder = 1000;
            }
        }
        else
        {
            // Fallback für Non-XR: ScreenSpaceOverlay
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        canvas.pixelPerfect = true;

        // EventSystem hinzufügen, falls noch keins existiert
        if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // --- TextMeshPro-Objekt erstellen ---
        GameObject textObj = new GameObject("FPSText");
        textObj.transform.SetParent(canvasObj.transform, false);

        fpsText = textObj.AddComponent<TextMeshProUGUI>();
        // Größere FontSize im WorldSpace, kleinere für ScreenSpace
        fpsText.fontSize = XRSettings.enabled ? 36f : 24f;
        fpsText.alignment = TextAlignmentOptions.TopLeft;
        fpsText.color = Color.white;
        fpsText.text = "FPS: 0";

        // --- Position & Layout einstellen ---
        RectTransform rect = fpsText.GetComponent<RectTransform>();
        if (XRSettings.enabled)
        {
            // Für WorldSpace: Zentriert positionieren, in Metern (kleinere Werte)
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0.2f, 0.05f); // in Metern beim WorldSpace-Canvas mit scale 0.01
        }
        else
        {
            // Für ScreenSpaceOverlay: Oben links
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
            rect.sizeDelta = new Vector2(200, 50);
        }
    }

    private Camera FindXRCamera()
    {
        // Versuche per Reflection, XROrigin zu finden, falls das Paket installiert ist.
        var xrOriginType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XROrigin, Unity.XR.Interaction.Toolkit");
        if (xrOriginType != null)
        {
            // Benutze Resources.FindObjectsOfTypeAll statt der veralteten FindObjectOfType-APIs
            var objs = Resources.FindObjectsOfTypeAll(xrOriginType);
            if (objs != null && objs.Length > 0)
            {
                var xrOriginObj = objs[0];
                if (xrOriginObj != null)
                {
                    // Versuche Property "Camera"
                    var camProp = xrOriginType.GetProperty("Camera");
                    if (camProp != null)
                    {
                        var camVal = camProp.GetValue(xrOriginObj) as Camera;
                        if (camVal != null) return camVal;
                    }

                    // Falls Property nicht vorhanden: Field "Camera" prüfen
                    var camField = xrOriginType.GetField("Camera");
                    if (camField != null)
                    {
                        var camVal = camField.GetValue(xrOriginObj) as Camera;
                        if (camVal != null) return camVal;
                    }

                    // Manche Versionen bieten eine Kamera als GameObject-Feld an
                    var camGoProp = xrOriginType.GetProperty("CameraFloorOffsetObject") ?? xrOriginType.GetProperty("CameraGameObject");
                    if (camGoProp != null)
                    {
                        var go = camGoProp.GetValue(xrOriginObj) as GameObject;
                        if (go != null)
                        {
                            var cam = go.GetComponent<Camera>();
                            if (cam != null) return cam;
                        }
                    }
                }
            }
        }

        // Fallback: Main Camera oder irgendeine Kamera in der Scene
        if (Camera.main != null) return Camera.main;

        Camera[] cams = FindObjectsOfType<Camera>();
        if (cams != null && cams.Length > 0) return cams[0];

        return null;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        if (fpsText != null)
        {
            fpsText.text = $"{fps:0.} FPS";
        }
    }
}