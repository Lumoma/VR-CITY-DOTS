/**
 * @file SpawnerAuthoring.cs
 * @brief Authoring-Komponente für das Spawnen von Entitäten (DOTS).
 *
 * Ermöglicht die Konfiguration von Spawnern für DOTS-Entitäten im Editor.
 */
using UnityEngine;
using Unity.Entities;

namespace DOTS_Scripts
{
    /// <summary>
    /// Authoring-Komponente zur Konfiguration von SpawnerData für DOTS-Entitäten.
    /// </summary>
    public class SpawnerAuthoring : MonoBehaviour
    {
        /// <summary>
        /// Prefab, das als DOTS-Entität gebacken werden soll.
        /// </summary>
        [Header("DOTS Settings")]
        public GameObject prefabToBake;
        /// <summary>
        /// Anzahl der zu spawnenden Entitäten.
        /// </summary>
        public int spawnCount = 1000;
        /// <summary>
        /// Größe des Spawnbereichs.
        /// </summary>
        public Vector3 spawnAreaSize = new Vector3(20, 0, 20);

        // Wir merken uns die Entity, die dieser Spawner repräsentiert
        private Entity _spawnerEntity;
        private EntityManager _entityManager;

        /// <summary>
        /// Baker konvertiert Inspector-Daten in DOTS-Komponenten.
        /// </summary>
        class SpawnerBaker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SpawnerData
                {
                    PrefabEntity = GetEntity(authoring.prefabToBake, TransformUsageFlags.Dynamic),
                    Count = authoring.spawnCount,
                    AreaSize = authoring.spawnAreaSize,
                    SpawnCenter = authoring.transform.position
                });

                // Fügt direkt zu Beginn den Request hinzu, damit einmalig beim Start gespawnt wird
                AddComponent<RespawnRequest>(entity);
            }
        }

        /// <summary>
        /// Initialisiert EntityManager für spätere UI-Interaktion.
        /// </summary>
        private void Start()
        {
            // Wir holen uns den EntityManager, um später mit ECS zu sprechen
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        /// <summary>
        /// Setzt die Anzahl der zu spawnenden Entitäten über einen UI-Slider.
        /// </summary>
        /// <param name="sliderValue">Neuer Wert des Sliders</param>
        public void SetCountFromSlider(float sliderValue)
        {
            int newCount = Mathf.RoundToInt(sliderValue);

            if (spawnCount != newCount)
            {
                spawnCount = newCount;
                TriggerRespawn();
            }
        }

        /// <summary>
        /// Löst das Respawn der Entitäten in der DOTS-Welt aus.
        /// </summary>
        private void TriggerRespawn()
        {
            // 1. Finde Spawner-Entity in der ECS-Welt
            if (_spawnerEntity == Entity.Null)
            {
                var query = _entityManager.CreateEntityQuery(typeof(SpawnerData));
                if (query.CalculateEntityCount() > 0)
                {
                    // Wir nehmen einfach den ersten Spawner (Singleton-Prinzip)
                    _spawnerEntity = query.GetSingletonEntity();
                }
                else return; // Noch nicht initialisiert
            }

            // 2. Aktualisieren der Daten (Count) auf der Entity
            var data = _entityManager.GetComponentData<SpawnerData>(_spawnerEntity);
            data.Count = spawnCount;
            _entityManager.SetComponentData(_spawnerEntity, data);

            // 3. Hinzufügen der "RespawnRequest"-Komponente
            // Das System sieht das im nächsten Frame und reagiert (aktualisiert die Anzahl).
            if (!_entityManager.HasComponent<RespawnRequest>(_spawnerEntity))
            {
                _entityManager.AddComponent<RespawnRequest>(_spawnerEntity);
            }
        }
    }
}