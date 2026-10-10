// Mural layer that can appear or disappear as if painted on or wiped off with a brush.
// Same look as MuralLayer when _Dissolve is 0: unlit, transparent, no lighting.
// Properties:
//   _BaseMap, _BaseColor, _Alpha   as in MuralLayer
//   _Dissolve       0 fully visible, 1 fully gone. Animate it with LayerReveal.
//   _EdgeSoftness   width of the soft edge between painted and bare
//   _StrokeScale    noise size along (x) and across (y) the brush strokes
//   _StrokeAngle    direction of the strokes and of the sweep, in degrees
//   _Sweep          how much the reveal travels in the stroke direction (0 random, 1 a clean wipe)
//   _EdgeColor      colour blended into the edge band, alpha is its strength
Shader "WakeTheWalls/BrushDissolve"
{
    Properties
    {
        [MainTexture] _BaseMap ("Layer Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _Alpha ("Alpha", Range(0, 1)) = 1
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _EdgeSoftness ("Edge Softness", Range(0.005, 0.3)) = 0.06
        _StrokeScale ("Stroke Scale (along, across)", Vector) = (4, 36, 0, 0)
        _StrokeAngle ("Stroke Angle", Range(-180, 180)) = 0
        _Sweep ("Sweep", Range(0, 1)) = 0.55
        _EdgeColor ("Edge Colour", Color) = (1, 1, 1, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "BrushDissolve"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Alpha;
                half _Dissolve;
                half _EdgeSoftness;
                float4 _StrokeScale;
                float _StrokeAngle;
                half _Sweep;
                half4 _EdgeColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 rawUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Smooth value noise in the range 0 to 1.
            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash(i);
                float b = Hash(i + float2(1, 0));
                float c = Hash(i + float2(0, 1));
                float d = Hash(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // Streaky noise that looks like dry brush strokes: long along the stroke, thin across it.
            float StrokeNoise(float2 p)
            {
                float n = ValueNoise(p) * 0.55;
                n += ValueNoise(p * 2.13 + 17.7) * 0.3;
                n += ValueNoise(p * 4.37 + 41.3) * 0.15;
                return n;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.rawUV = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                // Rotate the canvas so x runs along the strokes.
                float angle = radians(_StrokeAngle);
                float2 dir = float2(cos(angle), sin(angle));
                float2 centred = input.rawUV - 0.5;
                float2 local = float2(dot(centred, dir), dot(centred, float2(-dir.y, dir.x)));

                float noise = StrokeNoise(local * _StrokeScale.xy);
                float sweep = saturate(local.x + 0.5);
                float mask = lerp(noise, sweep, _Sweep);

                // Threshold moves from just below 0 to just above 1, so 0 and 1 are fully on and fully off.
                float soft = max(_EdgeSoftness, 1e-4);
                float threshold = lerp(-soft, 1.0 + soft, _Dissolve);
                float visible = smoothstep(threshold - soft, threshold + soft, mask);

                // Paint colour on the edge band only.
                float edge = 1.0 - abs(visible * 2.0 - 1.0);
                color.rgb = lerp(color.rgb, _EdgeColor.rgb, edge * _EdgeColor.a);

                color.a *= _Alpha * visible;
                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
