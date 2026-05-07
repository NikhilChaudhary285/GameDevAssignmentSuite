# 🎮 Unity Technical Assignment — Production-Ready Systems

<div align="center">

![Unity](https://img.shields.io/badge/Unity-2022.3.62f3-black?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS%20%7C%20Editor-blue?style=for-the-badge)
![Architecture](https://img.shields.io/badge/Architecture-SOLID%20%7C%20Clean%20Code-orange?style=for-the-badge)

**A complete Unity project demonstrating three production-level systems:
Algorithm Design · Mobile UI · Networked Image Management**

[Features](#-features-at-a-glance) · [Architecture](#-architecture-overview) · [Setup](#-getting-started) · [Tasks](#-task-1--coin-change-algorithm) · [Design Patterns](#-design-patterns-used) · [Screenshots](#-screenshots--demo)

</div>

\---

## 📋 Table of Contents

* [Project Overview](#-project-overview)
* [Features at a Glance](#-features-at-a-glance)
* [Tech Stack](#-tech-stack)
* [Getting Started](#-getting-started)
* [Project Structure](#-project-structure)
* [Task 1 — Coin Change Algorithm](#-task-1--coin-change-algorithm)
* [Task 2 — Touch Scroll Menu](#-task-2--touch-scroll-menu)
* [Task 3 — Image Download System](#-task-3--image-download-system)
* [Architecture Overview](#-architecture-overview)
* [Design Patterns Used](#-design-patterns-used)
* [SOLID Principles](#-solid-principles-applied)
* [Performance Considerations](#-performance-considerations)
* [Edge Cases Handled](#-edge-cases-handled)
* [Testing Guide](#-testing-guide)
* [Known Limitations \& Future Improvements](#%EF%B8%8F-known-limitations--future-improvements)
* [Author](#%E2%80%8D-author)

\---

## 🧩 Project Overview

This repository is a **Unity technical assignment** built to production standards. It implements three independent but architecturally consistent systems — each following **SOLID principles**, **clean separation of concerns**, and **industry-standard design patterns**.

The project was built **from scratch in a single sprint** demonstrating the ability to:

* Design and implement efficient algorithms
* Build responsive mobile UI systems
* Create reusable, decoupled service layers
* Write maintainable, interview-ready code

> \\\\\\\\\\\\\\\*\\\\\\\\\\\\\\\*Every system in this project is designed to be plug-and-play reusable in any Unity project.\\\\\\\\\\\\\\\*\\\\\\\\\\\\\\\*

\---

## ✨ Features at a Glance

|Feature|Description|
|-|-|
|🧮 **Coin Change DP**|Combination-counting dynamic programming — counts all unique ways to make change|
|📱 **Swipe-Snap Menu**|Mobile-grade horizontal scroll with snap, scale animation, pagination dots|
|🌐 **Image Downloader**|Concurrent download manager with 3-tier cache and 10-second override|
|🏗️ **Clean Architecture**|SOLID, MVC, separation of concerns throughout|
|♻️ **Reusable Systems**|Every system is independently importable via `.unitypackage`|
|🧪 **Test Suite**|Built-in test runner with 12+ test cases and live UI reporting|
|💾 **Disk Cache**|7-day file-system cache with MD5-hashed filenames and expiry metadata|
|🔒 **Concurrency Control**|Max 3 simultaneous downloads with request coalescing and override|
|📐 **Aspect Ratio Support**|Scales gracefully from 9:16 (iPhone) to 3:4 (iPad) in portrait mode|

\---

## 🛠 Tech Stack

```
Engine          Unity 2022.3.62f3 LTS (96770f904ca7)
Language        C# (.NET Standard 2.1)
UI System       Unity UGUI + TextMeshPro
Networking      UnityWebRequest (coroutine-based)
Caching         System.IO + JsonUtility (no third-party dependencies)
Architecture    SOLID + MVC + Repository + Strategy patterns
Platforms       Android, iOS, Unity Editor
Orientation     Portrait (9:16 → 3:4 aspect ratios)
```

\---

## 🚀 Getting Started

### Prerequisites

* Unity **2022.3.62f3 LTS** (exact version recommended)
* TextMeshPro package (auto-prompted on first open)
* Git LFS (if cloning with large assets)

### Installation

```bash
# Clone the repository
git clone https://github.com/NikhilChaudhary285/GameDevAssignmentSuite.git

# Open in Unity Hub
# File → Open Project → select the cloned folder
```

### First Run

1. Open **Unity Hub** → **Add project from disk** → select cloned folder
2. Unity will auto-compile — wait for it to complete
3. When prompted, click **"Import TMP Essential Resources"**
4. Navigate to `Assets/\\\\\\\\\\\\\\\_Project/Scenes/`
5. Open any of the three scenes to explore each system

### Scenes

|Scene|Path|Description|
|-|-|-|
|`CoinChangeTest`|`\\\\\\\\\\\\\\\_Project/Scenes/CoinChangeTest`|Algorithm + test runner UI|
|`ScrollMenuScene`|`\\\\\\\\\\\\\\\_Project/Scenes/ScrollMenuScene`|Level select swipe menu|
|`ImageDownloaderScene`|`\\\\\\\\\\\\\\\_Project/Scenes/ImageDownloaderScene`|Image download grid demo|

\---

## 📁 Project Structure

```
Assets/
└── \\\\\\\\\\\\\\\_Project/                          ← All project code (underscore = top of list)
    ├── Scripts/
    │   ├── CoinChange/
    │   │   ├── Core/
    │   │   │   ├── ICoinChangeService.cs      ← Interface (DIP)
    │   │   │   ├── CoinChangeService.cs       ← Pure C# DP algorithm
    │   │   │   └── CoinChangeController.cs    ← MonoBehaviour UI bridge
    │   │   └── Tests/
    │   │       ├── CoinChangeTestCase.cs      ← Test data model
    │   │       └── CoinChangeTestRunner.cs    ← Test execution engine
    │   │
    │   ├── ScrollMenu/
    │   │   ├── Core/
    │   │   │   ├── IScrollMenuController.cs   ← Navigation contract
    │   │   │   ├── ScrollMenuController.cs    ← Pure C# state machine
    │   │   │   ├── LevelData.cs               ← Data model (no Unity dep)
    │   │   │   └── LevelDataProvider.cs       ← Data source (swappable)
    │   │   └── UI/
    │   │       ├── ScrollMenuView.cs          ← Main orchestrator + drag input
    │   │       ├── LevelCard.cs               ← Single card renderer
    │   │       ├── CardScaleAnimator.cs        ← Scale/alpha animation strategy
    │   │       └── PaginationDots.cs          ← Dot indicator controller
    │   │
    │   ├── ImageDownloader/
    │   │   ├── Core/
    │   │   │   ├── IImageDownloader.cs        ← Public contract
    │   │   │   ├── ImageDownloader.cs         ← Singleton orchestrator
    │   │   │   └── DownloadRequest.cs         ← Request model + coalescing
    │   │   ├── Queue/
    │   │   │   └── DownloadQueue.cs           ← Concurrency + override logic
    │   │   ├── Cache/
    │   │   │   ├── ICacheManager.cs           ← Cache contract
    │   │   │   ├── CacheManager.cs            ← Memory + disk implementation
    │   │   │   └── CacheMetadata.cs           ← Expiry metadata models
    │   │   ├── Handler/
    │   │   │   └── RequestHandler.cs          ← HTTP download + texture decode
    │   │   └── UI/
    │   │       ├── WebImage.cs                ← Drop-in image component
    │   │       └── ImageDownloaderTestUI.cs   ← Test scene controller
    │   │
    │   └── Common/
    │       ├── Interfaces/
    │       │   ├── IInitializable.cs
    │       │   ├── IDisposableService.cs
    │       │   └── IService.cs
    │       ├── Patterns/
    │       │   └── SingletonMonoBehaviour.cs  ← Generic thread-safe singleton
    │       └── OperationResult.cs             ← Result monad (no exceptions)
    │
    ├── Scenes/
    │   ├── CoinChangeTest.unity
    │   ├── ScrollMenuScene.unity
    │   └── ImageDownloaderScene.unity
    │
    ├── Prefabs/
    │   ├── UI/
    │   │   ├── LevelCardPrefab.prefab
    │   │   └── DotPrefab.prefab
    │   └── ImageDownloader/
    │       └── ImageCardPrefab.prefab
    │
    └── Resources/
        └── Fallback/                          ← Fallback textures for WebImage
```

### Assembly Definitions

Each module has its own `.asmdef` enforcing hard architectural boundaries:

```
Common.asmdef           ← Base interfaces and utilities
CoinChange.asmdef       ← references Common
ScrollMenu.asmdef       ← references Common
ImageDownloader.asmdef  ← references Common
```

> \\\\\\\\\\\\\\\*\\\\\\\\\\\\\\\*Why Assembly Definitions?\\\\\\\\\\\\\\\*\\\\\\\\\\\\\\\* They prevent accidental cross-module dependencies. `ScrollMenu` cannot accidentally import `ImageDownloader` internals. Compile-time enforcement of architecture.

\---

## 🧮 Task 1 — Coin Change Algorithm

### Problem Statement

Given a cash counter with unlimited notes of given denominations, find **how many distinct ways** you can make change for a given amount. Order does **not** matter — `\\\\\\\\\\\\\\\[1,5]` and `\\\\\\\\\\\\\\\[5,1]` count as one way.

```csharp
public int GetChange(int\\\\\\\\\\\\\\\[] notes, int amount)
```

### Algorithm — Bottom-Up Dynamic Programming

This is the classic **Coin Change 2 / Unbounded Knapsack** problem solved with combination-counting DP.

```
dp\\\\\\\\\\\\\\\[0] = 1          ← One way to make 0: use nothing
dp\\\\\\\\\\\\\\\[i] = 0          ← All other amounts start at 0

For each coin denomination:
    For amount = coin → target:
        dp\\\\\\\\\\\\\\\[amount] += dp\\\\\\\\\\\\\\\[amount - coin]
```

**Time Complexity:** `O(n × amount)` where n = number of denominations
**Space Complexity:** `O(amount)` — single 1D array

#### Why Loop Order Matters (Key Interview Point)

```
Outer = COINS, Inner = AMOUNTS  →  COMBINATIONS  ✅ (our requirement)
Outer = AMOUNTS, Inner = COINS  →  PERMUTATIONS  ❌ (counts \\\\\\\\\\\\\\\[1,5] and \\\\\\\\\\\\\\\[5,1] separately)
```

#### Algorithm Trace — `notes=\\\\\\\\\\\\\\\[1,10], amount=20 → 3`

```
Init:  dp = \\\\\\\\\\\\\\\[1, 0, 0, 0, ..., 0]   (size 21)

Coin = 1:
  amt=1:  dp\\\\\\\\\\\\\\\[1]  += dp\\\\\\\\\\\\\\\[0]  →  1   (one way: 1×1)
  amt=2:  dp\\\\\\\\\\\\\\\[2]  += dp\\\\\\\\\\\\\\\[1]  →  1   (one way: 2×1)
  ...
  amt=20: dp\\\\\\\\\\\\\\\[20] += dp\\\\\\\\\\\\\\\[19] →  1   (one way: 20×1)

Coin = 10:
  amt=10: dp\\\\\\\\\\\\\\\[10] += dp\\\\\\\\\\\\\\\[0]  →  1+1=2  (10×1  OR  1×10)
  amt=11: dp\\\\\\\\\\\\\\\[11] += dp\\\\\\\\\\\\\\\[1]  →  1+1=2
  ...
  amt=20: dp\\\\\\\\\\\\\\\[20] += dp\\\\\\\\\\\\\\\[10] →  1+2=3  ✅

Result: dp\\\\\\\\\\\\\\\[20] = 3
Ways:   \\\\\\\\\\\\\\\[20×1]  |  \\\\\\\\\\\\\\\[10×1 + 1×10]  |  \\\\\\\\\\\\\\\[2×10]
```

### Verified Test Cases

|Input Notes|Amount|Expected|Result|Status|
|-|-|-|-|-|
|`\\\\\\\\\\\\\\\[1, 10]`|`20`|`3`|`3`|✅ PASS|
|`\\\\\\\\\\\\\\\[1, 5, 10]`|`20`|`9`|`9`|✅ PASS|
|`\\\\\\\\\\\\\\\[2, 5, 10]`|`100`|`66`|`66`|✅ PASS|
|`\\\\\\\\\\\\\\\[1, 5, 10]`|`0`|`1`|`1`|✅ PASS|
|`\\\\\\\\\\\\\\\[5]`|`23`|`0`|`0`|✅ PASS|
|`\\\\\\\\\\\\\\\[3, 7]`|`5`|`0`|`0`|✅ PASS|
|`\\\\\\\\\\\\\\\[1, 2]`|`4`|`3`|`3`|✅ PASS|
|`null`|`20`|`-1`|`-1`|✅ PASS|
|`\\\\\\\\\\\\\\\[1, 5]`|`-5`|`-1`|`-1`|✅ PASS|

### Key Design Decisions

```csharp
// WHY long\\\\\\\\\\\\\\\[] not int\\\\\\\\\\\\\\\[]?
// Number of combinations can exceed int.MaxValue for large inputs.
// Example: notes=\\\\\\\\\\\\\\\[1,2,3], amount=500 → astronomically large.
// We clamp to int.MaxValue on return to maintain interface contract.
long\\\\\\\\\\\\\\\[] dp = new long\\\\\\\\\\\\\\\[amount + 1];
```

### Zero Unity Dependency

`CoinChangeService` is a **pure C# class** with zero Unity dependency. You can run it in:

* Unity Editor
* NUnit / xUnit test runner
* Any .NET Standard 2.1 application
* Console app for benchmarking

\---

## 📱 Task 2 — Touch Scroll Menu

### What It Is

A **horizontal swipe-snap level selection menu** — the exact pattern used in mobile games like Angry Birds, Candy Crush, and Subway Surfers. Built from scratch using Unity UGUI without any third-party plugins.

### Features

* **Swipe** left/right to navigate cards (touch + mouse)
* **Snap** to nearest card on release (ease-out cubic animation)
* **Scale animation** — center card = 1.0×, side cards = 0.85×
* **Alpha fade** — center card = full opacity, sides = 65%
* **Pagination dots** — active dot is larger and brighter
* **Prev/Next buttons** — gray out at boundaries
* **Locked levels** — overlay with lock icon, disabled play button
* **Star ratings** — 0–3 stars per level
* **Dynamic generation** — cards spawned from `LevelData` list at runtime

### Architecture — MVC Pattern

```
LevelDataProvider  →  provides List<LevelData>
        ↓
ScrollMenuController (Pure C#)
  - Holds current page index
  - Processes swipe delta and velocity
  - Fires OnPageChanged event
        ↓ (event)
ScrollMenuView (MonoBehaviour)
  - Spawns card prefabs
  - Moves content RectTransform
  - Delegates to sub-components
        ├── CardScaleAnimator  →  lerps scale + alpha each frame
        ├── PaginationDots     →  updates dot visuals
        └── LevelCard\\\\\\\\\\\\\\\[]        →  each card renders its own LevelData
```

### Swipe Physics

```csharp
// Fast swipe detection — direction alone decides page
bool fastSwipe = Mathf.Abs(velocity) > 500f; // pixels/sec

// Slow drag — must cross 20% of card width to trigger page change
float dragFraction = accumulatedDrag / cardWidth;
bool slowSwipe = Mathf.Abs(dragFraction) > 0.2f;

// Neither → snap back to current card
```

### Snap Animation — Ease-Out Cubic

```csharp
// Feels snappy: fast start, smooth landing
// t=0.0 → position at start
// t=1.0 → position at target
// The cubic curve makes it decelerate naturally
float t = 1f - Mathf.Pow(1f - (elapsed / duration), 3f);
float newX = Mathf.Lerp(startX, targetX, t);
```

### Aspect Ratio Support

```csharp
// Card width scales proportionally to viewport width
// Capped at 500px to prevent oversized cards on tablets
\\\\\\\\\\\\\\\_cardWidth   = Mathf.Min(screenWidth \\\\\\\\\\\\\\\* 0.65f, 500f);
\\\\\\\\\\\\\\\_cardSpacing = screenWidth \\\\\\\\\\\\\\\* 0.15f;
\\\\\\\\\\\\\\\_cardStep    = \\\\\\\\\\\\\\\_cardWidth + \\\\\\\\\\\\\\\_cardSpacing;

// Result: always shows 1 full center card + 2 partial side cards
// at any aspect ratio from 9:16 (iPhone) to 3:4 (iPad)
```

|Aspect Ratio|Device|Behavior|
|-|-|-|
|`9:16`|iPhone SE, most Androids|✅ Full card + partial sides|
|`9:19.5`|iPhone 14 Pro, modern phones|✅ Scales correctly|
|`3:4`|iPad|✅ Wider cards, proportional spacing|
|`2:3`|Older phones|✅ No layout breaks|

\---

## 🌐 Task 3 — Image Download System

### What It Is

A **drop-in, reusable image management system** for Unity. Attach `WebImage.cs` to any UI Image, set a URL, and the system handles everything — concurrency, caching, fallback, and memory management.

### System Flow

```
WebImage.SetUrl(url)
        │
        ▼
ImageDownloader.Instance.RequestImage(url, callback, useMemoryCache)
        │
        ├─ Tier 1: Memory Cache ──────────────── Hit? → callback(texture) instantly
        │          Dictionary<url, Texture2D>
        │
        ├─ Tier 2: Disk Cache ────────────────── Hit? → load PNG → callback (\\\\\\\\\\\\\\\~5ms)
        │          persistentDataPath/ImageCache/
        │          {MD5(url)}.png + cache\\\\\\\\\\\\\\\_index.json
        │
        └─ Tier 3: Network ───────────────────── DownloadQueue → RequestHandler
                   UnityWebRequest                → Texture2D (RGBA32)
                                                  → Save to disk
                                                  → Store in memory
                                                  → callback(texture)
```

### Concurrency System

```
Queue State at any moment:

  ACTIVE SLOTS (max 3):   \\\\\\\\\\\\\\\[Download A] \\\\\\\\\\\\\\\[Download B] \\\\\\\\\\\\\\\[Download C]
  PENDING QUEUE:          \\\\\\\\\\\\\\\[D] → \\\\\\\\\\\\\\\[E] → \\\\\\\\\\\\\\\[F] → \\\\\\\\\\\\\\\[G] ...

  When A completes:       \\\\\\\\\\\\\\\[Download B] \\\\\\\\\\\\\\\[Download C] \\\\\\\\\\\\\\\[Download D]
  Pending count:          \\\\\\\\\\\\\\\[E] → \\\\\\\\\\\\\\\[F] → \\\\\\\\\\\\\\\[G] ...
```

#### 10-Second Override Rule

```
Priority Rules (in order of importance):
  1. No request waits more than 10 seconds
  2. No more than 3 concurrent downloads
  Rule 1 beats Rule 2.

Override mechanism:
  If oldest\\\\\\\\\\\\\\\_pending.WaitTime > 10s:
    → If free slot exists: promote normally
    → If no free slot: ABORT oldest active download
                       → notify its WebImage (shows fallback)
                       → give freed slot to long-waiting request
```

#### Request Coalescing

```
WebImage\\\\\\\\\\\\\\\_A requests: "https://cdn.example.com/hero.png"
WebImage\\\\\\\\\\\\\\\_B requests: "https://cdn.example.com/hero.png"  ← same URL!

WITHOUT coalescing: 2 HTTP requests, 2 downloads, race conditions
WITH coalescing:    1 HTTP request, both callbacks fire on completion
                    Zero duplicate downloads regardless of timing
```

### Cache System

#### Memory Cache

```
Type:     Dictionary<string url, Texture2D texture>
Speed:    O(1) lookup — instant
Lifetime: Current app session (cleared on app restart)
Control:  Per-WebImage boolean — \\\\\\\\\\\\\\\_useMemoryCache
```

#### Disk Cache

```
Location: Application.persistentDataPath/ImageCache/
Format:   {MD5(url)}.png  +  cache\\\\\\\\\\\\\\\_index.json
Expiry:   7 days (configurable via Inspector)
On load:  Checks expiry → loads bytes → creates Texture2D → promotes to memory
```

#### Why MD5 for filenames?

```
URL:  "https://cdn.example.com/images/hero\\\\\\\\\\\\\\\_portrait\\\\\\\\\\\\\\\_v2.png?token=abc123"
MD5:  "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4"
File: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4.png"

Reason: URLs contain characters invalid in filenames (/, ?, =, \\\\\\\\\\\\\\\&).
        MD5 gives a fixed-length, filesystem-safe, collision-resistant name.
```

### Alpha Image Support

```csharp
// WHY TextureFormat.RGBA32 and not RGB24 or DXT:
//
// RGB24   → No alpha channel. PNG transparency is LOST.
// DXT1/5  → Compressed, but not supported on all mobile GPUs.
// RGBA32  → Full alpha support, universal compatibility, correct for UI.

Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
texture.LoadImage(rawBytes);  // auto-resizes, preserves alpha
```

### WebImage Component

Drop `WebImage.cs` onto any UI `Image` GameObject:

```
Inspector Fields:
  \\\\\\\\\\\\\\\[URL]
    string url              → set in Inspector or call SetUrl() from code
    bool   loadOnStart      → auto-load on Start() if true

  \\\\\\\\\\\\\\\[Sprites]
    Sprite loadingSprite    → shown while downloading
    Sprite fallbackSprite   → shown if download fails

  \\\\\\\\\\\\\\\[Aspect]
    bool preserveAspect     → maintain image proportions

  \\\\\\\\\\\\\\\[Caching]
    bool useMemoryCache     → cache this image in RAM after download

  \\\\\\\\\\\\\\\[Debug]
    bool showDebugState     → verbose logging for this component
```

```csharp
// Usage from code:
webImage.SetUrl("https://example.com/image.png");
webImage.Clear();  // reset to fallback
Debug.Log(webImage.State);  // Idle / Loading / Loaded / Failed
```

### Performance Characteristics

|Scenario|Latency|Network Requests|
|-|-|-|
|Memory cache hit|`< 1ms`|0|
|Disk cache hit|`\\\\\\\\\\\\\\\~ 5ms`|0|
|Already downloading (coalesced)|`variable`|0 (shared)|
|Fresh download|`network latency`|1|
|Fresh download (queue full)|`network latency + queue wait`|1|

\---

## 🏛 Architecture Overview

### Layered Architecture

```
┌─────────────────────────────────────────────────────┐
│                   UI Layer                          │
│  CoinChangeController · ScrollMenuView · WebImage   │
│  (MonoBehaviours — thin wrappers, zero logic)       │
├─────────────────────────────────────────────────────┤
│                 Service Layer                       │
│  CoinChangeService · ScrollMenuController           │
│  ImageDownloader · DownloadQueue · RequestHandler   │
│  (Pure C# — no Unity dependency — fully testable)   │
├─────────────────────────────────────────────────────┤
│                  Data Layer                         │
│  LevelData · DownloadRequest · CacheMetadata        │
│  OperationResult<T>                                 │
│  (Plain data classes — no behavior)                 │
├─────────────────────────────────────────────────────┤
│               Infrastructure Layer                  │
│  CacheManager · SingletonMonoBehaviour<T>           │
│  (Platform-specific, swappable implementations)     │
└─────────────────────────────────────────────────────┘
```

### Dependency Graph

```
WebImage
  └── IImageDownloader
        └── ImageDownloader (Singleton)
              ├── ICacheManager
              │     └── CacheManager
              │           ├── Memory: Dictionary<string, Texture2D>
              │           └── Disk:   File.IO + JsonUtility
              ├── DownloadQueue
              │     └── List<DownloadRequest>
              └── RequestHandler
                    └── UnityWebRequest (coroutine)

ScrollMenuView
  └── IScrollMenuController
        └── ScrollMenuController (Pure C#)

CoinChangeController
  └── ICoinChangeService
        └── CoinChangeService (Pure C#)
```

\---

## 🎨 Design Patterns Used

|Pattern|Where Used|Why|
|-|-|-|
|**Singleton**|`ImageDownloader`|One global download manager, persists across scenes|
|**Strategy**|`CardScaleAnimator`|Swap animation style without touching menu logic|
|**Observer / Event**|`IScrollMenuController.OnPageChanged`|Decouples navigation from visual updates|
|**Repository**|`LevelDataProvider`|Swap data source (JSON, server, ScriptableObject) without touching UI|
|**Result Monad**|`OperationResult<T>`|No exceptions for expected failures (cache miss, network error)|
|**Factory**|`ScrollMenuView.SpawnCards()`|Creates card instances from data without callers knowing concrete type|
|**Coalescing**|`DownloadQueue.Enqueue()`|Merges duplicate URL requests into single download|
|**MVC**|Scroll Menu system|Model=LevelData, View=ScrollMenuView, Controller=ScrollMenuController|
|**Template Method**|`SingletonMonoBehaviour<T>`|Base singleton behavior, subclass provides `OnAwake()`|
|**Composition Root**|`Awake()` in controllers|All dependencies assembled in one place|

\---

## ✅ SOLID Principles Applied

### Single Responsibility Principle

```
CoinChangeService     → only computes coin change
CacheManager          → only manages cache (not downloads)
DownloadQueue         → only manages concurrency (not HTTP)
RequestHandler        → only makes HTTP requests (not caching)
WebImage              → only manages one UI Image display
PaginationDots        → only renders dots (not navigation)
LevelCard             → only renders one card's visual state
```

### Open/Closed Principle

```
Add new cache tier (CDN)?      → New ICacheManager implementation. Zero changes to ImageDownloader.
Add new animation style?       → New CardScaleAnimator. Zero changes to ScrollMenuView.
Add new coin change algorithm? → New ICoinChangeService impl. Zero changes to UI or tests.
```

### Liskov Substitution Principle

```
MockCacheManager can replace CacheManager     → same behavior from outside
MockImageDownloader can replace ImageDownloader → unit-testable WebImage
```

### Interface Segregation Principle

```
IImageDownloader → only: RequestImage, Cancel, Clear, stats (4 methods)
ICacheManager    → only: memory ops + disk ops + maintenance (8 methods)
IScrollMenuController → only: GoToNext, GoPrev, GoToIndex, drag events (6 methods)
WebImage never sees DownloadQueue, CacheManager, or RequestHandler internals.
```

### Dependency Inversion Principle

```
High-level:  WebImage, CoinChangeController, ScrollMenuView
             ↓ depend on ↓
Abstractions: IImageDownloader, ICoinChangeService, IScrollMenuController
             ↓ implemented by ↓
Low-level:   ImageDownloader, CoinChangeService, ScrollMenuController
```

\---

## ⚡ Performance Considerations

### No Main Thread Blocking

```
All file I/O      → System.IO (fast, synchronous — files are small PNGs)
All HTTP I/O      → UnityWebRequest in coroutine (never blocks Update())
Texture creation  → Main thread only (Unity requirement, unavoidable)
Cache dictionary  → Locked for thread safety
```

### Why Coroutines Over async/await

```
UnityWebRequest is NOT thread-safe.
Task.Run() + UnityWebRequest = crash.
Coroutines are Unity's sanctioned pattern for web I/O.
async/await is used only for pure C# code (no Unity objects).
```

### Memory Management

```
Textures created with makeNoLongerReadable: false
→ Keeps CPU-side copy for re-encoding to PNG (cache save)
→ Trade-off: slightly more RAM, but enables disk persistence

EncodeToPNG() called once after download
→ Normalizes JPEG/WebP to PNG (preserves alpha)
→ Cached PNG is always alpha-safe
```

### Scale Animation (CardScaleAnimator)

```csharp
// Runs in Update() — no coroutines, no allocations per frame
// Lerp approaches target asymptotically — naturally smooth stop
float newScale = Mathf.Lerp(current, target, Time.deltaTime \\\\\\\\\\\\\\\* 10f);
// Avoids GC from Vector3 allocations by using scalar lerp
\\\\\\\\\\\\\\\_cardTransforms\\\\\\\\\\\\\\\[i].localScale = Vector3.one \\\\\\\\\\\\\\\* newScale;
```

\---

## 🛡 Edge Cases Handled

### Coin Change

|Edge Case|Input|Output|Handling|
|-|-|-|-|
|Zero amount|`\\\\\\\\\\\\\\\[], 0`|`1`|Base case dp\[0]=1|
|Impossible|`\\\\\\\\\\\\\\\[3,7], 5`|`0`|dp\[5] never updated|
|Null array|`null, 20`|`-1`|Null check at entry|
|Negative amount|`\\\\\\\\\\\\\\\[1,5], -1`|`-1`|Validation at entry|
|Zero denomination|`\\\\\\\\\\\\\\\[0,5], 20`|`-1`|Per-coin validation|
|Large amount|`\\\\\\\\\\\\\\\[1,2,5], 10000`|computed|`long\\\\\\\\\\\\\\\[]` prevents overflow|
|Duplicate denoms|`\\\\\\\\\\\\\\\[2,2,5], 10`|correct|DP handles naturally|

### Image Downloader

|Edge Case|Handling|
|-|-|
|Null or empty URL|Immediate failure callback, no request created|
|Same URL requested 5× simultaneously|Request coalescing — 1 download, 5 callbacks|
|Component destroyed mid-download|`if (this == null)` check before applying texture|
|Zero bytes downloaded|Explicit check, failure callback|
|Corrupt disk cache file|`LoadImage()` returns false → delete entry, re-download|
|Corrupt cache index JSON|Try/catch → reset index, continue|
|App quit during download|`OnApplicationQuit` cleans up, no crash|
|No internet|`isNetworkError` caught, fallback shown|
|HTTP 404 / 500|`responseCode` logged, failure callback|
|Cache directory missing|Auto-created on `CacheManager` init|

### Scroll Menu

|Edge Case|Handling|
|-|-|
|Swipe on first card (can't go left)|Controller clamps index, PREV button disabled|
|Swipe on last card (can't go right)|Controller clamps index, NEXT button disabled|
|Fast swipe before animation completes|Previous coroutine stopped, new one starts|
|Viewport width = 0 on first frame|Fallback to `Screen.width`|
|Zero cards|No crash — empty list handled|
|Drag without releasing|`OnEndDrag` not called — no snap until release|

\---

## 🧪 Testing Guide

### Task 1 — Coin Change

1. Open `CoinChangeTest` scene → Press **Play**
2. Click **Run Tests** → all 9+ test cases show green ✅
3. Manual test:

   * Notes: `1,5,10` | Amount: `20` → **9 Ways**
   * Notes: `3,7` | Amount: `5` → **0 Ways**
   * Notes: `1,5,10` | Amount: `0` → **1 Way**

### Task 2 — Scroll Menu

1. Open `ScrollMenuScene` → Press **Play**
2. In Game window, test each aspect ratio: `9:16`, `3:4`, `2:3`
3. Drag cards left/right with mouse → verify snap
4. Click NEXT/PREV → verify dot updates
5. Verify locked cards (6–10) show lock overlay

### Task 3 — Image Downloader

1. Open `ImageDownloaderScene` → Press **▶ Play**
2. Click **📥 LOAD ALL** → observe queue counter shows max **3 active** downloads at a time
3. Wait for all images to load → click **🔄 RELOAD** → images appear **instantly** (memory cache hit — no network requests)
4. Press **⏹ Stop** → Press **▶ Play** again → click **📥 LOAD ALL** → images load **fast from disk** (memory cache is gone after app close, but disk cache still valid — no re-download)
5. Click **🗑 CLEAR DISK CACHE** → Press **⏹ Stop** → Press **▶ Play** → click **📥 LOAD ALL** → images **re-download from network** (both caches cleared — full download cycle restarts, then memory + disk cache rebuild automatically)
6. Test custom URL → paste any image URL into the input field → click **LOAD** → verify image appears in grid
7. Test 10-second override → set `MaxConcurrentDownloads = 1` in `ImageDownloader` Inspector → click **📥 LOAD ALL** with 5+ images → wait 10 seconds → observe Console log: `OVERRIDE: Request waited 10s — forcing slot`

\---

## ⚠️ Known Limitations \& Future Improvements

### Current Limitations

|Limitation|Description|Proposed Fix|
|-|-|-|
|No infinite scroll|Scroll menu has fixed card count|Implement circular buffer with recycled cells|
|No ETags/304|Cache invalidation is time-based only|Add HTTP ETag support for server-driven invalidation|
|Single scene downloader|ImageDownloader is DontDestroyOnLoad|Already handled — persists correctly across scenes|
|WebP not supported|Only PNG/JPEG via LoadImage|Add WebP decoder or platform-specific plugin|
|No download progress|No % progress events|Add `IProgress<float>` parameter to `RequestImage`|
|Memory cache unbounded|No max memory limit|Add LRU eviction with configurable max size|

### Planned Improvements

```
□ Virtual scroll list (recycle card GameObjects for 1000+ items)
□ Image placeholder shimmer animation while loading
□ HTTP ETag-based cache validation
□ Retry with exponential backoff on network failure
□ Download priority levels (foreground vs background)
□ Editor window for cache inspection and clearing
□ Unit test coverage via Unity Test Framework
□ Addressables integration for local asset fallback
```

\---

## 📊 Screenshots \& Demo

> \\\\\\\\\\\\\\\*Added screenshot links for demo showcase\\\\\\\\\\\\\\\*

```
| Scene | Screenshot |
|---|---|
| Coin Change Test UI | ![Coin Change Test UI](https://raw.githubusercontent.com/NikhilChaudhary285/GameDevAssignmentSuite/main/Screenshots/coin-change-ui.png) |
| Scroll Menu — 9:16 | ![Scroll Menu 9:16](https://raw.githubusercontent.com/NikhilChaudhary285/GameDevAssignmentSuite/main/Screenshots/scroll-menu-916.png) |
| Scroll Menu — 3:4 | ![Scroll Menu 3:4](https://raw.githubusercontent.com/NikhilChaudhary285/GameDevAssignmentSuite/main/Screenshots/scroll-menu-34.png) |
| Image Downloader Grid | ![Image Downloader Grid](https://raw.githubusercontent.com/NikhilChaudhary285/GameDevAssignmentSuite/main/Screenshots/image-downloader-grid.png) |
| Image Downloader Queue | ![Image Downloader Queue](https://raw.githubusercontent.com/NikhilChaudhary285/GameDevAssignmentSuite/main/Screenshots/image-downloader-queue.png) |
```

\---

## 📦 Exporting Unity Packages

### Scroll Menu Package

```
Assets → Export Package →
  ✅ \\\\\\\\\\\\\\\_Project/Scripts/ScrollMenu/
  ✅ \\\\\\\\\\\\\\\_Project/Scenes/ScrollMenuScene.unity
  ✅ \\\\\\\\\\\\\\\_Project/Prefabs/UI/LevelCardPrefab.prefab
  ✅ \\\\\\\\\\\\\\\_Project/Prefabs/UI/DotPrefab.prefab
  ✅ \\\\\\\\\\\\\\\_Project/Sprites/
→ Save as: ScrollMenu.unitypackage
```

### Image Downloader Package

```
Assets → Export Package →
  ✅ \\\\\\\\\\\\\\\_Project/Scripts/ImageDownloader/
  ✅ \\\\\\\\\\\\\\\_Project/Scripts/Common/
  ✅ \\\\\\\\\\\\\\\_Project/Scenes/ImageDownloaderScene.unity
  ✅ \\\\\\\\\\\\\\\_Project/Prefabs/ImageDownloader/
  ✅ \\\\\\\\\\\\\\\_Project/Sprites/
  ✅ \\\\\\\\\\\\\\\_Project/Resources/Fallback/ (more fallback or usable sprites, if any)
→ Save as: ImageDownloader.unitypackage
```

\---

## 🗂 Git History \& Development Phases

This project was built in structured phases:

```
Phase 1 → Project setup, folder architecture, assembly definitions, base patterns
Phase 2 → Coin Change algorithm (DP), test suite, Unity test scene
Phase 3 → Scroll Menu — swipe input, snap animation, card prefab, dots
Phase 4 → Image Download System — queue, cache, WebImage, test UI
```

\---

## 👨‍💻 Author

**Nikhil Chaudhary**

[![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/NikhilChaudhary285)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/nikhilchaudhary285/)
[![Portfolio](https://img.shields.io/badge/Portfolio-FF5722?style=for-the-badge&logo=google-chrome&logoColor=white)](https://nikhilchaudhary285.github.io/)

> \\\\\\\\\\\\\\\*Unity Developer | 2.5+ Years Experience | Mobile Games | Clean Architecture\\\\\\\\\\\\\\\*

\---

<div align="center">

**If this project helped you, consider giving it a ⭐**

*Built with ❤️*

</div>

