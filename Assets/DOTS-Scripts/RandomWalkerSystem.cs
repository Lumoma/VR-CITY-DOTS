using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;

namespace DOTS_Scripts
{
    [BurstCompile]
    public partial struct RandomWalkerSystem : ISystem
    {
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

    [BurstCompile]
    public partial struct RandomWalkerJob : IJobEntity
    {
        public double CurrentTime;
        public float DeltaTime;

        private void Execute(ref RandomWalkerData data, ref LocalTransform transform, ref PhysicsVelocity velocity)
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

        private void HandleOutOfBounds(ref RandomWalkerData data, ref LocalTransform transform, float3 forward)
        {
            if (CurrentTime - data.LastCollisionTime < data.CooldownDuration) return;

            data.LastCollisionTime = CurrentTime;

            // Drehung um 180 Grad
            float angle = math.radians(180f);
            transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(angle));
            transform.Position += forward * -data.BounceNudge;
        }

        private bool IsOutOfBounds(float3 pos, float3 center, float3 size)
        {
            float3 min = center - size * 0.5f;
            float3 max = center + size * 0.5f;
            return pos.x < min.x || pos.x > max.x || pos.z < min.z || pos.z > max.z;
        }
    }
}