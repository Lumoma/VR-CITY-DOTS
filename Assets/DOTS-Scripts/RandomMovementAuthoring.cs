using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS_Scripts
{      
    // WICHTIG: Diese Zeile wieder reinnehmen!
    // Sie garantiert, dass dein Prefab einen Rigidbody hat, den Unity "baken" kann.
    [RequireComponent(typeof(Rigidbody))]
    public class RandomMovementAuthoring : MonoBehaviour
    {
        [Header("Settings")]
        public float movementSpeed = 5f;
        public float bounceNudge = 0.5f;
        public float cooldown = 0.15f;
        
        [Header("Area")]
        public Vector3 areaCenter;
        public Vector3 areaSize = new Vector3(20, 2, 20);

        // Der Baker konvertiert die Inspector-Daten in ECS-Daten
        class Baker : Baker<RandomMovementAuthoring>
        {
            public override void Bake(RandomMovementAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Initialisierung der Component Data
                AddComponent(entity, new RandomMovementData
                {
                    MovementSpeed = authoring.movementSpeed,
                    BounceNudge = authoring.bounceNudge,
                    CooldownDuration = authoring.cooldown,
                    AreaCenter = authoring.areaCenter, // Autom. Cast Vector3 -> float3
                    AreaSize = authoring.areaSize,
                    LastCollisionTime = 0,
                    // Wichtig: Zufallsgenerator mit Seed initialisieren
                    RandomGenerator = new Unity.Mathematics.Random((uint)entity.Index + 1)
                });
            }
        }
        
        // Gizmos funktionieren weiterhin im Authoring Script
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(areaCenter, areaSize);
        }
    }
}