# Architecture

## Folders

```
Assets/
  _Project/
    Scripts/
      Core/         AppState, GameEvents, AppStateMachine, BootLoader, Tween
      Tracking/     MuralTrackingManager, TrackingLossHandler, TrackingDebugOverlay
      Murals/       MuralData, MuralLibrary, MuralExperience, Mural01Experience ...
      Rig/          LayerManifest, LayeredMuralRig, MuralLayer, LayerMotion
      Interaction/  Interactable, TapInteractor, InfoHotspot
      UI/           UIManager, UIScreen, StartScreen, ScanScreen, ARHud, InfoPanel, CompletionScreen
      Audio/        AudioManager
    Shaders/        MuralLayer, BrushDissolve, FlowWave, Painterly
    VFX/            PaintDust, PaintDrip
    Timelines/      one intro timeline per mural
    Prefabs/        one experience prefab per mural
    Scenes/         Boot, Main
    Murals/         MuralLibrary and MuralData assets
  Art/              exported art: Murals/MuralN/, UI/, Fonts/
  ThirdParty/       downloaded CC0 assets
```

## How a scan flows

1. `MuralTrackingManager` hears from `ARTrackedImageManager` that an image was added.
2. It looks up the image name in `MuralLibrary`, spawns `MuralData.ExperiencePrefab` on the image, calls `Initialize` then `OnFound`, and raises `GameEvents.MuralFound`.
3. `AppStateMachine` moves to `Experience`. UI and audio react to the events on their own.
4. If tracking drops for longer than the grace period, `TrackingLossHandler` calls `OnLost` and raises `MuralLost`. When the image comes back it calls `OnResume`.
5. The experience calls `Complete()` when the user is done, which raises `MuralCompleted`, and the app shows the completion screen.

Nothing holds a direct reference to another system when an event will do.

## Space and scale

- An experience prefab is built at the mural's real size, in metres.
- Origin is the centre of the mural. +X is right, +Y is up the wall, -Z points out of the wall towards the viewer.
- The tracking manager parents the prefab under the tracked image with a local rotation of (90, 0, 0), which turns AR Foundation's image plane (X and Z, normal on +Y) into the convention above. Check this with the alignment test before building on it.

## Shared numbers

| What | Value |
|---|---|
| Layer gap while still flat on the wall | 2 to 5 mm per layer |
| How far a layer can peel off the wall | up to 0.3 m |
| How far 3D content can travel | up to 1.5 m out, within the mural width plus half on each side |
| Transitions | 0.6 to 1.5 s, eased, never instant |
| Tracking loss grace period | 0.75 s |
| Layer texture max size on device | 2048 px |
| Frame rate target | 60 fps, never below 30 |

## Look

- Flat layers use the unlit `MuralLayer` shader so they match the photo exactly.
- 3D content that leaves the wall uses `Painterly`: 3 to 4 colour bands, brush noise on the band edges, a thin outline and a tint from `MuralData.Palette`.
- Things appear through `BrushDissolve`, paint dust or a scale-up from their layer. Nothing pops in.

## Code rules

- Small files with one job each. If a script passes about 150 lines, split it.
- Event driven. Avoid polling in `Update` when an event exists.
- No mural names or IDs in code. Read them from `MuralData`.
- XML doc comments on public classes and methods, written plainly.
- Namespaces follow folders: `WakeTheWalls.Core`, `WakeTheWalls.Tracking`, and so on.
