using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace OOP_Scripts
{
    public class Spawner : MonoBehaviour
    {
        [Header("Settings")]
        public int count = 20;
        public Vector3 areaSize = new Vector3(10f, 2f, 10f);

        [Header("UI")]
        [Tooltip("Hier das TextMeshPro Feld reinziehen, das die Anzahl anzeigen soll")]
        public TMP_Text countText; // <--- NEU: Das Textfeld

        [Header("Visuals")]
        public GameObject malePrefab;
        public GameObject femalePrefab;
        public float agentScale = 1f;

        [Header("Movement")]
        public float minSpeed = 0.5f;
        public float maxSpeed = 2f;
        public Transform parentForAgents;
        public LayerMask overlapMask = ~0;

        [Header("Spawn Settings")]
        public int maxPlacementAttempts = 10;
        public bool randomizeRotY = true;

        private List<GameObject> spawned = new List<GameObject>();
        private int lastCount;

        void Start()
        {
            SpawnAgents();
            lastCount = count;
        }

        void Update()
        {
            // Prüft, ob sich die Zahl geändert hat (durch Slider oder Inspector)
            if (count != lastCount)
            {
                SpawnAgents();
                lastCount = count;
            }
        }

        public void SetCountFromSlider(float sliderValue)
        {
            int newCount = Mathf.RoundToInt(sliderValue);

            if (count != newCount)
            {
                count = newCount;
                // Update erkennt die Änderung im nächsten Frame und ruft SpawnAgents auf
            }
        }

        public void SpawnAgents()
        {
            ClearExisting();

            // Sicherheitscheck
            if (malePrefab == null && femalePrefab == null) return;

            for (int i = 0; i < count; i++)
            {
                GameObject prefabToSpawn = (i % 2 == 0) ? malePrefab : femalePrefab;
                if (prefabToSpawn == null) prefabToSpawn = (malePrefab != null) ? malePrefab : femalePrefab;
                if (prefabToSpawn == null) continue;

                Vector3 pos = Vector3.zero;
                bool placed = false;
                float approxRadius = GetPrefabRadius(prefabToSpawn);

                for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
                {
                    pos = GetRandomPositionInside();
                    if (Physics.OverlapSphere(pos, approxRadius * 0.6f, overlapMask).Length == 0)
                    {
                        placed = true;
                        break;
                    }
                }

                if (!placed) pos = GetRandomPositionInside();

                Quaternion initRot = Quaternion.Euler(0f, randomizeRotY ? Random.Range(0f, 360f) : 0f, 0f);
                GameObject go = Instantiate(prefabToSpawn, pos, initRot);
                
                go.name = $"{prefabToSpawn.name}_{i}";
                if (parentForAgents != null) go.transform.SetParent(parentForAgents, true);
                else go.transform.SetParent(transform, true);

                go.transform.localScale = Vector3.one * agentScale;

                var rb = go.GetComponent<Rigidbody>();
                if (rb == null) rb = go.AddComponent<Rigidbody>();
                
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate; 

                var walker = go.GetComponent<RandomWalker>();
                if (walker == null) walker = go.AddComponent<RandomWalker>();

                if (walker != null)
                {
                    walker.speed = Random.Range(minSpeed, maxSpeed);
                    walker.areaCenter = new Vector3(transform.position.x, 0f, transform.position.z);
                    walker.areaSize = areaSize;
                }

                spawned.Add(go);
            }

            // <--- NEU: Am Ende des Spawnens den Text aktualisieren
            UpdateTextDisplay();
        }

        // Neue Hilfsfunktion für den Text
        private void UpdateTextDisplay()
        {
            if (countText != null)
            {
                // Zeigt z.B. "Count: 20" an
                countText.text = $"Count: {count}";
            }
        }

        private float GetPrefabRadius(GameObject prefab)
        {
            var rend = prefab.GetComponentInChildren<Renderer>();
            if (rend != null) return rend.bounds.extents.magnitude * agentScale;
            return 0.5f * agentScale;
        }

        private Vector3 GetRandomPositionInside()
        {
            Vector3 half = areaSize * 0.5f;
            float x = Random.Range(-half.x, half.x);
            float z = Random.Range(-half.z, half.z);
            return new Vector3(transform.position.x + x, 0f, transform.position.z + z);
        }

        public void ClearExisting()
        {
            foreach (var g in spawned)
            {
                if (g != null) Destroy(g);
            }
            spawned.Clear();
        }

        void OnDrawGizmosSelected()
        {
            Vector3 center = new Vector3(transform.position.x, 0f, transform.position.z);
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawCube(center, areaSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, areaSize);
        }
    }
}