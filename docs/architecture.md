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

## Layered mural rig

The rig turns a mural's exported layers into a stack of flat quads at the mural's real size, sitting a few millimetres off the wall, so parts of the painting can move while the rest stays exactly on the paint.

### How it fits together

- `LayeredMuralRig` (Rig) reads `muralN_layers.json`, builds the filled background flush on the wall, then one `MuralLayer` per entry. Textures are matched to entries by file name. `TryGetLayer("element")` finds a layer by its manifest name.
- `MuralLayer` (Rig) is one quad on the full canvas at `Z = -depthMm / 1000`, drawn in `order`. Its `Pivot` child sits on the manifest pivot, so rotating or scaling the pivot moves the element around that point.
- `LayerMotion` (Rig) gives a layer sway and bob, `Peel` and `Return` along -Z (max 0.3 m), and an `ExtraDegrees` value that interactions add on top.
- `LayerMotionSet` (Rig) sets up `LayerMotion` for a list of elements from the Inspector and plays, pauses or resets them together.
- `LayerParallax` (Rig) slides layers apart a little as the camera moves sideways, scaled by depth. Seen straight on, the shift is zero.
- `LayerReveal` (Rig) paints a layer on or wipes it off with the brush dissolve.
- `LayeredMuralExperience` (Murals) is the base for every mural on the rig: fade in and out, pause and resume, idle motion, drifting paint dust and completion after enough different interactions. Each `MuralNNExperience` adds its own story through `OnStarted`, `OnStopped` and `OnResetState`.

### Setting up a new mural

1. Export the art as in `layer-export-guide.md`: reference, filled background, one PNG per element on the full canvas, and the manifest.
2. Create `Prefabs/MuralN/MuralNExperience.prefab` with an empty root. Add your `MuralNNExperience` class (or a class that only inherits from `LayeredMuralExperience`) and a `LayerMotionSet`.
3. Add a child called `Rig` with `LayeredMuralRig`. Assign the manifest, drag in the background and every element texture, and set the material to `Shaders/BrushDissolve.mat` (or `MuralLayer.mat` if nothing on this mural needs the brush reveal). Add `LayerParallax` to the same child if you want parallax.
4. On the root, fill in the experience: the rig, motion set and parallax, plus `PaintDust` and dust source elements if paint should drift.
5. In the `LayerMotionSet`, add one entry per element that should move. Keep sway to a few degrees, bob to a few millimetres, and give each entry a different phase.
6. For each interaction, add a child with a `BoxCollider`, an `Interactable` and one of the interaction scripts below. Set its tap area as a rectangle from 0 to 1 across the mural, and connect its `used` event to the experience's `ReportInteraction`.
7. Point `MuralN.asset` at the prefab's experience component.

### Interaction scripts (Murals)

| Script | What it does |
|---|---|
| `SwingInteraction` | Swings layers hard and lets them settle (gusts, jewellery), with an optional paint burst and sound |
| `PulseInteraction` | Swells layers open from their pivot and throws out a burst in chosen colours (flowers) |
| `GlowSweepInteraction` | Sends a band of light across one layer using a brushed copy of it (beads, gold) |
| `InfoTapInteraction` | Shows a small burst and raises `InfoRequested` with a topic id for the info hotspot. It holds no facts itself |
| `BloomTapInteraction` | Makes Mural 2's floating blooms tappable to send them home |

`TapArea` fits any of these colliders to the mural at its real size, so tap areas stay right if a mural is measured again.

### Shaders and effects

| Asset | Use it for |
|---|---|
| `MuralLayer` shader and material | Flat layers that must look exactly like their PNG |
| `BrushDissolve` shader and material | Layers that appear or disappear with a brush stroke. Animate `_Dissolve` with `LayerReveal` |
| `FlowWave` shader | Paint that moves inside a layer (leaves, water, sky, fabric) while the outline stays still. Supports the brush reveal too |
| `Painterly` shader and material | 3D content that leaves the wall. Tint it from the palette with `PaletteTint` |
| `VFX/PaintDust` | A short burst of paint flecks. Play it with `PaintBurst.Spawn`, which tints it and cleans up afterwards |
| `VFX/PaintDrip` | A few drips running down from an edge |
| `VFX/SandDrift` | Looping sand blowing sideways (Mural 3) |

`BloomBuilder` (VFX) builds simple painterly 3D flowers in code, and `DesertFloor` (Murals) brushes a painted floor out from the bottom of a mural.

### Checking a new mural

- In a scratch scene, spawn the prefab, call `Initialize` with the mural's data, then `OnFound`. Check that every layer sits exactly on the background and nothing pops in.
- Call `OnLost`, `OnResume` and `ResetExperience` in turn. The mural should fade out, carry on and go back to rest with no leftovers.
- At the wall, check the layers sit on the paint from a few spots, then tune depths, sway, peel and tap areas.

## Code rules

- Small files with one job each. If a script passes about 150 lines, split it.
- Event driven. Avoid polling in `Update` when an event exists.
- No mural names or IDs in code. Read them from `MuralData`.
- XML doc comments on public classes and methods, written plainly.
- Namespaces follow folders: `WakeTheWalls.Core`, `WakeTheWalls.Tracking`, and so on.
