using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    [BurstCompile]
    public partial struct SpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Wir führen das System nur aus, wenn es einen aktiven RespawnRequest gibt
            state.RequireForUpdate<RespawnRequest>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 1. Spawner Entity holen
            var spawnerEntity = SystemAPI.GetSingletonEntity<SpawnerData>();
            var config = SystemAPI.GetComponent<SpawnerData>(spawnerEntity);

            // 2. Alte Agenten löschen
            // Wir suchen alle Entities, die den "SpawnedAgentTag" haben
            var oldAgentsQuery = SystemAPI.QueryBuilder().WithAll<SpawnedAgentTag>().Build();
            state.EntityManager.DestroyEntity(oldAgentsQuery);

            // 3. Neue Agenten erstellen
            var instances = state.EntityManager.Instantiate(config.PrefabEntity, config.Count, Allocator.Temp);
            var random = new Unity.Mathematics.Random((uint)SystemAPI.Time.ElapsedTime + 1);

            for (int i = 0; i < instances.Length; i++)
            {
                var entity = instances[i];

                // Zufallsposition
                float3 randomPos = config.SpawnCenter + new float3(
                    random.NextFloat(-config.AreaSize.x / 2, config.AreaSize.x / 2),
                    0,
                    random.NextFloat(-config.AreaSize.z / 2, config.AreaSize.z / 2)
                );
                
                quaternion randomRot = quaternion.RotateY(random.NextFloat(0, math.PI * 2));

                // Transform setzen
                var transform = LocalTransform.FromPositionRotation(randomPos, randomRot);
                state.EntityManager.SetComponentData(entity, transform);

                // RandomWalker Daten setzen
                if (state.EntityManager.HasComponent<RandomWalkerData>(entity))
                {
                    var walkerData = state.EntityManager.GetComponentData<RandomWalkerData>(entity);
                    walkerData.AreaSize = config.AreaSize;
                    walkerData.AreaCenter = config.SpawnCenter;
                    walkerData.RandomGenerator = new Unity.Mathematics.Random(random.NextUInt());
                    state.EntityManager.SetComponentData(entity, walkerData);
                }

                // WICHTIG: Den Tag hinzufügen, damit wir sie beim nächsten Mal löschen können
                state.EntityManager.AddComponent<SpawnedAgentTag>(entity);
            }

            // 4. Request entfernen
            // Damit das System im nächsten Frame nicht wieder läuft, entfernen wir den Befehl
            state.EntityManager.RemoveComponent<RespawnRequest>(spawnerEntity);
        }
    }
}