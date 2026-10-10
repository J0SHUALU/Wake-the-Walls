# Wake the Walls

An AR app that finds five murals on the ALU campus with image tracking and brings each painting to life. Parts of the mural lift off the wall, move, grow into 3D and tell the mural's story, and the user can tap into each one to explore it.

## Team

| Member | Focus |
|---|---|
| Fawziyyah (group leader) | UI flow, interaction, audio, Murals 4 and 5 |
| Joshua | Art, mural layer cut-outs, UI design, design document |
| Remy | Project setup, tracking, app flow, builds |
| Donald | Layered mural rig, shaders, VFX, Murals 1 to 3 |

## Tech

- Unity 6000.4.7f1, URP
- AR Foundation 6.4 with ARKit (iOS, main target) and ARCore (Android)
- Input System, uGUI with TextMeshPro, Timeline

## Getting started

1. Install Git LFS, then clone: `git lfs install` and `git clone https://github.com/J0SHUALU/Wake-the-Walls`
2. Open the project in Unity Hub with exactly **6000.4.7f1** (add iOS and Android build support modules).
3. Switch to the `develop` branch and open `Assets/_Project/Scenes/Boot.unity` (the scenes land on Tue 6 Oct).

## Building

- **iOS:** needs a Mac with Xcode. File > Build Profiles > iOS > Build, open the Xcode project, pick your team under Signing and run on an iPhone.
- **Android:** File > Build Profiles > Android > Build And Run with an ARCore supported phone.

## Docs

- [Architecture](docs/architecture.md): how the code fits together
- [Murals](docs/murals.md): the facts about each mural
- [Photo capture guide](docs/capture-guide.md)
- [Layer export guide](docs/layer-export-guide.md)
- [Contributing](docs/contributing.md): branches, commits and reviews
