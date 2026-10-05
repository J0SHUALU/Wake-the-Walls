# Layer export guide

Each mural becomes a stack of flat layers that line up exactly with the real painting. The layers then move, so it looks like the painting itself comes alive.

## Files per mural

All in `Assets/Art/Murals/MuralN/`, all the same canvas size as `muralN_reference.jpg`:

| File | What it is |
|---|---|
| `muralN_reference.jpg` | The cropped, straight photo (tracking image) |
| `muralN_bg_filled.png` | The mural with the moving elements removed and the gaps painted in |
| `muralN_L01_element.png`, `L02` ... | One element each, on a transparent background, in its original position |
| `muralN_layers.json` | The manifest below |

Keep it to 2 to 4 layers per mural. A few strong moving parts look better than many small ones, and it keeps the art workload realistic.

## Affinity steps

1. Open the reference photo. Duplicate the pixel layer for every element.
2. On each duplicate, mask everything except the element, with a soft 1 to 2 px edge.
3. On the background copy, remove the elements and paint the gaps in (Inpainting Brush, then touch up by hand in the mural's style).
4. Export each layer as PNG with transparency, at the full canvas size, so positions stay correct.

## Manifest format

```json
{
  "mural": "Mural1",
  "canvasWidth": 2048,
  "canvasHeight": 1365,
  "background": "mural1_bg_filled",
  "layers": [
    { "file": "mural1_L01_element", "element": "element", "depthMm": 3, "order": 1, "pivotX": 0.5, "pivotY": 0.1 }
  ]
}
```

- `file`: PNG name without the extension.
- `element`: short name the code uses to find the layer.
- `depthMm`: gap in front of the wall in millimetres. Use 2 to 5, rising for layers further forward.
- `order`: draw order, higher draws on top.
- `pivotX`, `pivotY`: point the layer sways or rotates around, from 0 to 1 across the canvas, with (0, 0) at the bottom left. For a tree, put it at the base of the trunk.

## Unity import settings

- Texture Type: Default, Alpha Is Transparency on, Wrap Mode Clamp.
- Max Size 2048, compression ASTC 6x6 for iOS and Android.
- Turn off Generate Mip Maps for layers that stay flat on the wall.
