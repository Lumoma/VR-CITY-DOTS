using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    public class DOTSSpawner : MonoBehaviour
    {
        [Header("DOTS Settings")]
        public GameObject prefabToBake;
        public int spawnCount = 1000;
        public Vector3 spawnAreaSize = new Vector3(20, 0, 20);

        // Wir merken uns die Entity, die dieser Spawner repräsentiert
        private Entity _spawnerEntity;
        private EntityManager _entityManager;

        // Baker: Wandelt das GameObject beim Start in eine Entity um
        class SpawnerBaker : Baker<DOTSSpawner>
        {
            public override void Bake(DOTSSpawner authoring)
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
            // 1. Wir müssen die Spawner-Entity in der ECS-Welt finden
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

            // 2. Wir aktualisieren die Daten (Count) auf der Entity
            var data = _entityManager.GetComponentData<SpawnerData>(_spawnerEntity);
            data.Count = spawnCount;
            _entityManager.SetComponentData(_spawnerEntity, data);

            // 3. WICHTIG: Wir fügen die "RespawnRequest"-Komponente hinzu
            // Das System sieht das im nächsten Frame und reagiert.
            if (!_entityManager.HasComponent<RespawnRequest>(_spawnerEntity))
            {
                _entityManager.AddComponent<RespawnRequest>(_spawnerEntity);
            }
        }
    }
}