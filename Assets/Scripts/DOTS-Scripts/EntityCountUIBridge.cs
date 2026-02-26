/**
 * @file EntityCountUIBridge.cs
 * @brief Bindeglied zwischen DOTS-Entitäten und UI zur Anzeige der Entitätsanzahl.
 *
 * Dieses Script verbindet DOTS-Entitäten mit der Benutzeroberfläche, um die aktuelle Anzahl anzuzeigen.
 */
using UnityEngine;
using Unity.Entities;
using TMPro;

namespace DOTS_Scripts
{
    /// <summary>
    /// Zeigt die aktuelle Anzahl der DOTS-Entitäten (z.B. Agenten) im UI an.
    /// </summary>
    public class EntityCountUIBridge : MonoBehaviour
    {
        /// <summary>
        /// TextMeshPro-Objekt zur Anzeige der Entitätsanzahl.
        /// </summary>
        [Tooltip("Zieh hier dein TextMeshPro Textfeld rein")]
        public TMP_Text textDisplay;

        private EntityManager _entityManager;
        private EntityQuery _agentQuery;

        /// <summary>
        /// Initialisiert EntityManager und EntityQuery für die Anzeige.
        /// </summary>
        void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _agentQuery = _entityManager.CreateEntityQuery(typeof(SpawnedAgentTag));
        }

        /// <summary>
        /// Aktualisiert die Anzeige der Entitätsanzahl in jedem Frame.
        /// </summary>
        void Update()
        {
            if (textDisplay != null)
            {
                int count = _agentQuery.CalculateEntityCount();
                textDisplay.text = $"{count}";
            }
        }
    }
}