/**
 * @file AutomatedBenchmark.cs
 * @brief Automatisiertes Benchmarking-Script zur Messung von Performance-Kennzahlen.
 *
 * Dieses Script misst automatisch verschiedene Performance-Metriken in der Szene.
 */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using OOP_Scripts;
using DOTS_Scripts;
using System;

namespace Metric_Scripts
{
    /// <summary>
    /// Führt automatisierte Benchmarks für verschiedene Agentenzahlen durch und speichert die Ergebnisse als CSV.
    /// </summary>
    public class AutomatedBenchmark : MonoBehaviour
    {
        /// <summary>
        /// Status des Benchmarks.
        /// </summary>
        public enum BenchmarkState { Idle, Running, Finished }

        [Header("Benchmark Einstellungen")]
        /// <summary>
        /// Getestete Agentenzahlen.
        /// </summary>
        public int[] agentCounts = { 100, 500, 1000, 2500, 5000, 7500, 10000 };
        /// <summary>
        /// Aufwärmzeit vor Messung (Sekunden).
        /// </summary>
        public float warmupTime = 2.0f;
        /// <summary>
        /// Messdauer pro Schritt (Sekunden).
        /// </summary>
        public float measureTime = 5.0f;
        /// <summary>
        /// Basisname für die Ergebnisdatei.
        /// </summary>
        public string baseFileName = "BenchmarkResult";

        [Header("Optional UI")]
        /// <summary>
        /// Optionales Textfeld zur Statusanzeige.
        /// </summary>
        public TMP_Text statusText;

        private Spawner _currentOopSpawner;
        private SpawnerUIBridge _currentDotsBridge;
        private BenchmarkState _state = BenchmarkState.Idle;
        private string _currentTypeString = "Unknown";

        /// <summary>
        /// Startet die Benchmark-Sequenz (z.B. per Button).
        /// </summary>
        public void StartFullBenchmarkChain()
        {
            if (_state == BenchmarkState.Running) return;
            _state = BenchmarkState.Running;
            StartCoroutine(InitAndRunSequence());
        }

        /// <summary>
        /// Initialisiert und führt die Benchmark-Sequenz aus.
        /// </summary>
        private IEnumerator InitAndRunSequence()
        {
            yield return null;
            LogStatus($"Szene: {SceneManager.GetActiveScene().name}. Suche Spawner...");
            bool foundTarget = FindReferencesInScene();
            if (foundTarget)
            {
                yield return StartCoroutine(RunMeasurementLoop());
            }
            else
            {
                Debug.LogError("Keinen Spawner (weder OOP noch DOTS) gefunden!");
                LogStatus("Fehler: Kein Spawner gefunden.");
            }
            _state = BenchmarkState.Finished;
            LogStatus("Benchmark beendet. Datei gespeichert.");
        }

        /// <summary>
        /// Sucht OOP- oder DOTS-Spawner in der Szene.
        /// </summary>
        private bool FindReferencesInScene()
        {
            _currentOopSpawner = null;
            _currentDotsBridge = null;
            _currentOopSpawner = FindFirstObjectByType<Spawner>();
            if (_currentOopSpawner != null)
            {
                _currentTypeString = "OOP";
                LogStatus("OOP Spawner erkannt.");
                return true;
            }
            _currentDotsBridge = FindFirstObjectByType<SpawnerUIBridge>();
            if (_currentDotsBridge != null)
            {
                _currentTypeString = "DOTS";
                LogStatus("DOTS Bridge erkannt.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Führt die Messschleife für alle Agentenzahlen durch.
        /// </summary>
        private IEnumerator RunMeasurementLoop()
        {
            StringBuilder csv = new StringBuilder();
            string sceneName = SceneManager.GetActiveScene().name;
            csv.AppendLine("Scene;Type;AgentCount;AvgFPS;MinFPS;MaxFPS;1PercentLowFPS;AvgFrameTime_ms");
            foreach (int count in agentCounts)
            {
                LogStatus($"Messe {count} Agenten ({_currentTypeString})...");
                SetAgentCount(count);
                yield return new WaitForSeconds(warmupTime);
                List<float> frames = new List<float>();
                float elapsed = 0f;
                while (elapsed < measureTime)
                {
                    float dt = Time.unscaledDeltaTime;
                    frames.Add(dt);
                    elapsed += dt;
                    yield return null;
                }
                RecordMetric(csv, sceneName, count, frames);
            }
            SaveFile(sceneName, csv.ToString());
            SetAgentCount(0);
            yield return new WaitForSeconds(1.0f);
        }

        /// <summary>
        /// Setzt die Agentenzahl im jeweiligen Spawner.
        /// </summary>
        private void SetAgentCount(int count)
        {
            if (_currentTypeString == "OOP" && _currentOopSpawner != null)
            {
                _currentOopSpawner.SetCountFromSlider(count);
            }
            else if (_currentTypeString == "DOTS" && _currentDotsBridge != null)
            {
                _currentDotsBridge.OnSliderValueChanged(count);
            }
        }

        /// <summary>
        /// Berechnet und speichert die Metriken für einen Messschritt.
        /// </summary>
        private void RecordMetric(StringBuilder csv, string sceneName, int count, List<float> frameTimes)
        {
            if (frameTimes.Count == 0) return;
            float avgFrameTime = frameTimes.Average();
            float avgFPS = 1.0f / avgFrameTime;
            float minFPS = 1.0f / frameTimes.Max();
            float maxFPS = 1.0f / frameTimes.Min();
            frameTimes.Sort((a, b) => b.CompareTo(a));
            int index1Percent = Mathf.CeilToInt(frameTimes.Count * 0.01f);
            float p1Low = 1.0f / frameTimes[Mathf.Clamp(index1Percent, 0, frameTimes.Count - 1)];
            string line = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0};{1};{2};{3:F2};{4:F2};{5:F2};{6:F2};{7:F4}",
                sceneName, _currentTypeString, count, avgFPS, minFPS, maxFPS, p1Low, avgFrameTime * 1000f
            );
            csv.AppendLine(line);
        }

        /// <summary>
        /// Speichert die CSV-Datei auf dem Gerät (PC oder Android).
        /// </summary>
        private void SaveFile(string sceneName, string content)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{baseFileName}_{sceneName}_{_currentTypeString}_{timestamp}.csv";
            string path;
            if (Application.platform == RuntimePlatform.Android)
            {
                string folder = Path.Combine(Application.persistentDataPath, "BenchmarkResults");
                if (!Directory.Exists(folder)) 
                {
                    Directory.CreateDirectory(folder);
                }
                path = Path.Combine(folder, fileName);
            }
            else
            {
                path = Path.Combine(Application.dataPath, "../", fileName);
            }
            try
            {
                File.WriteAllText(path, content);
                Debug.Log($"<color=green>Gespeichert: {path}</color>");
                LogStatus("CSV gespeichert!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Fehler beim Speichern: {e.Message}");
            }
        }

        /// <summary>
        /// Gibt Statusmeldungen im UI und in der Konsole aus.
        /// </summary>
        private void LogStatus(string msg)
        {
            if (statusText != null)
            {
                statusText.text = msg;
            }
            Debug.Log($"[AutoBenchmark] {msg}");
        }
    }
}
