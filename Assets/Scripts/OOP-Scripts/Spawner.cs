using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace OOP_Scripts
{
    public class Spawner : MonoBehaviour
    {
        [Header("Spawn Einstellungen")]
        [SerializeField, Tooltip("Anzahl der Agenten, die generiert werden sollen.")]
        private int spawnCount = 20;

        [SerializeField, Tooltip("Größe des Bereichs, in dem die Agenten platziert werden.")]
        private Vector3 areaSize = new Vector3(10f, 2f, 10f);

        [SerializeField, Tooltip("Wie oft versucht wird, eine freie Position zu finden.")]
        private int maxPlacementAttempts = 10;
        
        [Header("UI Referenzen")]
        [SerializeField, Tooltip("Zeigt die aktuelle Anzahl der Agenten im UI an.")]
        private TMP_Text countDisplay;

        [Header("Visuals")]
        [SerializeField] private GameObject malePrefab;
        [SerializeField] private GameObject femalePrefab;
        [SerializeField] private float agentScale = 1f;

        [Header("Agenten Konfiguration")]
        [SerializeField] private float minSpeed = 0.5f;
        [SerializeField] private float maxSpeed = 2f;
        [SerializeField] private bool randomizeRotationY = true;
        
        [Header("Infrastruktur")]
        [SerializeField, Tooltip("Optional: Container-Objekt.")]
        private Transform agentContainer;
        
        [SerializeField, Tooltip("Layer für Kollisionsprüfung.")]
        private LayerMask collisionCheckLayer = ~0;

        private readonly List<GameObject> _spawnedAgents = new List<GameObject>();
        [Header("Optional UI")]
        [SerializeField, Tooltip("Optional: UI Slider, damit Start-Wert synchronisiert wird.")]
        private Slider countSlider;

        private void Start()
        {
            // Sorge dafür, dass der Slider beim Start mit dem aktuellen spawnCount synchronisiert wird
            // ohne das OnValueChanged-Event auszulösen (falls ein Slider referenziert wurde).
            if (countSlider != null)
            {
                countSlider.SetValueWithoutNotify(spawnCount);
            }

            RespawnAllAgents();
        }

        // Neue inkrementelle Variante: fügt nur Differenz hinzu oder entfernt sie.
        public void SetCountFromSlider(float sliderValue)
        {
            int newCount = Mathf.Max(0, Mathf.RoundToInt(sliderValue));
            if (spawnCount == newCount) return;

            if (newCount > spawnCount)
            {
                int toAdd = newCount - spawnCount;
                AddAgents(toAdd);
            }
            else
            {
                int toRemove = spawnCount - newCount;
                RemoveAgents(toRemove);
            }

            spawnCount = newCount;
            UpdateUiText();
        }

        // Fügt eine Anzahl von Agenten hinzu (benutzt die aktuelle Liste-Größe als Indexbasis)
        private void AddAgents(int countToAdd)
        {
            if (!HasValidPrefabs())
            {
                Debug.LogWarning("Spawner: Keine Prefabs zugewiesen! Keine Agenten hinzugefügt.");
                return;
            }

            for (int i = 0; i < countToAdd; i++)
            {
                // Verwende die aktuelle Anzahl der vorhandenen Agenten als Basis für den Index
                SpawnSingleAgent(_spawnedAgents.Count);
            }
        }

        // Entfernt die letzten N Agenten (sofern vorhanden)
        private void RemoveAgents(int countToRemove)
        {
            for (int i = 0; i < countToRemove && _spawnedAgents.Count > 0; i++)
            {
                int lastIndex = _spawnedAgents.Count - 1;
                GameObject agent = _spawnedAgents[lastIndex];
                if (agent != null) Destroy(agent);
                _spawnedAgents.RemoveAt(lastIndex);
            }
        }

        public void RespawnAllAgents()
        {
            ClearExistingAgents();

            if (!HasValidPrefabs())
            {
                Debug.LogWarning("Spawner: Keine Prefabs zugewiesen!");
                return;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnSingleAgent(i);
            }

            UpdateUiText();
        }

        private void ClearExistingAgents()
        {
            foreach (var agent in _spawnedAgents)
            {
                if (agent != null) Destroy(agent);
            }
            _spawnedAgents.Clear();
        }

        private void SpawnSingleAgent(int index)
        {
            GameObject prefabToSpawn = SelectPrefab(index);
            if (prefabToSpawn == null) return;

            float radius = GetPrefabRadius(prefabToSpawn);
            Vector3 position = TryFindSpawnPosition(radius);
            Quaternion rotation = GetInitialRotation();

            GameObject newAgent = Instantiate(prefabToSpawn, position, rotation);
            
            SetupAgentHierarchy(newAgent, prefabToSpawn.name, index);
            
            // WICHTIG: Wir müssen den Rigidbody nicht manuell konfigurieren.
            // Das erledigt der RandomWalker in seiner Awake()-Methode selbst.
            
            SetupRandomWalker(newAgent);

            _spawnedAgents.Add(newAgent);
        }

        private bool HasValidPrefabs()
        {
            return malePrefab != null || femalePrefab != null;
        }

        private GameObject SelectPrefab(int index)
        {
            if (malePrefab != null && femalePrefab != null)
            {
                return (index % 2 == 0) ? malePrefab : femalePrefab;
            }
            return malePrefab != null ? malePrefab : femalePrefab;
        }

        private Vector3 TryFindSpawnPosition(float agentRadius)
        {
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                Vector3 candidatePos = GetRandomPositionInArea();
                
                if (Physics.OverlapSphere(candidatePos, agentRadius * 0.6f, collisionCheckLayer).Length == 0)
                {
                    return candidatePos;
                }
            }
            return GetRandomPositionInArea();
        }

        private Vector3 GetRandomPositionInArea()
        {
            Vector3 halfSize = areaSize * 0.5f;
            float randomX = Random.Range(-halfSize.x, halfSize.x);
            float randomZ = Random.Range(-halfSize.z, halfSize.z);
            
            // Wichtig: Position relativ zum Spawner-Zentrum
            return new Vector3(transform.position.x + randomX, 0f, transform.position.z + randomZ);
        }

        private Quaternion GetInitialRotation()
        {
            float yAngle = randomizeRotationY ? Random.Range(0f, 360f) : 0f;
            return Quaternion.Euler(0f, yAngle, 0f);
        }

        private float GetPrefabRadius(GameObject prefab)
        {
            var rend = prefab.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                return rend.bounds.extents.magnitude * agentScale;
            }
            return 0.5f * agentScale; 
        }

        private void SetupAgentHierarchy(GameObject agent, string baseName, int index)
        {
            agent.name = $"{baseName}_{index}";
            agent.transform.localScale = Vector3.one * agentScale;

            if (agentContainer != null)
                agent.transform.SetParent(agentContainer, true);
            else
                agent.transform.SetParent(transform, true);
        }

        private void SetupRandomWalker(GameObject agent)
        {
            // Holt die Komponente oder fügt sie hinzu.
            // Durch [RequireComponent] am Walker wird hier automatisch auch ein Rigidbody erzeugt/geprüft.
            if (!agent.TryGetComponent(out RandomMovement walker))
            {
                walker = agent.AddComponent<RandomMovement>();
            }

            // Hier nutzen wir nun die saubere Public API statt direkt auf private Felder zuzugreifen
            float randomSpeed = Random.Range(minSpeed, maxSpeed);
            Vector3 areaCenter = new Vector3(transform.position.x, 0f, transform.position.z);
            
            walker.InitializeMovementSettings(randomSpeed, areaCenter, areaSize);
        }

        private void UpdateUiText()
        {
            if (countDisplay != null)
            {
                countDisplay.text = $"{spawnCount}";
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = new Vector3(transform.position.x, 0f, transform.position.z);
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawCube(center, areaSize);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, areaSize);
        }
    }
}