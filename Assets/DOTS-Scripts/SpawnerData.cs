using Unity.Entities;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    // Die Konfiguration (bleibt wie vorher)
    public struct SpawnerData : IComponentData
    {
        public Entity PrefabEntity;
        public int Count;
        public float3 AreaSize;
        public float3 SpawnCenter;
    }

    // Ein "Tag" (leeres Struct), mit dem wir alle gespawnten Agenten markieren
    // Damit wissen wir: "Diese Entity gehört zum Spawner und muss gelöscht werden"
    public struct SpawnedAgentTag : IComponentData { }

    // Der Befehl: Wenn diese Komponente auf dem Spawner liegt, arbeitet das System
    public struct RespawnRequest : IComponentData { }
}