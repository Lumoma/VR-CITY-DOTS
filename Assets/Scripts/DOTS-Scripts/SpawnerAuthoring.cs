using UnityEngine;
using Unity.Entities;

namespace DOTS_Scripts
{
    public class SpawnerAuthoring : MonoBehaviour
    {
        [Header("DOTS Settings")]
        public GameObject prefabToBake;
        public int spawnCount = 1000;
        public Vector3 spawnAreaSize = new Vector3(20, 0, 20);

        // Wir merken uns die Entity, die dieser Spawner repräsentiert
        private Entity _spawnerEntity;
        private EntityManager _entityManager;

        // Baker: Wandelt das GameObject beim Start in eine Entity um
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

        // --- UI Event Logic ---

        private void Start()
        {
            // Wir holen uns den EntityManager, um später mit ECS zu sprechen
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        // Diese Funktion verknüpfst du mit deinem UI Slider
        public void SetCountFromSlider(float sliderValue)
        {
            int newCount = Mathf.RoundToInt(sliderValue);

            if (spawnCount != newCount)
            {
                spawnCount = newCount;
                TriggerRespawn();
            }
        }

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