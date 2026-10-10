// Light that runs through a greyscale mask laid over the whole mural, for example along gold lines.
// White in the mask can glow, black never does. Drawn additively so the paint underneath shows through.
// Properties:
//   _MaskMap       greyscale light mask on the full mural canvas (white glows)
//   _GlowColor     colour of the light, alpha is its strength
//   _Alpha         overall opacity, follows the mural's fade in and out
//   _Intensity     steady glow across the whole mask, 0 to 1
//   _Origin        where a flow starts, 0 to 1 across the canvas
//   _BandCenter    how far the flow has travelled from the origin, in canvas widths
//   _BandWidth     width of the bright front of the flow
//   _Trail         length of the fading trail behind the front
//   _Aspect        canvas width divided by height, so the flow spreads in circles
Shader "WakeTheWalls/MaskGlow"
{
    Properties
    {
        [NoScaleOffset] _MaskMap ("Light Mask", 2D) = "black" {}
        _GlowColor ("Glow Colour", Color) = (1, 0.82, 0.4, 1)
        _Alpha ("Alpha", Range(0, 1)) = 1
        _Intensity ("Steady Intensity", Range(0, 1)) = 0
        _Origin ("Flow Origin", Vector) = (0.5, 0.5, 0, 0)
        _BandCenter ("Flow Distance", Float) = -1
        _BandWidth ("Flow Front Width", Range(0.01, 0.5)) = 0.08
        _Trail ("Flow Trail", Range(0.01, 1)) = 0.25
        _Aspect ("Canvas Aspect", Float) = 1.5
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
            Name "MaskGlow"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MaskMap);
            SAMPLER(sampler_MaskMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _GlowColor;
                half _Alpha;
                half _Intensity;
                float4 _Origin;
                float _BandCenter;
                half _BandWidth;
                half _Trail;
                float _Aspect;
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
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, input.uv).r;

                // Distance from the flow origin, measured in canvas widths.
                float2 p = float2(input.uv.x, input.uv.y / _Aspect);
                float2 o = float2(_Origin.x, _Origin.y / _Aspect);
                float d = distance(p, o);
                float behind = _BandCenter - d;

                // Bright front where the flow has just arrived, fading trail where it has passed.
                half front = 1.0h - smoothstep(0.0h, _BandWidth, abs(behind));
                half trail = behind > 0.0 ? exp(-behind / _Trail) * 0.6h : 0.0h;

                // A soft shimmer so the light feels alive rather than flat.
                half shimmer = 0.85h + 0.15h * sin(_Time.y * 3.0 + input.uv.x * 40.0 + input.uv.y * 25.0);

                half glow = saturate(_Intensity + front + trail) * shimmer;
                return half4(_GlowColor.rgb, mask * glow * _GlowColor.a * _Alpha);
            }
            ENDHLSL
        }
    }
}
