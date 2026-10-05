# Photo capture guide

The reference photo is the tracking image, so its quality decides how well each mural tracks.

## Shooting

1. Go when the mural is evenly lit: overcast, or when no hard shadow crosses it. Avoid glare and reflections.
2. Use the iPhone main camera (1x). Do not use the ultra wide lens, it bends straight lines.
3. Stand straight in front of the centre of the mural, phone held level and parallel to the wall. Back up until the whole painted area fits with a little margin.
4. Turn off Live Photo, flash and filters. Tap to focus on the middle of the mural.
5. Take 3 or 4 shots and keep the sharpest one.
6. Measure the painted area (width and height) and write it in [murals.md](murals.md).

## Editing in Affinity Photo

1. Fix the perspective (Perspective tool) so the mural edges are straight and square.
2. Crop exactly to the painted area. The crop must match the area you measured.
3. Do not change colours, add text or sharpen heavily. Tracking needs the real look of the wall.
4. Export as JPEG, quality 90, longest side about 2048 px, named `muralN_reference.jpg`.
5. Save to `Assets/Art/Murals/MuralN/`.

## Checking trackability

- Good: lots of detail, sharp edges and contrast spread across the whole image.
- Weak: large flat colour areas, repeating patterns, low contrast.
- Xcode shows a warning for weak reference images when you build for iOS. Note any warning in `docs/tracking-tests.md` so we know which murals need extra care on site.
