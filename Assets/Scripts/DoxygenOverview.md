# Doxygen Dokumentation für KIT-VR-CITY

Dieses Dokument beschreibt die Struktur und den Zweck der wichtigsten Scripte in den folgenden Ordnern:
- OOP-Scripts
- DOTS-Scripts
- Metric-Scripts

![All-UML](UML-Diagramme/All-Scripts-UML.png)

## OOP-Scripts

Enthält klassische objektorientierte Scripte für das Verhalten von Agenten und die Steuerung der Szene.

| Datei                | Beschreibung |
|----------------------|--------------|
| RandomMovement.cs    | Implementiert zufällige Bewegungen für Agenten/Prefabs. |
| Spawner.cs           | Verantwortlich für das Erzeugen (Spawnen) von Agenten/Prefabs in der Szene. |

![OOP-UML](UML-Diagramme/OOP-Scripts-UML.png)

## DOTS-Scripts

Enthält Scripte, die auf Unity DOTS (Data-Oriented Technology Stack) basieren und für performante Simulationen mit vielen Entitäten optimiert sind.

| Datei                      | Beschreibung |
|----------------------------|--------------|
| EntityCountUIBridge.cs     | Bindeglied zwischen DOTS-Entitäten und UI zur Anzeige der Entitätsanzahl. |
| RandomMovementAuthoring.cs | Authoring-Komponente für zufällige Bewegungen (DOTS). |
| RandomMovementData.cs      | Datenstruktur für zufällige Bewegungen (DOTS). |
| RandomMovementSystem.cs    | System zur Steuerung der zufälligen Bewegungen (DOTS). |
| SpawnerAuthoring.cs        | Authoring-Komponente für das Spawnen von Entitäten (DOTS). |
| SpawnerData.cs             | Datenstruktur für das Spawnen von Entitäten (DOTS). |
| SpawnerSystem.cs           | System zum Spawnen von Entitäten (DOTS). |
| SpawnerUIBridge.cs         | Bindeglied zwischen Spawner-System und UI (DOTS). |

![DOTS-UML](UML-Diagramme/DOTS-Scripts-UML.png)

## Metric-Scripts

Enthält Scripte zur Messung und Auswertung von Performance-Metriken.

| Datei                | Beschreibung |
|----------------------|--------------|
| AutomatedBenchmark.cs| Automatisiertes Benchmarking-Script zur Messung von Performance-Kennzahlen. |
| VRFPSCounter.cs      | Misst und zeigt die aktuelle Framerate (FPS) im VR-Modus an. |
| VRSceneSwitcher.cs   | Ermöglicht das Wechseln zwischen verschiedenen Szenen im VR-Modus. |

![Metric-UML](UML-Diagramme/Metric-Scripts-UML.png)

---

**Hinweis:**
- Die .cs.meta-Dateien werden für die Doxygen-Dokumentation ignoriert.
- Weitere Doxygen-Kommentare können an Klassen, Methoden und Feldern ergänzt werden.

