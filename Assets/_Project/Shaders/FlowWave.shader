// Mural layer whose paint gently moves: a slow rippling of the image for leaves, sky, water and fabric.
// The edges of the cut-out stay where they are; only the paint inside them moves.
// Properties:
//   _BaseMap, _BaseColor, _Alpha   as in MuralLayer
//   _FlowMask      optional greyscale mask: white moves, black stays still (default all white)
//   _Strength      how far the paint moves, in UV units (0.002 is about 4 px on a 2048 texture)
//   _Speed         how fast the waves travel
//   _Frequency     how many waves fit across the canvas
//   _Direction     direction the waves travel in (x, y)
//   _EdgeHold      distance from the cut-out edge, in UV units, over which movement fades to zero
//   _Dissolve ...  brush reveal settings, same as BrushDissolve, so LayerReveal works on this layer too
Shader "WakeTheWalls/FlowWave"
{
    Properties
    {
        [MainTexture] _BaseMap ("Layer Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _Alpha ("Alpha", Range(0, 1)) = 1
        [NoScaleOffset] _FlowMask ("Flow Mask", 2D) = "white" {}
        _Strength ("Strength", Range(0, 0.02)) = 0.002
        _Speed ("Speed", Range(0, 5)) = 0.6
        _Frequency ("Frequency", Range(1, 80)) = 18
        _Direction ("Direction", Vector) = (1, 0.3, 0, 0)
        _EdgeHold ("Edge Hold", Range(0, 0.05)) = 0.008
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _EdgeSoftness ("Brush Edge Softness", Range(0.005, 0.3)) = 0.06
        _StrokeScale ("Stroke Scale (along, across)", Vector) = (4, 36, 0, 0)
        _StrokeAngle ("Stroke Angle", Range(-180, 180)) = 0
        _Sweep ("Sweep", Range(0, 1)) = 0.55
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
            Name "FlowWave"
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
            TEXTURE2D(_FlowMask);
            SAMPLER(sampler_FlowMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Alpha;
                float _Strength;
                float _Speed;
                float _Frequency;
                float4 _Direction;
                float _EdgeHold;
                half _Dissolve;
                half _EdgeSoftness;
                float4 _StrokeScale;
                float _StrokeAngle;
                half _Sweep;
            CBUFFER_END

            #include "BrushMask.hlsl"

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

            // Lowest alpha in a small cross around uv, so movement fades out near the cut-out edge.
            half EdgeWeight(float2 uv)
            {
                if (_EdgeHold <= 0) return 1;
                float d = _EdgeHold;
                half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv + float2(d, 0)).a;
                a = min(a, SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv - float2(d, 0)).a);
                a = min(a, SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv + float2(0, d)).a);
                a = min(a, SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv - float2(0, d)).a);
                return smoothstep(0.5, 1.0, a);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv;
                float t = _Time.y * _Speed;
                float2 dir = normalize(_Direction.xy + 1e-5);
                float2 side = float2(-dir.y, dir.x);

                // Two crossing waves travelling along dir, so the paint rolls instead of sliding.
                float phase = dot(uv, dir) * _Frequency - t * 6.2831;
                float crossPhase = dot(uv, side) * _Frequency * 0.63 + t * 4.1;
                float2 offset = dir * sin(phase) + side * sin(crossPhase) * 0.6;

                half weight = SAMPLE_TEXTURE2D(_FlowMask, sampler_FlowMask, input.rawUV).r * EdgeWeight(uv);
                float2 movedUV = uv + offset * _Strength * weight;

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, movedUV) * _BaseColor;
                // Alpha always comes from the unmoved UV, so the outline of the cut-out never wobbles.
                color.a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a * _BaseColor.a;

                half visible = BrushVisible(input.rawUV, _Dissolve, _EdgeSoftness, _StrokeScale.xy, _StrokeAngle, _Sweep);
                color.a *= _Alpha * visible;
                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
