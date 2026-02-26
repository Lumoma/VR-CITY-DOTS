/**
 * @file SpawnerUIBridge.cs
 * @brief Bindeglied zwischen Spawner-System und UI (DOTS).
 *
 * Dieses Script verbindet das DOTS-Spawner-System mit der Benutzeroberfläche.
 */
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace DOTS_Scripts
{
    /// <summary>
    /// Brücke zwischen UI (z.B. Slider) und DOTS-Spawner-System.
    /// </summary>
    public class SpawnerUIBridge : MonoBehaviour
    {
        private EntityManager _entityManager;
        private EntityQuery _spawnerQuery;

        /// <summary>
        /// Initialisiert EntityManager und EntityQuery für die UI-Interaktion.
        /// </summary>
        void Start()
        {
            // Verbindung zur DOTS-Welt herstellen
            var world = World.DefaultGameObjectInjectionWorld;
            _entityManager = world.EntityManager;
            
            // Wir bereiten eine Suche nach der Spawner-Entity vor
            _spawnerQuery = _entityManager.CreateEntityQuery(typeof(SpawnerData));
        }

        /// <summary>
        /// Setzt die Anzahl der zu spawnenden Entitäten über einen UI-Slider.
        /// </summary>
        /// <param name="value">Neuer Wert des Sliders</param>
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