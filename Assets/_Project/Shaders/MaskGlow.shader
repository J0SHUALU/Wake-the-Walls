// A band of light that travels across a mural, but only where a painted detail is.
//   _MaskMap     greyscale mask on the full mural canvas: white where the light may show
//   _GlowColor   colour of the light, alpha is its strength
//   _Position    where the band is along the sweep direction, 0 to 1 across the canvas
//   _Width       half width of the band, in canvas units
//   _Angle       direction the band travels, in degrees (0 left to right, -90 top to bottom)
//   _Alpha       overall strength, used to fade the whole effect
// Blends additively, so the paint underneath brightens instead of being covered.
Shader "WakeTheWalls/MaskGlow"
{
    Properties
    {
        _MaskMap ("Mask", 2D) = "black" {}
        [HDR] _GlowColor ("Glow Colour", Color) = (1, 0.86, 0.45, 1)
        _Position ("Position", Range(-0.5, 1.5)) = -0.5
        _Width ("Width", Range(0.01, 0.3)) = 0.06
        _Angle ("Angle", Range(-180, 180)) = -90
        _Alpha ("Alpha", Range(0, 1)) = 1
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
            #include "BrushMask.hlsl"

            TEXTURE2D(_MaskMap);
            SAMPLER(sampler_MaskMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MaskMap_ST;
                half4 _GlowColor;
                half _Position;
                half _Width;
                half _Angle;
                half _Alpha;
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
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, input.uv).r;

                float angle = radians(_Angle);
                float2 dir = float2(cos(angle), sin(angle));
                float2 centred = input.uv - 0.5;
                float along = dot(centred, dir) + 0.5;
                float across = dot(centred, float2(-dir.y, dir.x));

                // Brushy edge so the band reads as light on paint, not a hard stripe.
                float wobble = (BrushValueNoise(float2(across * 40.0, along * 4.0)) - 0.5) * _Width;
                float band = 1.0 - smoothstep(0.0, _Width, abs(along - _Position + wobble));

                return half4(_GlowColor.rgb, saturate(mask * band * _GlowColor.a * _Alpha));
            }
            ENDHLSL
        }
    }

    FallBack Off
}
