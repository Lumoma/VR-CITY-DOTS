/**
 * @file SpawnerData.cs
 * @brief Datenstruktur für das Spawnen von Entitäten (DOTS).
 *
 * Enthält die Datenkomponenten für das Spawnen von DOTS-Entitäten.
 */
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    /// <summary>
    /// DOTS-Komponente mit Konfigurationsdaten für das Spawnen von Entitäten.
    /// </summary>
    public struct SpawnerData : IComponentData
    {
        /// <summary>
        /// Prefab-Entity, die instanziiert werden soll.
        /// </summary>
        public Entity PrefabEntity;
        /// <summary>
        /// Anzahl der zu spawnenden Entitäten.
        /// </summary>
        public int Count;
        /// <summary>
        /// Größe des Spawnbereichs.
        /// </summary>
        public float3 AreaSize;
        /// <summary>
        /// Zentrum des Spawnbereichs.
        /// </summary>
        public float3 SpawnCenter;
    }

    /// <summary>
    /// Tag-Komponente zur Markierung gespawnter Agenten.
    /// </summary>
    public struct SpawnedAgentTag : IComponentData { }

    /// <summary>
    /// Tag-Komponente als Befehl für das Respawn-System.
    /// </summary>
    public struct RespawnRequest : IComponentData { }
}