# UI kit

The look for every screen in the app. Art lives in `Assets/Art/UI/`; the full design with all six screens is in `Assets/Art/UI/Screens/` as reference images.

## Colour

| Token | Hex | Use |
|---|---|---|
| Soot | `#1B1713` | Text, icon buttons, dark panels |
| Brick | `#A9481F` | Primary buttons and collected stamps (white text on it) |
| Neck-ring gold | `#E0A126` | Scan frame, tap markers, highlights (soot text on it) |
| Sky teal | `#1D6F6A` | Success only |
| Limewash | `#EFE7D8` | Sheets, cards and screen backgrounds |
| Plaster grey | `#5E554B` | Labels, secondary text, empty stamps |

All text pairs above pass WCAG AA contrast. Panels for a single mural can be tinted from its `MuralData.Palette`.

## Type

| Style | Font | Size / line height | Case |
|---|---|---|---|
| Hero title | Big Shoulders Display Black | 68 / 0.88 | Upper |
| Screen title | Big Shoulders Display Black | 34 to 52 / 0.95 | Upper |
| Wall label title | Big Shoulders Display ExtraBold | 22 to 26 / 1 | Upper |
| Body | Instrument Sans Regular | 16 / 1.5 | Sentence |
| Button | Instrument Sans Bold | 17 | Sentence |
| Panel heading | Instrument Sans SemiBold | 18 | Sentence |
| Label | IBM Plex Mono Regular | 11 to 12, tracking 0.1em | Upper |

Sizes are in points on a 390 x 844 phone frame.

## Pieces

- **Primary button:** Brick fill, 56 high, radius 12, label left, arrow icon right.
- **Secondary button:** 2 px soot outline, 52 to 56 high, radius 12.
- **Icon button:** 48 round, soot fill, white icon (`Icons/`).
- **Wall label:** limewash card, radius 4, mono label line (`MURAL 04 / 5.4 x 3.5 M`) above a Big Shoulders title. It is the base of the HUD tag and the info panel.
- **Wall stamps:** one per mural. Empty = dashed ring in plaster grey (`Shapes/stamp_empty.png`), collected = brick disc with the number, tilted a few degrees (`Shapes/stamp_filled.png`).
- **Scan frame:** gold painted corners (`Shapes/scan_frame.png` or four rotated `scan_corner.png`).
- **Brush stroke:** gold underline behind hero titles (`Shapes/brush_stroke.png`).
- **Bottom sheet:** limewash, top radius 22, 24 side padding, 34 bottom padding for the home indicator.

## Screens

| File | State |
|---|---|
| `screen_start.png` | Start |
| `screen_scan.png` | Scanning |
| `screen_experience.png` | AR experience HUD |
| `screen_info.png` | Info panel |
| `screen_lost.png` | Tracking lost |
| `screen_complete.png` | Completion |

Text in [brackets] on the screens is a placeholder until guest relations confirm the content.

## Unity setup

1. Icons and Shapes: Texture Type **Sprite (2D and UI)**, Alpha Is Transparency on, no mipmaps. They are white, so tint them with the Image colour.
2. Fonts: right click each `.ttf` > Create > TextMeshPro > Font Asset (SDF). Use Big Shoulders Display Black for titles, Instrument Sans for body and buttons, IBM Plex Mono for labels.
3. Canvas Scaler: Scale With Screen Size, reference 1170 x 2532, match 0.5, and keep content inside the safe area.
