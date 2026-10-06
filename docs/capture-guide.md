# Photo capture guide

The reference photo is the tracking image, so its quality decides how well each mural tracks.

## Shooting

1. Go when the mural is evenly lit: overcast, or when no hard shadow crosses it. Avoid glare and reflections.
2. Use the iPhone main camera (1x). Do not use the ultra wide lens, it bends straight lines.
3. Stand straight in front of the centre of the mural, phone held level and parallel to the wall. Back up until the whole painted area fits with a little margin.
4. Turn off Live Photo, flash and filters. Tap to focus on the middle of the mural.
5. Take 3 or 4 shots and keep the sharpest one.
6. Measure the painted area (width and height) and write it in [murals.md](murals.md).

## Preparing the photo

1. Put the original photos in `Assets/images/` and push them. The team straightens, crops and exports them into `Assets/Art/Murals/MuralN/`.
2. The perspective is corrected so the mural edges are straight and square, then the photo is cropped exactly to the painted area you measured.
3. Colours are left as shot. No text, filters or heavy sharpening, because tracking needs the real look of the wall.
4. The result is a JPEG, quality 90, longest side 2048 px, named `muralN_reference.jpg`.

## Checking trackability

- Good: lots of detail, sharp edges and contrast spread across the whole image.
- Weak: large flat colour areas, repeating patterns, low contrast.
- Xcode shows a warning for weak reference images when you build for iOS. Note any warning in `docs/tracking-tests.md` so we know which murals need extra care on site.
