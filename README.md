# SmashFest Clone

A 2–3 day Unity clone of the trending mobile game **Smash Fest**. Tap to aim, the cannon fires a ballistic shot, and you knock every object off the platform before you run out of balls.

## Gameplay

| | |
|-|-|
https://github.com/user-attachments/assets/2d6ee734-4576-4ad1-962a-ac18885d12e0
https://github.com/user-attachments/assets/27ca8924-7bb5-4fe0-a540-a223f7d05e29

## Features

- **Tap-to-aim cannon:** a screen tap is raycast into the world, and a closed-form ballistic solver finds the launch velocity. The barrel turns smoothly toward the shot.
- **Punch-through balls:** a ball keeps a set share of its speed after hitting a breakable, so it can plough through a stack.
- **Breakables:** damage depends on impact speed, with separate thresholds for ball and ground hits. A Jar shatters; a Cube just gets knocked around.
- **Level flow:** you have a limited number of balls. An object counts as cleared when it breaks or touches the ground. The level ends once the physics world is at rest, then shows Restart or Next.
- **JSON levels:** levels live in `Assets/Levels/*.json` and are spawned at runtime from a prefab catalog.
- **Level Brush editor tool** (`Tools > SmashFest > Level Brush`): paint, rotate or erase objects on a multi-layer 3D grid, with drag painting, undo, a live scene preview and JSON import/export.
- **Pooled cannonballs** and a single `Update` loop that drives everything through `ITickable`.

## Architecture

Game logic is plain C# with no MonoBehaviours. Unity components are thin adapters around it, and assembly definitions enforce the split.

```
Assets/Scripts/
  Core/            Game.Core       Pure C# logic: Aiming, Ballistics, Breakables, Cannon, Firing, Impacts, Levels
  Runtime/         Game.Runtime    MonoBehaviour adapters, pooling, input, HUD
  Editor/          Game.Editor     Level Brush window
  Tests/EditMode/  Game.Tests      NUnit tests + hand-written fakes
```

- **Composition root:** `GameInstaller` wires everything by hand. There are no singletons.
- **Seams:** input, time, raycasting, launching, ammo and the "world at rest" check each sit behind an interface (`IPointerInputSource`, `ITimeProvider`, `IAimRaycaster`, `IBallLauncher`, `IAmmoSource`, `IWorldRestQuery`).
- **Unity usage in Core:** limited to math types (`Vector3`, `Quaternion`, `Mathf`) and `JsonUtility`.

## Tests

The project has 14 EditMode test suites with 186 tests. They cover the ballistic solver, barrel aim, cannon brain, fire gate and cooldown, breakables, punch-through, level session, progress, serialization and grid conversion.

To run them, open `Window > General > Test Runner > EditMode` and choose **Run All**.

## Level Format

```json
{
  "version": 1,
  "id": "level_01",
  "ballCount": 5,
  "platforms": [{ "position": {"x": 0, "y": 0, "z": 12}, "size": {"x": 8, "y": 0.5, "z": 6} }],
  "objects":   [{ "type": "Jar", "position": {"x": 0, "y": 3, "z": 12}, "rotation": {"x": 0, "y": 0, "z": 0} }]
}
```

## Tech

- Unity **6000.3.21f1**, URP 17.3
- Input System, Test Framework, Recorder, TextMesh Pro
- VFX: JMO Assets (War FX / Cartoon FX)

## Getting Started

1. Open the project in Unity 6000.3.21f1.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press Play and tap or click to shoot.
