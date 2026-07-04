# Phantom Heist – Unity Project Agent Instructions

## Tech Stack

- Unity 6.3 LTS (6000.3.0f1)
- C# scripts under `Assets/Scripts/**`
- Target platforms: Android, Windows (Editor)

## Core Game Loop

- Player starts at the **left side of 1F**, moves to the right, takes an elevator, and keeps climbing up.
- Building is represented as a **UI grid 8 (columns) x 10 (visible floors)**.
- Player has **3 heart HP**. When HP reaches 0, it is Game Over.
- There is a **timer**; when it reaches 0, it is Game Over (Time Over).
- Reaching higher floors is the main progress metric. We record the highest floor when the game ends.
- Screen rules:
  - Resolution target: 9:20 portrait.
  - Only the current floor is fully bright.
  - Floors not yet reached are blacked out.
  - Floors already passed are slightly dark.

## Current Scene / UI Structure

- Main Game scene structure:
  - `Canvas/TopUI`:
    - Character portrait (later), level, nickname, hearts, time, floor text, options.
  - `Canvas/MiddleUI`:
    - Building background image, 8x10 grid, player, enemies, items.
    - `GridPanel` with `BuildingGridUI`:
      - Manages 8x10 cells and floor brightness (unreached/current/passed).
    - `LinePanel` with `LineManagerUI`:
      - Vertical lines aligned to grid column boundaries. Used later to highlight enemy path.
    - `FloorLinePanel` with `FloorLineUI`:
      - Horizontal floor boundary lines aligned to grid row boundaries.
  - `Canvas/BottomUI`:
    - Item slots.
    - (Future) Banner ad area.

## Important Scripts (so far)

- `Assets/Scripts/Core/World/BuildingGridUI.cs`
  - Creates and manages the 8x10 cell grid UI.
  - Controls floor colors: unreached (black), current (bright), passed (dark).
  - Exposes `SetCurrentFloor(int floorIndex)` and `GetCellRectTransform(int column, int row)`.

- `Assets/Scripts/Core/World/LineManagerUI.cs`
  - Creates **vertical lines** aligned with the grid column boundaries.
  - Uses `BuildingGridUI.GetCellRectTransform()` to compute exact positions.
  - There are 9 lines (0..8) for 8 columns (including left/right walls).
  - Provides `SetLineVisible(int columnIndex, bool visible, float alpha = 1f)` for enemy highlighting.

- `Assets/Scripts/Core/World/FloorLineUI.cs`
  - Creates **horizontal lines** aligned with the grid row boundaries.
  - Uses `BuildingGridUI` cell positions to compute positions.
  - There are 11 lines (0..10) for 10 visible floors.

- `Assets/Scripts/Core/World/FloorDebugTester.cs`
  - Temporary debug utility to visually test floor brightness by changing `currentFloorIndex` in the inspector.

## Coding Guidelines

- Do NOT remove or break existing features unless explicitly asked.
- Prefer adding new methods or classes rather than heavily changing existing public APIs.
- Follow Unity’s standard C# style:
  - PascalCase for methods and public fields.
  - camelCase for private fields.
- Apply SOLID principles when designing or extending systems (SRP/LSP/ISP/OCP/DIP) to keep responsibilities clear and dependencies abstracted.
- When adding new scripts:
  - Place them in an appropriate folder (`Core/World`, `Core/UI`, `Systems`, etc.).
  - Add clear comments in English where new or modified logic is introduced.

## Safety / Scope

- Work only inside this workspace; do not touch system files.
- When modifying scripts, keep them **compilable for Unity 6.3 LTS**.
- Assume the project uses **Input System package (new)**, not the old `UnityEngine.Input` API, unless a script is explicitly using the old system.

## Future Plans (for context)

- Add TopUI HUD (hearts, timer, floor label).
- Implement Player controller, enemies, elevator behavior and their interaction with the grid.
- Introduce items and leveling system using ScriptableObject assets.
- Plan to integrate backend services (login/signup/leaderboard) using BackND (https://backnd.com/ko/). When implementing, keep client-side code decoupled via interfaces/service abstractions to allow backend provider swaps (e.g., backend SDK wrappers, async calls with error handling and retries).
