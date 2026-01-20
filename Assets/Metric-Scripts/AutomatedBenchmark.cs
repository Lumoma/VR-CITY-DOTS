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

public class AutomatedBenchmark : MonoBehaviour
{
    public enum BenchmarkState { Idle, Running, Finished }

    [Header("Benchmark Einstellungen")]
    // Wir testen nur die aktuelle Szene

    [Header("Metrik Konfiguration")]
    public int[] agentCounts = { 100, 500, 1000, 2500, 5000, 7500, 10000 };
    public float warmupTime = 2.0f;
    public float measureTime = 5.0f;
    public string baseFileName = "BenchmarkResult";

    [Header("Optional UI")]
    public TMP_Text statusText;

    // Interne Referenzen
    private Spawner _currentOopSpawner;
    private SpawnerUIBridge _currentDotsBridge;
    private BenchmarkState _state = BenchmarkState.Idle;
    
    // Damit wir wissen, was wir gerade testen (wird automatisch erkannt)
    private string _currentTypeString = "Unknown"; 

    // Button-Methode zum Starten des Benchmarks in der aktuellen Szene
    public void StartFullBenchmarkChain()
    {
        if (_state == BenchmarkState.Running) return;
        
        _state = BenchmarkState.Running;
        StartCoroutine(InitAndRunSequence());
    }

    private IEnumerator InitAndRunSequence()
    {
        // Warte kurz einen Frame, damit alle anderen Scripts bereit sind
        yield return null;

        LogStatus($"Szene: {SceneManager.GetActiveScene().name}. Suche Spawner...");

        // AUTOMATISCHE ERKENNUNG: OOP oder DOTS?
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

    private bool FindReferencesInScene()
    {
        _currentOopSpawner = null;
        _currentDotsBridge = null;

        // Versuch 1: OOP Spawner finden
        // Hinweis: FindFirstObjectByType ist performanter ab Unity 2023, sonst FindObjectOfType nutzen
        _currentOopSpawner = FindFirstObjectByType<Spawner>(); 
        if (_currentOopSpawner != null)
        {
            _currentTypeString = "OOP";
            LogStatus("OOP Spawner erkannt.");
            return true;
        }

        // Versuch 2: DOTS Bridge finden
        _currentDotsBridge = FindFirstObjectByType<SpawnerUIBridge>();
        if (_currentDotsBridge != null)
        {
            _currentTypeString = "DOTS";
            LogStatus("DOTS Bridge erkannt.");
            return true;
        }

        return false;
    }

    private IEnumerator RunMeasurementLoop()
    {
        StringBuilder csv = new StringBuilder();
        string sceneName = SceneManager.GetActiveScene().name;

        // CSV Header
        csv.AppendLine("Scene;Type;AgentCount;AvgFPS;MinFPS;MaxFPS;1PercentLowFPS;AvgFrameTime_ms");

        foreach (int count in agentCounts)
        {
            LogStatus($"Messe {count} Agenten ({_currentTypeString})...");

            // 1. Setzen
            SetAgentCount(count);

            // 2. Warmup
            yield return new WaitForSeconds(warmupTime);

            // 3. Messen
            List<float> frames = new List<float>();
            float elapsed = 0f;
            while (elapsed < measureTime)
            {
                float dt = Time.unscaledDeltaTime;
                frames.Add(dt);
                elapsed += dt;
                yield return null;
            }

            // 4. Daten temporär erfassen
            RecordMetric(csv, sceneName, count, frames);
        }

        // Speichern
        SaveFile(sceneName, csv.ToString());
        
        // Aufräumen: Agenten auf 0 setzen
        SetAgentCount(0);
        yield return new WaitForSeconds(1.0f);
    }

    private void SetAgentCount(int count)
    {
        if (_currentTypeString == "OOP" && _currentOopSpawner != null)
        {
            _currentOopSpawner.SetCountFromSlider((float)count);
        }
        else if (_currentTypeString == "DOTS" && _currentDotsBridge != null)
        {
            _currentDotsBridge.OnSliderValueChanged((float)count);
        }
    }

    private void RecordMetric(StringBuilder csv, string sceneName, int count, List<float> frameTimes)
    {
        if (frameTimes.Count == 0) return;

        float avgFrameTime = frameTimes.Average();
        float avgFPS = 1.0f / avgFrameTime;
        float minFPS = 1.0f / frameTimes.Max();
        float maxFPS = 1.0f / frameTimes.Min();

        // 1% Low Berechnung
        frameTimes.Sort((a, b) => b.CompareTo(a)); 
        int index1Percent = Mathf.CeilToInt(frameTimes.Count * 0.01f);
        float p1Low = 1.0f / frameTimes[Mathf.Clamp(index1Percent, 0, frameTimes.Count - 1)];

        string line = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0};{1};{2};{3:F2};{4:F2};{5:F2};{6:F2};{7:F4}",
            sceneName, _currentTypeString, count, avgFPS, minFPS, maxFPS, p1Low, avgFrameTime * 1000f
        );
        csv.AppendLine(line);
    }

    // SPEICHERFUNKTION FÜR QUEST 3 / ANDROID & PC
    private void SaveFile(string sceneName, string content)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{baseFileName}_{sceneName}_{_currentTypeString}_{timestamp}.csv";
        string path;

        // Prüfen, ob wir auf Android (Quest 3) sind
        if (Application.platform == RuntimePlatform.Android)
        {
            // Pfad: /storage/emulated/0/Android/data/<PackageName>/files/
            path = Path.Combine(Application.persistentDataPath, fileName);
        }
        else
        {
            // Pfad: Im Projektordner (neben dem Assets Ordner)
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
            LogStatus("Fehler beim Speichern (siehe Logs)");
        }
    }

    private void LogStatus(string msg)
    {
        if (statusText != null)
        {
            statusText.text = msg;
        }
        Debug.Log($"[AutoBenchmark] {msg}");
    }
}

