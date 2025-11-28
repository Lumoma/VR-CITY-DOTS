using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    // [BurstCompile] optimiert das System für High-Performance
    [BurstCompile]
    public partial struct RandomWalkerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Wir erstellen den Job und übergeben die aktuellen Zeit-Werte
            var job = new RandomWalkerJob
            {
                CurrentTime = SystemAPI.Time.ElapsedTime,
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            // ScheduleParallel verteilt die Arbeit auf alle verfügbaren CPU-Kerne
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }

    // Der Job enthält die eigentliche Logik für jeden Agenten
    [BurstCompile]
    public partial struct RandomWalkerJob : IJobEntity
    {
        public double CurrentTime;
        public float DeltaTime;

        // Execute wird automatisch für jede passende Entity aufgerufen
        private void Execute(ref RandomWalkerData data, ref LocalTransform transform, ref PhysicsVelocity velocity)
        {
            // --- 1. Bewegung (mit Schwerkraft-Fix) ---
            
            float3 forward = math.forward(transform.Rotation);
            
            // WICHTIG: Wir merken uns die aktuelle Fallgeschwindigkeit (Y-Achse)
            // Wenn wir das nicht tun, würden wir sie mit 0 überschreiben und die Kapsel würde schweben.
            float currentVerticalVelocity = velocity.Linear.y;

            // Wir berechnen die neue Geschwindigkeit nur für die Ebene (X und Z)
            float3 newVelocity = forward * data.MovementSpeed;
            
            // Wir setzen die alte Fallgeschwindigkeit wieder ein
            newVelocity.y = currentVerticalVelocity;

            // Zuweisen der kombinierten Geschwindigkeit
            velocity.Linear = newVelocity;
            
            // Stabilisierung: Verhindert wildes Rotieren bei Kollisionen
            velocity.Angular = float3.zero; 

            // --- 2. Bounds Check (Sind wir noch im Gebiet?) ---
            
            if (IsOutOfBounds(transform.Position, data.AreaCenter, data.AreaSize))
            {
                HandleOutOfBounds(ref data, ref transform, forward);
            }
        }

        // Logik für das Umdrehen, wenn man die Grenze erreicht
        private void HandleOutOfBounds(ref RandomWalkerData data, ref LocalTransform transform, float3 forward)
        {
            // Cooldown prüfen, damit man nicht sofort wieder dreht
            if (CurrentTime - data.LastCollisionTime < data.CooldownDuration) return;

            data.LastCollisionTime = CurrentTime;

            // Drehung um 180 Grad
            float angle = math.radians(180f);
            transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(angle));

            // Kleiner "Nudge" (Rückstoß), um nicht in der Wand zu kleben
            transform.Position += forward * -data.BounceNudge;
        }

        // Hilfsfunktion zur Prüfung der Grenzen
        private bool IsOutOfBounds(float3 pos, float3 center, float3 size)
        {
            float3 min = center - size * 0.5f;
            float3 max = center + size * 0.5f;

            // Wir prüfen nur X und Z, die Höhe (Y) ist egal
            return pos.x < min.x || pos.x > max.x || 
                   pos.z < min.z || pos.z > max.z;
        }
    }
}