/**
 * @file RandomMovementAuthoring.cs
 * @brief Authoring-Komponente für zufällige Bewegungen (DOTS).
 *
 * Dieses Script ermöglicht die Konfiguration von zufälligen Bewegungen für DOTS-Entitäten im Editor.
 */
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    /// <summary>
    /// Authoring-Komponente zur Konfiguration von RandomMovementData für DOTS-Entitäten.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RandomMovementAuthoring : MonoBehaviour
    {
        /// <summary>
        /// Bewegungsgeschwindigkeit der Entität.
        /// </summary>
        [Header("Settings")]
        public float movementSpeed = 5f;
        /// <summary>
        /// Rückstoß nach Kollision.
        /// </summary>
        public float bounceNudge = 0.5f;
        /// <summary>
        /// Cooldown-Zeit nach Kollision.
        /// </summary>
        public float cooldown = 0.15f;
        /// <summary>
        /// Zentrum des Bewegungsbereichs.
        /// </summary>
        [Header("Area")]
        public Vector3 areaCenter;
        /// <summary>
        /// Größe des Bewegungsbereichs.
        /// </summary>
        public Vector3 areaSize = new Vector3(20, 2, 20);

        /// <summary>
        /// Baker konvertiert Inspector-Daten in DOTS-Komponenten.
        /// </summary>
        class Baker : Baker<RandomMovementAuthoring>
        {
            public override void Bake(RandomMovementAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RandomMovementData
                {
                    MovementSpeed = authoring.movementSpeed,
                    BounceNudge = authoring.bounceNudge,
                    CooldownDuration = authoring.cooldown,
                    AreaCenter = authoring.areaCenter,
                    AreaSize = authoring.areaSize,
                    LastCollisionTime = 0,
                    RandomGenerator = new Unity.Mathematics.Random((uint)entity.Index + 1)
                });
            }
        }

        /// <summary>
        /// Zeichnet den Bewegungsbereich im Editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(areaCenter, areaSize);
        }
    }
}