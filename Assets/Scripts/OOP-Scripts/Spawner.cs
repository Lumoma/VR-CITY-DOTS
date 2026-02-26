/**
 * @file Spawner.cs
 * @brief Verantwortlich für das Erzeugen (Spawnen) von Agenten/Prefabs in der Szene.
 *
 * Dieses Script erzeugt neue Agenten/Prefabs zur Laufzeit.
 */

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace OOP_Scripts
{
    /**
     * @file Spawner.cs
     * @brief Verantwortlich für das Erzeugen (Spawnen) von Agenten/Prefabs in der Szene.
     *
     * Dieses Script erzeugt neue Agenten/Prefabs zur Laufzeit.
     */
    /// <summary>
    /// Spawnt und verwaltet Agenten/Prefabs in der Szene. Unterstützt inkrementelles Hinzufügen und Entfernen.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        /// <summary>
        /// Anzahl der Agenten, die generiert werden sollen.
        /// </summary>
        [Header("Spawn Einstellungen")]
        [SerializeField, Tooltip("Anzahl der Agenten, die generiert werden sollen.")]
        private int spawnCount = 20;

        /// <summary>
        /// Größe des Bereichs, in dem die Agenten platziert werden.
        /// </summary>
        [SerializeField, Tooltip("Größe des Bereichs, in dem die Agenten platziert werden.")]
        private Vector3 areaSize = new Vector3(10f, 2f, 10f);

        /// <summary>
        /// Wie oft versucht wird, eine freie Position zu finden.
        /// </summary>
        [SerializeField, Tooltip("Wie oft versucht wird, eine freie Position zu finden.")]
        private int maxPlacementAttempts = 10;
        
        /// <summary>
        /// Zeigt die aktuelle Anzahl der Agenten im UI an.
        /// </summary>
        [Header("UI Referenzen")]
        [SerializeField, Tooltip("Zeigt die aktuelle Anzahl der Agenten im UI an.")]
        private TMP_Text countDisplay;

        /// <summary>
        /// Prefab für männliche Agenten.
        /// </summary>
        [Header("Visuals")]
        [SerializeField] private GameObject malePrefab;
        /// <summary>
        /// Prefab für weibliche Agenten.
        /// </summary>
        [SerializeField] private GameObject femalePrefab;
        /// <summary>
        /// Skalierung der Agenten.
        /// </summary>
        [SerializeField] private float agentScale = 1f;

        /// <summary>
        /// Minimale Bewegungsgeschwindigkeit der Agenten.
        /// </summary>
        [Header("Agenten Konfiguration")]
        [SerializeField] private float minSpeed = 0.5f;
        /// <summary>
        /// Maximale Bewegungsgeschwindigkeit der Agenten.
        /// </summary>
        [SerializeField] private float maxSpeed = 2f;
        /// <summary>
        /// Ob die Y-Rotation zufällig gewählt wird.
        /// </summary>
        [SerializeField] private bool randomizeRotationY = true;
        
        /// <summary>
        /// Optional: Container-Objekt für Agenten.
        /// </summary>
        [Header("Infrastruktur")]
        [SerializeField, Tooltip("Optional: Container-Objekt.")]
        private Transform agentContainer;
        
        /// <summary>
        /// Layer für Kollisionsprüfung.
        /// </summary>
        [SerializeField, Tooltip("Layer für Kollisionsprüfung.")]
        private LayerMask collisionCheckLayer = ~0;

        private readonly List<GameObject> _spawnedAgents = new List<GameObject>();
        /// <summary>
        /// Optional: UI Slider, damit Start-Wert synchronisiert wird.
        /// </summary>
        [Header("Optional UI")]
        [SerializeField, Tooltip("Optional: UI Slider, damit Start-Wert synchronisiert wird.")]
        private Slider countSlider;

        /// <summary>
        /// Initialisiert den Spawner und synchronisiert den UI-Slider.
        /// </summary>
        private void Start()
        {
            if (countSlider != null)
            {
                countSlider.SetValueWithoutNotify(spawnCount);
            }
            RespawnAllAgents();
        }

        /// <summary>
        /// Setzt die Agentenanzahl über den UI-Slider.
        /// </summary>
        /// <param name="sliderValue">Neuer Wert des Sliders</param>
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

        /// <summary>
        /// Fügt eine Anzahl von Agenten hinzu.
        /// </summary>
        /// <param name="countToAdd">Anzahl der hinzuzufügenden Agenten</param>
        private void AddAgents(int countToAdd)
        {
            if (!HasValidPrefabs())
            {
                Debug.LogWarning("Spawner: Keine Prefabs zugewiesen! Keine Agenten hinzugefügt.");
                return;
            }
            for (int i = 0; i < countToAdd; i++)
            {
                SpawnSingleAgent(_spawnedAgents.Count);
            }
        }

        /// <summary>
        /// Entfernt die letzten N Agenten.
        /// </summary>
        /// <param name="countToRemove">Anzahl der zu entfernenden Agenten</param>
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

        /// <summary>
        /// Löscht alle Agenten und spawnt die gewünschte Anzahl neu.
        /// </summary>
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

        /// <summary>
        /// Löscht alle existierenden Agenten.
        /// </summary>
        private void ClearExistingAgents()
        {
            foreach (var agent in _spawnedAgents)
            {
                if (agent != null) Destroy(agent);
            }
            _spawnedAgents.Clear();
        }

        /// <summary>
        /// Spawnt einen einzelnen Agenten.
        /// </summary>
        /// <param name="index">Index für die Namensgebung und Prefab-Auswahl</param>
        private void SpawnSingleAgent(int index)
        {
            GameObject prefabToSpawn = SelectPrefab(index);
            if (prefabToSpawn == null) return;
            float radius = GetPrefabRadius(prefabToSpawn);
            Vector3 position = TryFindSpawnPosition(radius);
            Quaternion rotation = GetInitialRotation();
            GameObject newAgent = Instantiate(prefabToSpawn, position, rotation);
            SetupAgentHierarchy(newAgent, prefabToSpawn.name, index);
            SetupRandomWalker(newAgent);
            _spawnedAgents.Add(newAgent);
        }

        /// <summary>
        /// Prüft, ob gültige Prefabs zugewiesen sind.
        /// </summary>
        /// <returns>True, wenn mindestens ein Prefab vorhanden ist.</returns>
        private bool HasValidPrefabs()
        {
            return malePrefab != null || femalePrefab != null;
        }

        /// <summary>
        /// Wählt das Prefab anhand des Index (abwechselnd männlich/weiblich).
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Prefab GameObject</returns>
        private GameObject SelectPrefab(int index)
        {
            if (malePrefab != null && femalePrefab != null)
            {
                return (index % 2 == 0) ? malePrefab : femalePrefab;
            }
            return malePrefab != null ? malePrefab : femalePrefab;
        }

        /// <summary>
        /// Sucht eine freie Spawnposition im Bereich.
        /// </summary>
        /// <param name="agentRadius">Radius des Agenten</param>
        /// <returns>Position im Bereich</returns>
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

        /// <summary>
        /// Gibt eine zufällige Position im Bereich zurück.
        /// </summary>
        /// <returns>Position</returns>
        private Vector3 GetRandomPositionInArea()
        {
            Vector3 halfSize = areaSize * 0.5f;
            float randomX = Random.Range(-halfSize.x, halfSize.x);
            float randomZ = Random.Range(-halfSize.z, halfSize.z);
            return new Vector3(transform.position.x + randomX, 0f, transform.position.z + randomZ);
        }

        /// <summary>
        /// Gibt die Startrotation zurück.
        /// </summary>
        /// <returns>Quaternion für die Rotation</returns>
        private Quaternion GetInitialRotation()
        {
            float yAngle = randomizeRotationY ? Random.Range(0f, 360f) : 0f;
            return Quaternion.Euler(0f, yAngle, 0f);
        }

        /// <summary>
        /// Ermittelt den Radius des Prefabs anhand des Renderers.
        /// </summary>
        /// <param name="prefab">Prefab</param>
        /// <returns>Radius</returns>
        private float GetPrefabRadius(GameObject prefab)
        {
            var rend = prefab.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                return rend.bounds.extents.magnitude * agentScale;
            }
            return 0.5f * agentScale; 
        }

        /// <summary>
        /// Setzt Name, Skalierung und Hierarchie für den Agenten.
        /// </summary>
        /// <param name="agent">Agent GameObject</param>
        /// <param name="baseName">Basisname</param>
        /// <param name="index">Index</param>
        private void SetupAgentHierarchy(GameObject agent, string baseName, int index)
        {
            agent.name = $"{baseName}_{index}";
            agent.transform.localScale = Vector3.one * agentScale;
            if (agentContainer != null)
                agent.transform.SetParent(agentContainer, true);
            else
                agent.transform.SetParent(transform, true);
        }

        /// <summary>
        /// Initialisiert das RandomMovement-Script des Agenten.
        /// </summary>
        /// <param name="agent">Agent GameObject</param>
        private void SetupRandomWalker(GameObject agent)
        {
            if (!agent.TryGetComponent(out RandomMovement walker))
            {
                walker = agent.AddComponent<RandomMovement>();
            }
            float randomSpeed = Random.Range(minSpeed, maxSpeed);
            Vector3 areaCenter = new Vector3(transform.position.x, 0f, transform.position.z);
            walker.InitializeMovementSettings(randomSpeed, areaCenter, areaSize);
        }

        /// <summary>
        /// Aktualisiert die UI-Anzeige der Agentenzahl.
        /// </summary>
        private void UpdateUiText()
        {
            if (countDisplay != null)
            {
                countDisplay.text = $"{spawnCount}";
            }
        }

        /// <summary>
        /// Zeichnet den Spawnbereich im Editor.
        /// </summary>
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

