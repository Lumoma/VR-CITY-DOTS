namespace OOP_Scripts
{
    using UnityEngine;

    public class RandomWalker : MonoBehaviour
    {
        public float speed = 1f;
        public Vector3 areaCenter;
        public Vector3 areaSize;
        public float bounceNudge = 0.05f;
        public float collisionCooldown = 0.15f;

        private Rigidbody rb;
        private float[] turnOptions = new float[] { -90f, 90f, 180f };
        private float lastCollisionTime = -10f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            
            rb.useGravity = false;
            // WICHTIG: Wir frieren X und Z ein, damit er nicht umfällt. 
            // Y lassen wir hier "frei" in den Constraints, kontrollieren es aber manuell im Code.
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            // Verhindert, dass der Rigidbody einschläft, wenn er sich langsam bewegt
            rb.sleepThreshold = 0f; 
        }

        void FixedUpdate()
        {
            if (rb != null)
            {
                // 1. Vorwärts bewegen
                rb.linearVelocity = transform.forward * speed;

                // 2. WICHTIG: Physikalische Drehung komplett stoppen!
                // Das verhindert, dass Kollisionen den Charakter in eine "Kreisbahn" schubsen.
                rb.angularVelocity = Vector3.zero; 
            }

            // Bounds check
            var min = areaCenter - areaSize * 0.5f;
            var max = areaCenter + areaSize * 0.5f;
            Vector3 pos = transform.position;

            // Check, ob wir außerhalb sind
            if ((pos.x < min.x || pos.x > max.x || pos.y < min.y || pos.y > max.y || pos.z < min.z || pos.z > max.z)
                && Time.time - lastCollisionTime > collisionCooldown)
            {
                lastCollisionTime = Time.time;
                // Drehung und kleiner Schubs zurück ins Feld
                RotateBy(180f);
                // Wichtig: Position direkt ändern kann physikalisch hakelig sein, 
                // aber für diesen "Bounce" ist es okay, solange kein Collider im Weg ist.
                transform.position += transform.forward * bounceNudge;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (Time.time - lastCollisionTime < collisionCooldown) return;
            lastCollisionTime = Time.time;

            float angle = turnOptions[Random.Range(0, turnOptions.Length)];
            RotateBy(angle);

            // Velocity sofort aktualisieren, damit er nicht einen Frame lang stehen bleibt
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * speed;
                rb.angularVelocity = Vector3.zero; // Auch hier sicherheitshalber nullen
            }
            
            // Nudge
            transform.position += transform.forward * bounceNudge;
        }

        private void RotateBy(float yAngle)
        {
            // Wir drehen den Transform hart. Da angularVelocity in FixedUpdate genullt wird,
            // gibt es keinen Konflikt.
            transform.Rotate(0f, yAngle, 0f, Space.Self);
        }

        // ... (Rest der Klasse ApplyVisual etc. bleibt unverändert) ...
        public void ApplyVisual(Mesh mesh, Material mat, float scale, ColliderType colliderType)
        {
            // Dein bestehender Code hier...
            var mf = GetComponent<MeshFilter>();
            if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
            mf.mesh = mesh;

            var mr = GetComponent<MeshRenderer>();
            if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            transform.localScale = Vector3.one * scale;

            var existingBox = GetComponent<BoxCollider>();
            var existingSphere = GetComponent<SphereCollider>();
            if (existingBox != null) Destroy(existingBox);
            if (existingSphere != null) Destroy(existingSphere);

            Bounds b = mesh != null ? mesh.bounds : new Bounds(Vector3.zero, Vector3.one);

            if (colliderType == ColliderType.Box)
            {
                var bc = gameObject.AddComponent<BoxCollider>();
                bc.center = Vector3.Scale(b.center, transform.localScale);
                bc.size = Vector3.Scale(b.size, transform.localScale);
            }
            else
            {
                var sc = gameObject.AddComponent<SphereCollider>();
                sc.center = Vector3.Scale(b.center, transform.localScale);
                sc.radius = Mathf.Max(Vector3.Scale(b.extents, transform.localScale).x,
                                      Vector3.Scale(b.extents, transform.localScale).y,
                                      Vector3.Scale(b.extents, transform.localScale).z);
            }
        }
        public enum ColliderType { Box, Sphere }
    }
}