/**
 * @file RandomMovement.cs
 * @brief Implementiert zufällige Bewegungen für Agenten/Prefabs.
 *
 * Dieses Script steuert die zufällige Bewegung von Agenten in der Szene.
 */

using UnityEngine;

namespace OOP_Scripts
{
    /// <summary>
    /// Steuert die zufällige Bewegung eines Agenten innerhalb eines definierten Bereichs.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RandomMovement : MonoBehaviour
    {
        /// <summary>
        /// Geschwindigkeit der Vorwärtsbewegung.
        /// </summary>
        [Header("Bewegungseinstellungen")]
        [SerializeField, Tooltip("Geschwindigkeit der Vorwärtsbewegung.")]
        private float movementSpeed = 1f;

        /// <summary>
        /// Distanz, um die der Agent bei einer Kollision zurückgesetzt wird.
        /// </summary>
        [SerializeField, Tooltip("Distanz, um die der Walker bei einer Kollision zurückgesetzt wird.")]
        private float bounceNudgeDistance = 0.05f;

        /// <summary>
        /// Wartezeit in Sekunden zwischen zwei Kollisionsreaktionen.
        /// </summary>
        [SerializeField, Tooltip("Wartezeit in Sekunden zwischen zwei Kollisionsreaktionen.")]
        private float collisionCooldownDuration = 0.15f;

        /// <summary>
        /// Zentrum des Bewegungsbereichs.
        /// </summary>
        [Header("Gebietsgrenzen")]
        [SerializeField] private Vector3 areaCenter;
        /// <summary>
        /// Größe des Bewegungsbereichs.
        /// </summary>
        [SerializeField] private Vector3 areaSize;

        /// <summary>
        /// Mögliche Drehwinkel für Richtungsänderungen nach Kollision.
        /// </summary>
        private readonly float[] _turnAngles = { -90f, 90f, 180f };
        private Rigidbody _rigidBody;
        private float _lastCollisionTime = -10f;

        /// <summary>
        /// Typ des zu verwendenden Colliders.
        /// </summary>
        public enum ColliderType { Box, Sphere }

        /// <summary>
        /// Initialisiert den Rigidbody und setzt die Constraints.
        /// </summary>
        private void Awake()
        {
            InitializeRigidbody();
        }

        /// <summary>
        /// Führt die Bewegung und Stabilisierung in jedem FixedUpdate aus.
        /// </summary>
        private void FixedUpdate()
        {
            MoveForward();
            StabilizePhysics();
            CheckAndHandleBounds();
        }

        /// <summary>
        /// Reagiert auf Kollisionen mit einer Richtungsänderung.
        /// </summary>
        /// <param name="collision">Die Kollisionsdaten (nicht verwendet).</param>
        private void OnCollisionEnter(Collision collision)
        {
            if (IsOnCooldown()) return;
            HandleCollisionReaction();
        }

        /// <summary>
        /// Erlaubt externen Skripten (z.B. Spawner), die Bewegungsdaten zu setzen.
        /// </summary>
        /// <param name="speed">Bewegungsgeschwindigkeit</param>
        /// <param name="center">Zentrum des Bereichs</param>
        /// <param name="size">Größe des Bereichs</param>
        public void InitializeMovementSettings(float speed, Vector3 center, Vector3 size)
        {
            movementSpeed = speed;
            areaCenter = center;
            areaSize = size;
        }

        /// <summary>
        /// Konfiguriert Mesh, Material und Collider zur Laufzeit.
        /// </summary>
        /// <param name="mesh">Mesh für das Objekt</param>
        /// <param name="material">Material für das Objekt</param>
        /// <param name="scale">Skalierung</param>
        /// <param name="colliderType">Typ des Colliders</param>
        public void ApplyVisual(Mesh mesh, Material material, float scale, ColliderType colliderType)
        {
            UpdateMeshRenderer(mesh, material);
            transform.localScale = Vector3.one * scale;
            UpdateCollider(mesh, colliderType);
        }

        // --- Interne Logik ---

        /// <summary>
        /// Initialisiert den Rigidbody und setzt die Constraints.
        /// </summary>
        private void InitializeRigidbody()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _rigidBody.useGravity = false;
            _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidBody.sleepThreshold = 0f;
            // Die Bitweise-Operation ist hier korrekt, da Unity dies für Constraints vorsieht.
            _rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        /// <summary>
        /// Bewegt den Agenten vorwärts.
        /// </summary>
        private void MoveForward()
        {
            _rigidBody.linearVelocity = transform.forward * movementSpeed;
        }

        /// <summary>
        /// Setzt die Drehgeschwindigkeit zurück.
        /// </summary>
        private void StabilizePhysics()
        {
            _rigidBody.angularVelocity = Vector3.zero;
        }

        /// <summary>
        /// Prüft, ob der Cooldown nach einer Kollision aktiv ist.
        /// </summary>
        /// <returns>True, wenn Cooldown aktiv ist.</returns>
        private bool IsOnCooldown()
        {
            return Time.time - _lastCollisionTime < collisionCooldownDuration;
        }

        /// <summary>
        /// Prüft, ob der Agent außerhalb des Bereichs ist und dreht ihn ggf. um.
        /// </summary>
        private void CheckAndHandleBounds()
        {
            if (areaSize == Vector3.zero) return;
            if (IsOutOfBounds() && !IsOnCooldown())
            {
                PerformTurnAround();
            }
        }

        /// <summary>
        /// Prüft, ob der Agent außerhalb des erlaubten Bereichs ist.
        /// </summary>
        /// <returns>True, wenn außerhalb.</returns>
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

        /// <summary>
        /// Dreht den Agenten um 180 Grad und verschiebt ihn leicht.
        /// </summary>
        private void PerformTurnAround()
        {
            _lastCollisionTime = Time.time;
            RotateCharacter(180f);
            ApplyNudge();
        }

        /// <summary>
        /// Reagiert auf eine Kollision mit einer zufälligen Drehung und Nudge.
        /// </summary>
        private void HandleCollisionReaction()
        {
            _lastCollisionTime = Time.time;
            float randomAngle = _turnAngles[Random.Range(0, _turnAngles.Length)];
            RotateCharacter(randomAngle);
            MoveForward();
            ApplyNudge();
        }

        /// <summary>
        /// Dreht den Agenten um den gegebenen Winkel.
        /// </summary>
        /// <param name="angle">Winkel in Grad</param>
        private void RotateCharacter(float angle)
        {
            transform.Rotate(0f, angle, 0f, Space.Self);
        }

        /// <summary>
        /// Verschiebt den Agenten leicht nach vorne.
        /// </summary>
        private void ApplyNudge()
        {
            transform.position += transform.forward * bounceNudgeDistance;
        }

        /// <summary>
        /// Aktualisiert Mesh und Material des Agenten.
        /// </summary>
        private void UpdateMeshRenderer(Mesh mesh, Material material)
        {
            if (!TryGetComponent(out MeshFilter mf)) mf = gameObject.AddComponent<MeshFilter>();
            if (!TryGetComponent(out MeshRenderer mr)) mr = gameObject.AddComponent<MeshRenderer>();
            mf.mesh = mesh;
            mr.sharedMaterial = material;
        }

        /// <summary>
        /// Erstellt und konfiguriert den Collider.
        /// </summary>
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

        /// <summary>
        /// Entfernt alle vorhandenen Collider.
        /// </summary>
        private void RemoveExistingColliders()
        {
            foreach (var c in GetComponents<Collider>())
            {
                Destroy(c);
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

