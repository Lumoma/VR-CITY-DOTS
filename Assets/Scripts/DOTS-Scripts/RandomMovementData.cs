/**
 * @file RandomMovementData.cs
 * @brief Datenstruktur für zufällige Bewegungen (DOTS).
 *
 * Enthält die Datenkomponenten für zufällige Bewegungen von DOTS-Entitäten.
 */
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    /// <summary>
    /// DOTS-Komponente für zufällige Bewegungen von Entitäten.
    /// </summary>
    public struct RandomMovementData : IComponentData
    {
        /// <summary>
        /// Bewegungsgeschwindigkeit der Entität.
        /// </summary>
        public float MovementSpeed;
        /// <summary>
        /// Rückstoß nach Kollision.
        /// </summary>
        public float BounceNudge;
        /// <summary>
        /// Cooldown-Zeit nach Kollision.
        /// </summary>
        public float CooldownDuration;
        /// <summary>
        /// Zentrum des Bewegungsbereichs.
        /// </summary>
        public float3 AreaCenter;
        /// <summary>
        /// Größe des Bewegungsbereichs.
        /// </summary>
        public float3 AreaSize;
        /// <summary>
        /// Zeitpunkt der letzten Kollision.
        /// </summary>
        public double LastCollisionTime;
        /// <summary>
        /// Zufallsgenerator für Bewegungsentscheidungen.
        /// </summary>
        public Unity.Mathematics.Random RandomGenerator;
    }
}