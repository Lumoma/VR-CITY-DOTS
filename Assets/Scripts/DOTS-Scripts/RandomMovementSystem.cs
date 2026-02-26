/**
 * @file RandomMovementSystem.cs
 * @brief System zur Steuerung der zufälligen Bewegungen (DOTS).
 *
 * Dieses System steuert die Bewegung von DOTS-Entitäten basierend auf RandomMovementData.
 */
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    /// <summary>
    /// DOTS-System zur Steuerung der zufälligen Bewegungen von Entitäten.
    /// </summary>
    [BurstCompile]
    public partial struct RandomMovementSystem : ISystem
    {
        /// <summary>
        /// Führt das System-Update aus und plant den RandomWalkerJob.
        /// </summary>
        /// <param name="state">SystemState</param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new RandomWalkerJob
            {
                CurrentTime = SystemAPI.Time.ElapsedTime,
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }

    /// <summary>
    /// Job zur Steuerung der Bewegung und Bounds-Prüfung für jede Entität.
    /// </summary>
    [BurstCompile]
    public partial struct RandomWalkerJob : IJobEntity
    {
        /// <summary>
        /// Aktuelle Zeit (für Cooldown).
        /// </summary>
        public double CurrentTime;
        /// <summary>
        /// DeltaTime für die Bewegung.
        /// </summary>
        public float DeltaTime;

        /// <summary>
        /// Führt die Bewegungs- und Bounds-Logik für eine Entität aus.
        /// </summary>
        private void Execute(ref RandomMovementData data, ref LocalTransform transform, ref PhysicsVelocity velocity)
        {
            // --- 1. Rotation korrigieren (Der Zombie-Fix) ---
            // Wir zwingen die Entity, absolut aufrecht zu stehen.
            // Wir nehmen die aktuelle Rotation, aber löschen jegliches Kippen (X/Z).
            float3 currentForward = math.forward(transform.Rotation);
            
            // Wir berechnen eine neue Rotation, die nur um die Y-Achse (oben) schaut
            // math.atan2 gibt uns den Winkel auf dem Boden
            float currentYAngle = math.atan2(currentForward.x, currentForward.z);
            transform.Rotation = quaternion.RotateY(currentYAngle);

            // --- 2. Bewegung ---
            float3 forward = math.forward(transform.Rotation);
            float currentVerticalVelocity = velocity.Linear.y;

            float3 newVelocity = forward * data.MovementSpeed;
            newVelocity.y = currentVerticalVelocity; // Schwerkraft beibehalten

            velocity.Linear = newVelocity;
            
            // WICHTIG: Drehimpuls komplett töten, damit sie nicht trudeln
            velocity.Angular = float3.zero; 

            // --- 3. Bounds Check ---
            if (IsOutOfBounds(transform.Position, data.AreaCenter, data.AreaSize))
            {
                HandleOutOfBounds(ref data, ref transform, forward);
            }
        }

        /// <summary>
        /// Reagiert auf das Verlassen des Bereichs mit Drehung und Nudge.
        /// </summary>
        private void HandleOutOfBounds(ref RandomMovementData data, ref LocalTransform transform, float3 forward)
        {
            if (CurrentTime - data.LastCollisionTime < data.CooldownDuration) return;

            data.LastCollisionTime = CurrentTime;

            // Drehung um 180 Grad
            float angle = math.radians(180f);
            transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(angle));
            transform.Position += forward * -data.BounceNudge;
        }

        /// <summary>
        /// Prüft, ob die Entität außerhalb des erlaubten Bereichs ist.
        /// </summary>
        private bool IsOutOfBounds(float3 pos, float3 center, float3 size)
        {
            float3 min = center - size * 0.5f;
            float3 max = center + size * 0.5f;
            return pos.x < min.x || pos.x > max.x || pos.z < min.z || pos.z > max.z;
        }
    }
}