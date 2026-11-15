# Wake the Walls

![Wake the Walls: point your phone at a campus mural and watch it come alive](docs/media/poster.png)

An AR app that finds seven murals on the ALU campus with image tracking and brings each painting to life. The murals stay flat on the wall and come alive in place: parts move, glow, breathe and drift, and Pearl, the app's guide, tells the story of each artwork and its artists. Tap the marked parts of a mural to explore it, collect a stamp for every mural you wake, and record a video of it to save to your phone.

## Links

- **Playable build (Android APK):** _itch.io link_
- **Video walkthrough:** _YouTube / Drive link_
- **DevLog:** _link_

## Team

| Member | Focus |
|---|---|
| Fawziyyah Oke (group leader) | UI flow, interaction, audio, Murals 4 and 5 |
| Joshua Moses | Art and mural layer cut-outs, mural animations, Pearl the narrator, Summit UI design, app icon, obstruction pause, design document |
| Remy Intwali | Project setup, tracking and world anchoring, app flow, Murals 6 and 7, iOS and Android builds |
| Donald Duru | Layered mural rig, shaders, VFX, record button, Murals 1 to 3 |
| Thierry Gusenga | Animation, device testing, documentation |

## The murals

Titles and artists are the official ones from ALU.

| # | Mural | Artists | What happens |
|---|---|---|---|
| 1 | Heritage of the Time to Come | Lakaz D'art (Mauritius), painted by Masha Studio | Paints itself in, her eyes follow you and blink, leaves drift down, gold light sweeps her neck rings. Tap the trees or earrings |
| 2 | African Infrastructure Civilization (flowers) | Mashakiro Bienvenue and Masha Art Studio (Rwanda) | The flower crown breathes, blooms float out, a glow sweeps the sky. Tap a flower or the collar |
| 3 | African Infrastructure Civilization (desert mosque) | Mashakiro Bienvenue and Masha Art Studio | Sand blows across the desert floor, the mosque windows glow, the plants sway. Tap the plants or the mosque |
| 4 | The Spirit of African Leadership | Samuel Daddy Ishimwe and Masha Studio (Rwanda) | The mask glows, light runs along the gold lines to each leader: Queen Nzinga, Mansa Musa, Thomas Sankara, Kwame Nkrumah, Winnie Madikizela-Mandela. Tap a face to hear their story |
| 5 | The Resurgence | Lakaz D'art (Mauritius), painted by Masha Studio | The gold rings spin around her head and the rays glow. Tap the rings or bangles |
| 6 | African Infrastructure Civilization (caravan) | Mashakiro Bienvenue and Masha Art Studio | The camel caravan walks across the dunes, the sun's beams sweep and dusk falls. Tap the caravan or the oasis |
| 7 | African Infrastructure Civilization (Great Zimbabwe) | Mashakiro Bienvenue and Masha Art Studio | Smoke rises from the huts, birds circle the tower and the doorways glow at dusk. Tap a hut or the tower |

## Features

- **Seven murals** tracked with AR image tracking and anchored in the world, so they stay put when the camera moves.
- **Pearl the narrator** introduces each mural and its artists, with the music ducking while she talks.
- **Tap markers** pulse on the parts of a mural you can tap; each mural has an info panel with its name, location and story.
- **Obstruction pause:** if a person or object stands in front of the mural, it pauses and asks for a clearer view.
- **Record button** saves a video of the AR view straight to Photos.
- **Stamps and completion screen** for every mural you wake.
- **Original soundtrack** for each mural plus menu music.

People occlusion (cutting people out in front of the murals) was tried and removed on 13 November 2026 because it pushed murals off the wall and ran phones out of memory. The work is kept on `feature/remy-people-occlusion`.

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
- **Android:** File > Build Profiles > Android > Build, and save the APK to `Builds/Android/`. Needs an ARCore supported phone on Android 11 or later.

## Docs

- [Architecture](docs/architecture.md): how the code fits together
- [Murals](docs/murals.md): the facts about each mural
- [Photo capture guide](docs/capture-guide.md)
- [Layer export guide](docs/layer-export-guide.md)
- [UI kit](docs/ui-kit.md): screens, colours and type
- [Contributing](docs/contributing.md): branches, commits and reviews
- [Credits](CREDITS.md): fonts, assets and the original soundtracks
