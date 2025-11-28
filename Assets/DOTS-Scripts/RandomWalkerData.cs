using Unity.Entities;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    // IComponentData ist das Interface für ECS-Daten
    public struct RandomWalkerData : IComponentData
    {
        public float MovementSpeed;
        public float BounceNudge;
        public float CooldownDuration;
        
        // DOTS nutzt float3 statt Vector3
        public float3 AreaCenter;
        public float3 AreaSize;
        
        // Status-Werte
        public double LastCollisionTime;
        public Unity.Mathematics.Random RandomGenerator; // Für Zufall im Job-System
    }
}