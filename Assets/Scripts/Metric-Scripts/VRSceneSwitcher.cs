using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class VRSceneSwitcher : MonoBehaviour
{
    [Header("Einstellungen")]
    [Tooltip("Der Name der Szene, die geladen werden soll.")]
    public string targetSceneName;

    [Tooltip("Welcher Controller soll überwacht werden?")]
    public XRNode controllerNode = XRNode.RightHand; // Standardmäßig Rechte Hand (wo meist A/B sind)

    // Interne Referenz zum Input-Gerät
    private InputDevice _targetDevice;

    void Update()
    {
        // 1. Gerät abrufen, falls noch nicht vorhanden oder verloren gegangen
        if (!_targetDevice.isValid)
        {
            InitializeDevice();
        }

        // 2. Tasten überprüfen
        // In Unity XR ist "PrimaryButton" meistens die A-Taste (oder X links)
        // und "SecondaryButton" meistens die B-Taste (oder Y links).
        
        if (_targetDevice.isValid)
        {
            bool isAPressed = false;
            bool isBPressed = false;

            // Prüfe auf A-Taste (Primary)
            if (_targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryValue) && primaryValue)
            {
                isAPressed = true;
            }

            // Prüfe auf B-Taste (Secondary)
            if (_targetDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryValue) && secondaryValue)
            {
                isBPressed = true;
            }

            // Wenn eine der beiden gedrückt wurde -> Szene wechseln
            if (isAPressed || isBPressed)
            {
                SwitchScene();
            }
        }
    }

    private void InitializeDevice()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(controllerNode, devices);

        if (devices.Count > 0)
        {
            _targetDevice = devices[0];
        }
    }

    public void SwitchScene()
    {
        // Sicherheitscheck, ob ein Szenenname eingetragen wurde
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"Wechsle zu Szene: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("Kein Szenenname im Inspector eingetragen!");
        }
    }
}
