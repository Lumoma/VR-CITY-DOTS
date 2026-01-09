using UnityEngine;

namespace OOP_Scripts
{
    // Stellt sicher, dass automatisch ein Rigidbody hinzugefügt wird
    [RequireComponent(typeof(Rigidbody))]
    public class RandomMovement : MonoBehaviour
    {
        [Header("Bewegungseinstellungen")]
        [SerializeField, Tooltip("Geschwindigkeit der Vorwärtsbewegung.")]
        private float movementSpeed = 1f;

        [SerializeField, Tooltip("Distanz, um die der Walker bei einer Kollision zurückgesetzt wird.")]
        private float bounceNudgeDistance = 0.05f;

        [SerializeField, Tooltip("Wartezeit in Sekunden zwischen zwei Kollisionsreaktionen.")]
        private float collisionCooldownDuration = 0.15f;

        [Header("Gebietsgrenzen")]
        [SerializeField] private Vector3 areaCenter;
        [SerializeField] private Vector3 areaSize;

        private readonly float[] turnAngles = { -90f, 90f, 180f };

        private Rigidbody _rigidBody;
        private float _lastCollisionTime = -10f;

        // Muss public sein, damit der Spawner (oder andere Skripte) es nutzen können
        public enum ColliderType { Box, Sphere }

        private void Awake()
        {
            InitializeRigidbody();
        }

        private void FixedUpdate()
        {
            MoveForward();
            StabilizePhysics();
            CheckAndHandleBounds();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsOnCooldown()) return;

            HandleCollisionReaction();
        }

        // --- Public API (Setup) ---

        /// <summary>
        /// Erlaubt externen Skripten (z.B. Spawner), die Bewegungsdaten zu setzen.
        /// </summary>
        public void InitializeMovementSettings(float speed, Vector3 center, Vector3 size)
        {
            movementSpeed = speed;
            areaCenter = center;
            areaSize = size;
        }

        /// <summary>
        /// Konfiguriert Mesh, Material und Collider zur Laufzeit.
        /// </summary>
        public void ApplyVisual(Mesh mesh, Material material, float scale, ColliderType colliderType)
        {
            UpdateMeshRenderer(mesh, material);
            transform.localScale = Vector3.one * scale;
            UpdateCollider(mesh, colliderType);
        }

        // --- Interne Logik ---

        private void InitializeRigidbody()
        {
            _rigidBody = GetComponent<Rigidbody>();
            
            _rigidBody.useGravity = false;
            _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidBody.sleepThreshold = 0f; 

            // WICHTIG: Hier werden die Constraints zentral gesetzt. Der Spawner muss das nicht tun.
            _rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void MoveForward()
        {
            // Ab Unity 6: linearVelocity. Für ältere Versionen: _rigidBody.velocity
            _rigidBody.linearVelocity = transform.forward * movementSpeed;
        }

        private void StabilizePhysics()
        {
            _rigidBody.angularVelocity = Vector3.zero;
        }

        private bool IsOnCooldown()
        {
            return Time.time - _lastCollisionTime < collisionCooldownDuration;
        }

        private void CheckAndHandleBounds()
        {
            // Wenn areaSize 0 ist (nicht initialisiert), keine Bounds prüfen
            if (areaSize == Vector3.zero) return;

            if (IsOutOfBounds() && !IsOnCooldown())
            {
                PerformTurnAround();
            }
        }

        private bool IsOutOfBounds()
        {
            Vector3 pos = transform.position;
            Vector3 min = areaCenter - areaSize * 0.5f;
            Vector3 max = areaCenter + areaSize * 0.5f;

            bool outsideX = pos.x < min.x || pos.x > max.x;
            bool outsideY = pos.y < min.y || pos.y > max.y;
            bool outsideZ = pos.z < min.z || pos.z > max.z;

            return outsideX || outsideY || outsideZ;
        }

        private void PerformTurnAround()
        {
            _lastCollisionTime = Time.time;
            RotateCharacter(180f);
            ApplyNudge();
        }

        private void HandleCollisionReaction()
        {
            _lastCollisionTime = Time.time;
            float randomAngle = turnAngles[Random.Range(0, turnAngles.Length)];
            RotateCharacter(randomAngle);
            MoveForward();
            ApplyNudge();
        }

        private void RotateCharacter(float angle)
        {
            transform.Rotate(0f, angle, 0f, Space.Self);
        }

        private void ApplyNudge()
        {
            transform.position += transform.forward * bounceNudgeDistance;
        }

        private void UpdateMeshRenderer(Mesh mesh, Material material)
        {
            if (!TryGetComponent(out MeshFilter mf)) mf = gameObject.AddComponent<MeshFilter>();
            if (!TryGetComponent(out MeshRenderer mr)) mr = gameObject.AddComponent<MeshRenderer>();

            mf.mesh = mesh;
            mr.sharedMaterial = material;
        }

        private void UpdateCollider(Mesh mesh, ColliderType colliderType)
        {
            RemoveExistingColliders();

            Bounds bounds = mesh != null ? mesh.bounds : new Bounds(Vector3.zero, Vector3.one);

            Vector3 scaledCenter = Vector3.Scale(bounds.center, transform.localScale);
            Vector3 scaledSize = Vector3.Scale(bounds.size, transform.localScale);
            Vector3 scaledExtents = Vector3.Scale(bounds.extents, transform.localScale);

            if (colliderType == ColliderType.Box)
            {
                var boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.center = scaledCenter;
                boxCollider.size = scaledSize;
            }
            else
            {
                var sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.center = scaledCenter;
                sphereCollider.radius = Mathf.Max(scaledExtents.x, scaledExtents.y, scaledExtents.z);
            }
        }

        private void RemoveExistingColliders()
        {
            foreach (var collider in GetComponents<Collider>())
            {
                Destroy(collider);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(areaCenter, areaSize);
        }
    }
}