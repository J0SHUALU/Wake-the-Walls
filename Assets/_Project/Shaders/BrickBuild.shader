Shader "WakeTheWalls/BrickBuild"
{
    // A mural layer that lays itself down brick by brick, left to right and bottom up, as _Progress goes from 0 to 1.
    Properties
    {
        [MainTexture] _BaseMap ("Layer Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _Alpha ("Alpha", Range(0, 1)) = 1
        _Progress ("Progress", Range(0, 1)) = 1
        _BrickSize ("Brick Size (UV)", Vector) = (0.042, 0.033, 0, 0)
        _WallTop ("Top Of Wall (UV)", Range(0.1, 1)) = 0.63
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
            Name "BrickBuild"
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
                half _Progress;
                float4 _BrickSize;
                half _WallTop;
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

            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv;
                float2 size = _BrickSize.xy;
                float row = floor(uv.y / size.y);
                float shift = fmod(row, 2.0) * size.x * 0.5;
                float col = floor((uv.x + shift) / size.x);
                float2 centre = float2((col + 0.5) * size.x - shift, (row + 0.5) * size.y);

                // When this brick is laid: mostly by its place along the wall, bottom courses a little earlier.
                float start = centre.x * 0.78 + saturate(centre.y / _WallTop) * 0.12 + Hash(float2(col, row)) * 0.06;
                float laid = saturate((_Progress * 1.12 - start) / 0.06);
                float settle = 1.0 - pow(1.0 - laid, 3.0);

                // Bricks drop into place from just above their spot.
                float drop = (1.0 - settle) * size.y * 0.9;
                float2 from = float2(uv.x, uv.y - drop);
                if (floor(from.y / size.y) != row) return half4(0, 0, 0, 0);

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, from) * _BaseColor;
                color.rgb += laid * (1.0 - laid) * 0.6;
                color.a *= _Alpha * smoothstep(0.0, 0.35, laid);
                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
