using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace DOTS_Scripts
{
    // Dieses Script lebt in der "normalen" Welt (Main Scene)
    // Es leitet UI-Eingaben an die ECS-Welt weiter.
    public class SpawnerUIBridge : MonoBehaviour
    {
        private EntityManager _entityManager;
        private EntityQuery _spawnerQuery;

        void Start()
        {
            // Verbindung zur DOTS-Welt herstellen
            var world = World.DefaultGameObjectInjectionWorld;
            _entityManager = world.EntityManager;
            
            // Wir bereiten eine Suche nach der Spawner-Entity vor
            _spawnerQuery = _entityManager.CreateEntityQuery(typeof(SpawnerData));
        }

        // Verknüpfe diese Methode mit deinem Slider (Dynamic float)
        public void OnSliderValueChanged(float value)
        {
            // Sicherheitscheck: Gibt es die Spawner-Entity schon?
            if (_spawnerQuery.CalculateEntityCount() == 0) return;

            // Wir holen die Entity (es sollte nur eine geben -> Singleton)
            var spawnerEntity = _spawnerQuery.GetSingletonEntity();
            
            // 1. Daten holen
            var data = _entityManager.GetComponentData<SpawnerData>(spawnerEntity);
            
            // 2. Prüfen ob sich was geändert hat, um Spam zu vermeiden
            int newCount = Mathf.RoundToInt(value);
            if (data.Count == newCount) return;

            // 3. Wert ändern
            data.Count = newCount;
            _entityManager.SetComponentData(spawnerEntity, data);

            // 4. Den "RespawnRequest" Sticker draufkleben
            // Das System sieht diesen Sticker und führt den Respawn aus
            if (!_entityManager.HasComponent<RespawnRequest>(spawnerEntity))
            {
                _entityManager.AddComponent<RespawnRequest>(spawnerEntity);
            }
        }
    }
}