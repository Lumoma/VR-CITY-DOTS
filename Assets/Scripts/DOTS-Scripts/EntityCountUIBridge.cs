using UnityEngine;
using Unity.Entities;
using TMPro; // Wichtig für TextMeshPro

namespace DOTS_Scripts
{
    public class EntityCountUIBridge : MonoBehaviour
    {
        [Tooltip("Zieh hier dein TextMeshPro Textfeld rein")]
        public TMP_Text textDisplay;

        private EntityManager _entityManager;
        private EntityQuery _agentQuery;

        void Start()
        {
            // Verbindung zur ECS Welt
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            // Wir definieren eine Suche nach allen Entities, die den "SpawnedAgentTag" haben
            _agentQuery = _entityManager.CreateEntityQuery(typeof(SpawnedAgentTag));
        }

        void Update()
        {
            if (textDisplay != null)
            {
                // Zählt blitzschnell alle aktiven Agenten
                int count = _agentQuery.CalculateEntityCount();
                textDisplay.text = $"{count}";
            }
        }
    }
}