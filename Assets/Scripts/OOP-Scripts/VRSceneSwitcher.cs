namespace OOP_Scripts
{
    /**
     * @file VRSceneSwitcher.cs
     * @brief Ermöglicht das Wechseln zwischen verschiedenen Szenen im VR-Modus.
     *
     * Dieses Script erlaubt das Umschalten zwischen Szenen in einer VR-Umgebung.
     */
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Ermöglicht das Wechseln zwischen Szenen im VR-Modus durch Tastendruck auf dem Controller.
    /// </summary>
    public class VRSceneSwitcher : MonoBehaviour
    {
        /// <summary>
        /// Der Name der Szene, die geladen werden soll.
        /// </summary>
        [Header("Einstellungen")]
        [Tooltip("Der Name der Szene, die geladen werden soll.")]
        public string targetSceneName;

        /// <summary>
        /// Welcher Controller überwacht werden soll (z.B. rechte Hand).
        /// </summary>
        [Tooltip("Welcher Controller soll überwacht werden?")]
        public XRNode controllerNode = XRNode.RightHand;

        private InputDevice _targetDevice;

        /// <summary>
        /// Überprüft in jedem Frame, ob die relevanten Tasten gedrückt wurden und wechselt ggf. die Szene.
        /// </summary>
        private void Update()
        {
            if (!_targetDevice.isValid)
            {
                InitializeDevice();
            }
            if (_targetDevice.isValid)
            {
                bool isAPressed = false;
                bool isBPressed = false;
                if (_targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryValue) && primaryValue)
                {
                    isAPressed = true;
                }
                if (_targetDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryValue) && secondaryValue)
                {
                    isBPressed = true;
                }
                if (isAPressed || isBPressed)
                {
                    SwitchScene();
                }
            }
        }

        /// <summary>
        /// Initialisiert das InputDevice für den gewünschten Controller.
        /// </summary>
        private void InitializeDevice()
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(controllerNode, devices);
            if (devices.Count > 0)
            {
                _targetDevice = devices[0];
            }
        }

        /// <summary>
        /// Wechselt zur angegebenen Szene, sofern ein Name eingetragen ist.
        /// </summary>
        public void SwitchScene()
        {
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
}
