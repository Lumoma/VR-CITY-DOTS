using UnityEngine;
using UnityEngine.SceneManagement; // Wichtig für Szenenwechsel
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
    [Tooltip("Soll nach Abschluss automatisch die nächste Szene geladen werden?")]
    public bool enableAutoSwitching = true;

    [Tooltip("Liste der Szenennamen, die nacheinander abgearbeitet werden sollen (exakt wie im Build Settings).")]
    public List<string> scenesToTest = new List<string>();

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
    private int _currentSceneIndex = 0;
    
    // Damit wir wissen, was wir gerade testen (wird automatisch erkannt)
    private string _currentTypeString = "Unknown"; 

    private void Awake()
    {
        // 1. Singleton-Check & Überleben sichern
        // Wir suchen, ob es schon einen BenchmarkManager gibt
        var existingManagers = FindObjectsOfType<AutomatedBenchmark>();
        
        if (existingManagers.Length > 1)
        {
            // Wenn wir neu in eine Szene geladen werden, aber schon ein Manager aus der alten Szene da ist:
            // Zerstöre diesen neuen hier, damit der alte weitermachen kann.
            Destroy(gameObject);
            return;
        }

        // Mache dieses Objekt unsterblich
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Registriere dich für das Event, wenn eine Szene fertig geladen ist
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Wird von Unity automatisch aufgerufen, wenn eine neue Szene fertig geladen ist
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Wenn wir nicht im "Running" Modus sind, machen wir nichts (verhindert Start beim ersten Spielstart)
        if (_state != BenchmarkState.Running) return;

        StartCoroutine(InitAndRunSequence());
    }

    // Button-Methode zum Starten des gesamten Prozesses
    public void StartFullBenchmarkChain()
    {
        if (_state == BenchmarkState.Running) return;
        
        _state = BenchmarkState.Running;
        _currentSceneIndex = 0;

        // Falls wir eine Szenenliste haben, fangen wir mit der ersten an,
        // falls wir nicht schon zufällig in der richtigen sind.
        if (enableAutoSwitching && scenesToTest.Count > 0)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            string firstTarget = scenesToTest[0];

            if (currentSceneName != firstTarget)
            {
                LoadNextTargetScene(0);
                return; 
            }
        }

        // Wenn wir schon in der richtigen Szene sind oder keine Liste haben -> direkt loslegen
        StartCoroutine(InitAndRunSequence());
    }

    private IEnumerator InitAndRunSequence()
    {
        // Warte kurz einen Frame, damit alle anderen Scripts in der Szene ihre Awake/Start fertig haben
        yield return null;

        LogStatus($"Szene geladen: {SceneManager.GetActiveScene().name}. Suche Referenzen...");

        // AUTOMATISCHE ERKENNUNG: OOP oder DOTS?
        bool foundTarget = FindReferencesInScene();

        if (foundTarget)
        {
            yield return StartCoroutine(RunMeasurementLoop());
        }
        else
        {
            Debug.LogError("Keinen Spawner (weder OOP noch DOTS) in dieser Szene gefunden!");
        }

        // Benchmark in dieser Szene fertig. Was nun?
        if (enableAutoSwitching)
        {
            ProceedToNextScene();
        }
        else
        {
            _state = BenchmarkState.Finished;
            LogStatus("Benchmark beendet (AutoSwitch aus).");
        }
    }

    private bool FindReferencesInScene()
    {
        // Reset
        _currentOopSpawner = null;
        _currentDotsBridge = null;

        // Versuch 1: OOP Spawner finden
        _currentOopSpawner = FindFirstObjectByType<Spawner>(); // Unity 2023+ (sonst FindObjectOfType)
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

        // Header inklusive Szenen-Name
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

            // 4. Daten speichern
            RecordMetric(csv, sceneName, count, frames);
        }

        // Datei speichern (Pro Szene eine Datei ist sicherer, falls was abstürzt)
        SaveFile(sceneName, csv.ToString());
        
        // Aufräumen: Agenten auf 0 setzen
        SetAgentCount(0);
        yield return new WaitForSeconds(1.0f); // Kurze Pause vorm Wechsel
    }

    private void ProceedToNextScene()
    {
        _currentSceneIndex++;

        if (_currentSceneIndex < scenesToTest.Count)
        {
            LoadNextTargetScene(_currentSceneIndex);
        }
        else
        {
            _state = BenchmarkState.Finished;
            LogStatus("ALLE Szenen erfolgreich getestet!");
            
            // Optional: Manager zerstören, wenn alles fertig ist
            // Destroy(gameObject); 
        }
    }

    private void LoadNextTargetScene(int index)
    {
        string nextScene = scenesToTest[index];
        LogStatus($"Lade nächste Szene: {nextScene}...");
        SceneManager.LoadScene(nextScene);
    }

    // --- Hilfsfunktionen ---

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

        // 1% Low
        frameTimes.Sort((a, b) => b.CompareTo(a)); 
        int index1Percent = Mathf.CeilToInt(frameTimes.Count * 0.01f);
        float p1Low = 1.0f / frameTimes[Mathf.Clamp(index1Percent, 0, frameTimes.Count - 1)];

        string line = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0};{1};{2};{3:F2};{4:F2};{5:F2};{6:F2};{7:F4}",
            sceneName, _currentTypeString, count, avgFPS, minFPS, maxFPS, p1Low, avgFrameTime * 1000f
        );
        csv.AppendLine(line);
    }

    private void SaveFile(string sceneName, string content)
    {
        // Füge einen Zeitstempel zum Dateinamen hinzu (sicheres Format für Dateinamen)
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{baseFileName}_{sceneName}_{_currentTypeString}_{timestamp}.csv";
        string path = Path.Combine(Application.dataPath, "../", fileName);
        File.WriteAllText(path, content);
        Debug.Log($"<color=green>Gespeichert: {path}</color>");
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

