# VR Traffic Simulation Benchmark

[![Unity Version](https://img.shields.io/badge/Unity-6000.0.60f1-black.svg?style=flat&logo=unity)](https://unity.com/releases/editor/whats-new/6000.0.60)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Meta Quest](https://img.shields.io/badge/Meta%20Quest-2%20%7C%203%20%7C%20Pro-0467DF.svg?style=flat&logo=meta)](https://www.meta.com/quest/)
[![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20Windows%20%7C%20macOS-green.svg)](https://unity.com/)

A comprehensive performance comparison between Unity's traditional **Object-Oriented Programming (OOP)** and **Data-Oriented Technology Stack (DOTS/ECS)** in a virtual reality environment, measuring simulation performance of autonomous agents in an urban setting.

![Screenshot 1](Assets/Screenshots/Screenshot_1.png)
*DOTS Performance Test - Shot on MacBook Pro M5*

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Technology Stack](#️-technology-stack)
- [Benchmark Scenes](#-benchmark-scenes)
- [Installation](#-installation)
- [Usage](#-usage)
- [Exporting Results](#-exporting-results)
- [Performance Notes](#-performance-notes)
- [Project Structure](#-project-structure)
- [Assets & Credits](#-assets--credits)
- [License](#-license)
- [Author](#-author)

---

## 📋 Overview

This project simulates urban traffic scenarios (vehicles and pedestrians) in VR to benchmark performance differences between two Unity architectural approaches:

- **OOP**: Traditional Unity GameObject-based architecture with MonoBehaviours
- **DOTS**: Unity's Entity Component System (ECS) with Job System and Burst Compiler

**Key Metrics:**
- 📊 Entity/Agent Count (up to 10,000)
- 🎮 FPS (Frames Per Second)
- ⏱️ Frame Time
- 📈 Exported as CSV for analysis

---

## ✨ Features

- ✅ **4 Benchmark Scenes** (OOP vs DOTS for People/Cars)
- ✅ **Automated Benchmarking** (incremental entity spawning)
- ✅ **Manual Testing Mode** (adjust entity count in real-time)
- ✅ **VR-Native UI** (Meta Quest controllers)
- ✅ **CSV Export** via ADB for data analysis
- ✅ **Cross-Platform** (Quest 2/3/Pro, Windows, macOS)
- ✅ **Scalable** (tested up to 10,000 entities on DOTS)

---

## 🛠️ Technology Stack

| Component | Version |
|-----------|---------|
| **Unity** | 6 (6000.0.60f1) |
| **Entities** | 1.4.3 |
| **Burst Compiler** | 1.8.25 |
| **Collections** | 2.6.3 |
| **Mathematics** | 1.3.2 |
| **Target Platform** | Android (Meta Quest) |
| **Min API Level** | Android 12L (API 32) |
| **Tested on** | Meta Quest 2/3/Pro (Software v83.0) |

---

## 🎬 Benchmark Scenes

### 1. **OOP-People**
- Traditional MonoBehaviour-based pedestrian simulation
- **Recommended max entities:** 2,000
- **Default spawn:** 25 agents
- T-pose characters moving randomly, avoiding collisions

![Screenshot 2](Assets/Screenshots/com.lumo.citytrafficbenchmark-20260206-105232.jpg)

### 2. **OOP-Cars** *(Bigger Map)*
- Traditional GameObject-based vehicle simulation
- **Recommended max entities:** 2,000
- Larger urban environment for vehicle movement

![Screenshot 3](Assets/Screenshots/com.lumo.citytrafficbenchmark-20260206-105259.jpg)

### 3. **DOTS-People**
- ECS-based pedestrian simulation with Burst compilation
- **Max entities:** 10,000
- Same behavior as OOP-People, massively optimized

![Screenshot 4](Assets/Screenshots/com.lumo.citytrafficbenchmark-20260206-105345.jpg)

### 4. **DOTS-Cars** *(Bigger Map)*
- ECS-based vehicle simulation
- **Max entities:** 10,000
- Same map as OOP-Cars, DOTS-optimized

![Screenshot 5](Assets/Screenshots/com.lumo.citytrafficbenchmark-20260206-105409.jpg)

**Agent Behavior:**
- Randomized movement patterns
- Collision detection and avoidance
- Direction change on obstacle encounter

---

## 📦 Installation

### Option 1: Download Pre-built APK (Recommended)

1. Go to [**Releases**](https://github.com/lumoma/VR-CITY-DOTS/releases)
2. Download the latest `.apk` file
3. Install on Meta Quest:
   ```bash
   adb install VRTrafficBenchmark_v1.0.apk
   ```

### Option 2: Build from Source

#### Prerequisites
- Unity 6 (6000.0.60f1 or later)
- Android Build Support module
- Meta Quest development setup

#### Steps
1. **Clone the repository:**
   ```bash
   git clone https://github.com/lumoma/VR-Traffic-Simulation-Benchmark.git
   cd VR-Traffic-Simulation-Benchmark
   ```

2. **Open in Unity:**
   - Launch Unity Hub
   - Open project folder

3. **Configure Build Settings:**
   - File → Build Settings
   - Platform: **Android**
   - Texture Compression: **ASTC**
   - Minimum API Level: **Android 12L (API 32)**

4. **Build:**
   - Build and Run (for Meta Quest)
   - Or build for Windows/macOS for desktop testing

> **Note:** You can adapt this project for **Pico** or **Apple Vision Pro** by changing the XR plugin in Project Settings.

---

## 🚀 Usage

### Hands-On Video
[![Hands-On Video](https://img.youtube.com/vi/7R1KaIC8_kw/maxresdefault.jpg)](https://youtu.be/7R1KaIC8_kw)

### Running Automated Benchmarks

1. **Launch the app** on your Meta Quest
2. **Select a scene** from the VR menu:
   - OOP-People (Starting Scene)
   - OOP-Cars
   - DOTS-People
   - DOTS-Cars
3. **Start Benchmark:**
   - The system automatically spawns **10 agents** initially
   - Every **5 seconds**, **+25 agents** are added
   - Stops at **200 agents** (default max for automated benchmark)
   - FPS and entity count are recorded
   - CSV saved in files

### Manual Testing Mode

- Adjust entity count **in real-time** using VR UI controls
- Test specific scenarios (e.g., 500 cars, 1,000 pedestrians)
- Monitor FPS live in the headset

> **⚠️ Performance Warning:**  
> OOP scenes are optimized for **up to 2,000 entities**.  
> DOTS scenes can handle **10,000 entities** but performance depends on hardware.

---

## 📊 Exporting Results

Benchmark data is saved as CSV files on your Meta Quest device.
You can get it with ADB Commands by using the Meta Quest Developer Hub (MQDH)

### CSV File Format
```
BenchmarkResult_<SceneName>_<Architecture>_<Timestamp>.csv
```

**Example:**
```
BenchmarkResult_DOTS-People_DOTS_2026-01-20_10-53-53.csv
BenchmarkResult_OOP-Cars_OOP_2026-01-20_10-52-43.csv
```

### ADB Commands

**Package Name:** `com.lumoma.citytrafficbenchmark`

#### 1. List CSV Files
```bash
adb shell ls -l /sdcard/Android/data/com.lumo.citytrafficbenchmark/files | grep BenchmarkResult || true
```

#### 2. Download CSV Files to Desktop
```bash
adb pull /sdcard/Android/data/com.lumo.citytrafficbenchmark/files/BenchmarkResults/. ~/Desktop/
```

#### 3. Delete CSV Files (Cleanup)
```bash
adb shell rm "/sdcard/Android/data/com.lumo.citytrafficbenchmark/files/BenchmarkResults/*.csv"
```

### Example CSV Data

| Timestamp | Scene | Architecture | Entity Count | FPS | Frame Time (ms) |
|-----------|-------|--------------|--------------|-----|-----------------|
| 10:53:53  | DOTS-People | DOTS | 500 | 72 | 13.8 |
| 10:54:03  | DOTS-People | DOTS | 1000 | 68 | 14.7 |

> **📁 Sample Data:** Check the `/SampleResults/` folder for example CSV files.

---

## ⚡ Performance Notes

### Hardware Performance Reference

| Platform | Scene | Entities | FPS |
|----------|-------|----------|-----|
| **Macbook Pro M5** | OOP-People | 1850 | ~100 FPS |
| **Macbook Pro M5** | DOTS-People | 8500 | ~100 FPS |
| **Meta Quest 3** | DOTS-People | 400 | ~72 FPS |
| **Meta Quest 3** | OOP-People | 100 | ~72 FPS |

### Recommendations

- **OOP Scenes:** Max 100 entities for stable 72+ FPS on Quest
- **DOTS Scenes:** Can handle 400 entities on Quest (Cars not because of Graphics Issues)
- **Desktop Testing:** Windows/Mac builds run significantly faster (useful for development)

---

## 📂 Project Structure

```
VR-CITY-DOTS/
├── Assets/
│   ├── Scenes/
│   │   ├── OOP-People.unity
│   │   ├── OOP-Cars.unity
│   │   ├── DOTS-People.unity
│   │   ├── DOTS-Cars.unity
│   │   └── DOTS-Subscenes/
│   ├── Scripts/
│   │   ├── OOP/
│   │   ├── DOTS/
│   │   └── Metric/
│   ├── Prefabs/
│   ├── Samples/
│   ├── ThirdPartyAssets/
│   ├── XR/
│   └── Screenshots/        # App screenshots
├── Packages/
├── SampleResults/          # Example CSV files           
├── README.md
└── LICENSE
```

---

## 🎨 Assets & Credits

This project uses the following free Unity Asset Store packages:

| Asset | Creator | Link |
|-------|---------|------|
| **Cartoon City Free** | ITHAPPY | [Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/cartoon-city-free-low-poly-city-3d-models-pack-328170) |
| **Polygon Starter Pack** | SYNTY STUDIOS | [Asset Store](https://assetstore.unity.com/packages/essentials/tutorial-projects/polygon-starter-pack-art-by-synty-156819) |
| **Low Poly Cars** | AWBMECREATIONS | [Asset Store](https://assetstore.unity.com/packages/3d/vehicles/mobile-optimize-free-low-poly-cars-327313) |

---

## 🎯 Use Case

This project was developed as part of a **Bachelor's Thesis** at the **SRH University Heidelberg** in kooperation with the **Karlsruhe Institute of Technology (KIT)**.  

The prototype serves as a performance testing tool for the **Sustainable Futures Lab (ITAS/KIT)**, supporting research in:
- Autonomous driving simulations
- Multi-agent systems in VR
- Real-time performance optimization in a urban environment

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Lumoma**  
Bachelor Thesis Project - SRH University & Karlsruhe Institute of Technology (KIT)
  
🔗 LinkedIn: [Lumoma Profile](https://www.linkedin.com/in/lukasmorawietz)

---

## 🎥 Demo Video

[![VR Traffic Benchmark Demo](https://img.youtube.com/vi/hGeLczTzLNM/maxresdefault.jpg)](https://youtu.be/hGeLczTzLNM)

*Watch the full benchmark comparison: OOP vs DOTS with 6,000+ entities on Meta Quest 3*

---

## 🤝 Contributing

While this is primarily an academic project, suggestions and improvements are welcome!

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/improvement`)
3. Commit your changes (`git commit -m 'Add improvement'`)
4. Push to the branch (`git push origin feature/improvement`)
5. Open a Pull Request

---

## 🐛 Troubleshooting

### APK Installation Fails
- Ensure Developer Mode is enabled on Meta Quest
- Check ADB connection: `adb devices`

### Low FPS in OOP Scenes
- Reduce entity count below 2,000
- Check Quest thermal throttling (let device cool down)

### CSV Files Not Found
- Verify package name: `com.lumoma.citytrafficbenchmark`
- Ensure it is a debug build
- Chec if ADB Debug Bridge is allowed
- Ensure benchmark completed at least one cycle

### Build Errors
- Verify Unity 6 installation
- Check DOTS packages are installed (Window → Package Manager)

---

## 📚 Further Reading

- [Unity DOTS Documentation](https://docs.unity3d.com/Packages/com.unity.entities@latest)
- [Meta Quest Development](https://developer.oculus.com/documentation/)
- [Bachelor Thesis - Available March 2026]

---

**⭐ If this project helps your research, please consider giving it a star!**

---

*Last Updated: February 2026*
