# Wake the Walls

An AR app that finds five murals on the ALU campus with image tracking and brings each painting to life. Parts of the mural lift off the wall, move, grow into 3D and tell the mural's story, and the user can tap into each one to explore it.

## Team

| Member | Focus |
|---|---|
| Fawziyyah (group leader) | UI flow, interaction, audio, Murals 4 and 5 |
| Joshua | Art, mural layer cut-outs, UI design, design document |
| Remy | Project setup, tracking, app flow, builds |
| Donald | Layered mural rig, shaders, VFX, Murals 1 to 3 |

## The murals

| # | Mural | What happens |
|---|---|---|
| 1 | Rooted | Paints itself in, her eyes follow you and blink, leaves fall, birds fly out, gold shines down her neck rings. Tap the trees or earrings |
| 2 | Bloom | The flower crown breathes and drops petals, blooms orbit and float out, butterflies leave the flowers, a sunset glow moves down the sky. Tap a flower or the collar |
| 3 | Desert Crossing | Sand gusts across the wall, the mosque windows glow, plants jump out in the wind. Tap the plants or the mosque |
| 4 | Faces of the Continent | The big mask glows and breathes, light runs along the gold lines, portraits lift forward in turn. Tap a portrait or a gold line |
| 5 | Orbit | The rings become 3D rings orbiting her head, cubes and flowers circle the mural. Tap the rings or bangles |

Every mural has its own soundtrack, pulsing red dots on the parts you can tap, and an info panel with its name, location and story.

## Tech

- Unity 6000.4.7f1, URP
- AR Foundation 6.4 with ARKit (iOS, main target) and ARCore (Android)
- Input System, uGUI with TextMeshPro, Timeline

## Getting started

1. Install Git LFS, then clone: `git lfs install` and `git clone https://github.com/J0SHUALU/Wake-the-Walls`
2. Open the project in Unity Hub with exactly **6000.4.7f1** (add iOS and Android build support modules).
3. Switch to the `develop` branch and open `Assets/_Project/Scenes/Boot.unity`. Press Play from Boot, which loads Main.

## Building

- **iOS:** needs a Mac with Xcode. File > Build Profiles > iOS > Build, open the Xcode project, pick your team under Signing and run on an iPhone.
- **Android:** File > Build Profiles > Android > Build And Run with an ARCore supported phone.

## Docs

- [Architecture](docs/architecture.md): how the code fits together
- [Murals](docs/murals.md): the facts about each mural
- [Photo capture guide](docs/capture-guide.md)
- [Layer export guide](docs/layer-export-guide.md)
- [UI kit](docs/ui-kit.md): screens, colours and type
- [Contributing](docs/contributing.md): branches, commits and reviews
- [Credits](CREDITS.md): fonts, assets and the original soundtracks
