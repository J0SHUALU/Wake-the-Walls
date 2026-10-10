// Brush stroke mask shared by mural shaders that support the brush reveal.
// Expects _Dissolve, _EdgeSoftness, _StrokeScale, _StrokeAngle and _Sweep in the material's CBUFFER.
#ifndef WAKE_THE_WALLS_BRUSH_MASK_INCLUDED
#define WAKE_THE_WALLS_BRUSH_MASK_INCLUDED

float BrushHash(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

// Smooth value noise in the range 0 to 1.
float BrushValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = BrushHash(i);
    float b = BrushHash(i + float2(1, 0));
    float c = BrushHash(i + float2(0, 1));
    float d = BrushHash(i + float2(1, 1));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// How much of the layer is still painted at this canvas UV: 1 painted, 0 wiped off.
float BrushVisible(float2 canvasUV, float dissolve, float softness, float2 strokeScale, float strokeAngle, float sweepAmount)
{
    float angle = radians(strokeAngle);
    float2 dir = float2(cos(angle), sin(angle));
    float2 centred = canvasUV - 0.5;
    float2 local = float2(dot(centred, dir), dot(centred, float2(-dir.y, dir.x)));
    float2 p = local * strokeScale;

    float noise = BrushValueNoise(p) * 0.55 + BrushValueNoise(p * 2.13 + 17.7) * 0.3 + BrushValueNoise(p * 4.37 + 41.3) * 0.15;
    float mask = lerp(noise, saturate(local.x + 0.5), sweepAmount);

    float soft = max(softness, 1e-4);
    float threshold = lerp(-soft, 1.0 + soft, dissolve);
    return smoothstep(threshold - soft, threshold + soft, mask);
}

#endif
