# 🏙️ CityBuilder

A 3D city-building simulation game built in **Unity (C#)**, developed as a university Master's thesis project.

Place zones, roads, and resource buildings on a tile-based grid. Citizens (represented as cars) move into zoned property, commute to work and shops via a pathfinding road network, and pay taxes that grow the player's economy — all wrapped in a day/night cycle, procedural terrain, and a quest system.


> 🎓 The academic thesis behind this project: [English](<docs/Master Thesis- Creating a City-Building Game.pdf>) · [Hungarian](<docs/Diplomamunka - Varosepitos-jatek-keszitese.pdf>).

---

## Features

- **Zoning & construction** — Residential, Shopping, and Industrial zones that grow (level 1 → 3) once connected to roads and reachable workplaces/shops.
- **Road network & traffic** — Roads auto-select their mesh based on neighbours (straight/curve/T-junction/crossroads); a congestion-based weighting system makes cars reroute around busy roads.
- **Pathfinding** — Both Dijkstra and A* implemented behind a swappable `IShortestPathStrategy` interface, in a dependency-free Clean Architecture module.
- **Economy** — Taxes, resources (coal, wood, electricity) produced by dedicated buildings (`Mine`, `PowerPlant`, `TreeFarm`).
- **Morality / desirability system** — Per-tile score influencing zone growth.
- **Procedural terrain** — Layered Perlin-noise heightmaps with seeded, reproducible map generation and biome-based terrain coloring.
- **Day/night cycle & in-game clock** — A central `TimeManager` event bus drives population growth, resource production, and autosaving.
- **Save/Load** — Full game state serialized to JSON, autosaved every in-game hour.
- **Quests** — ScriptableObject-based quest definitions (e.g. "Earn 100 wood", "Earn a million dollars").
- **UI Toolkit interface** — HUD, minimap, and menus built with Unity's modern UXML/USS UI system.

## Screenshots

![Gameplay screenshot](docs/gameplay.png)

---

## Why this project is worth a look

This isn't just "made a Unity game" — it's an exercise in applying real software architecture to a simulation-heavy game:

- **Clean Architecture applied inside a game engine.** The navigation/pathfinding module is split into Entities → Use Cases → Interface Adapters → Frameworks layers, so the core graph algorithms (`NavigationGraph<T>`, `AStarAlgorithm`, `DijkstraAlgorithm`) are pure C#, engine-agnostic, and independently testable — with `NavigationManager` as the single MonoBehaviour boundary into Unity.
- **Strategy pattern for pathfinding**, letting Dijkstra and A* be swapped per query behind one interface.
- **Event-driven decoupling** — a `TimeManager` action/event bus lets population growth, saving, and resource production all subscribe independently without knowing about each other or the clock's implementation.

## Tech Stack

- **Engine:** Unity 6000.5.0f1 (Unity 6)
- **Language:** C#
- **UI:** Unity UI Toolkit (UXML/USS)
- **Serialization:** `JsonUtility` (JSON save files)
- **Architecture:** Clean Architecture (navigation module), Strategy & inheritance-based polymorphism (buildings)

## Documentation

| Doc | Contents |
|---|---|
| [docs/architecture.md](docs/architecture.md) | Core manager responsibilities and message flow (Grid / UI / Build Mode) with diagram |
| Thesis PDFs | Full academic writeup ([English](<docs/Master Thesis- Creating a City-Building Game.pdf>) / [Hungarian](<docs/Diplomamunka - Varosepitos-jatek-keszitese.pdf>)) |

## Getting Started

1. Install [Unity Hub](https://unity.com/download) and Unity **6000.5.0f1** (or later Unity 6.x).
2. Clone the repo and open the project folder via Unity Hub.
3. Open the main scene under `Assets/Level/Scenes/` and press Play.

## Project Structure

Follows the [recommended Unity project structure](https://unity.com/how-to/organizing-your-project):

```
Assets/
├── Art/           Materials, models, textures
├── Audio/         Music and sound effects
├── Code/
│   ├── Scripts/   C# gameplay code (grid, build mode, navigation, economy, ...)
│   └── Shaders/   Custom shaders
├── Level/         Scenes, prefabs, and UI (UXML/USS)
docs/              Architecture docs, overview, thesis PDFs
```

## License

[MIT](LICENSE) © Henrik Rudolf Élő
