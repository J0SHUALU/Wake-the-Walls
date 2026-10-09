// Look for 3D content that leaves the wall, so it reads as paint rather than a rendered model.
// Shading is split into a few flat colour bands with brushy edges, with a thin dark outline.
// Properties:
//   _BaseMap / _BaseColor  texture and tint. PaletteTint sets _BaseColor from the mural's palette.
//   _ShadowColor     colour multiplied into the darkest band
//   _Bands           number of colour bands (3 or 4 suits the murals)
//   _BrushNoise      how ragged the band edges are, like dry brush strokes
//   _BrushScale      size of the brush texture on the object
//   _LightDir        fixed light direction in world space, so every piece is lit the same way
//   _FollowSceneLight 0 uses _LightDir, 1 uses the scene's main light direction instead
//   _OutlineWidth    outline thickness in metres
//   _OutlineColor    outline colour
Shader "WakeTheWalls/Painterly"
{
    Properties
    {
        [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Colour", Color) = (0.55, 0.45, 0.5, 1)
        _Bands ("Bands", Range(2, 6)) = 3
        _BrushNoise ("Brush Noise", Range(0, 0.5)) = 0.25
        _BrushScale ("Brush Scale", Range(1, 200)) = 12
        _LightDir ("Light Direction", Vector) = (-0.4, 0.8, -0.45, 0)
        _FollowSceneLight ("Follow Scene Light", Range(0, 1)) = 0
        _OutlineWidth ("Outline Width", Range(0, 0.02)) = 0.004
        _OutlineColor ("Outline Colour", Color) = (0.08, 0.06, 0.06, 1)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "RenderType" = "Opaque" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _ShadowColor;
            half _Bands;
            half _BrushNoise;
            float _BrushScale;
            float4 _LightDir;
            half _FollowSceneLight;
            float _OutlineWidth;
            half4 _OutlineColor;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "Painterly"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash3(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            // Value noise in object space, stretched along one axis so it reads as strokes.
            float StrokeNoise(float3 p)
            {
                p *= float3(1.0, 4.0, 1.0);
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float n = lerp(
                    lerp(lerp(Hash3(i), Hash3(i + float3(1, 0, 0)), f.x),
                         lerp(Hash3(i + float3(0, 1, 0)), Hash3(i + float3(1, 1, 0)), f.x), f.y),
                    lerp(lerp(Hash3(i + float3(0, 0, 1)), Hash3(i + float3(1, 0, 1)), f.x),
                         lerp(Hash3(i + float3(0, 1, 1)), Hash3(i + float3(1, 1, 1)), f.x), f.y), f.z);
                return n;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 lightDir = normalize(lerp(_LightDir.xyz, GetMainLight().direction, _FollowSceneLight));
                float3 n = normalize(input.normalWS);
                float light = saturate(dot(n, lightDir) * 0.5 + 0.5);

                // Push the band edges around with stroke noise so they look painted, not computed.
                light += (StrokeNoise(input.positionOS * _BrushScale) - 0.5) * _BrushNoise;
                float bands = max(_Bands, 2);
                float band = saturate(floor(saturate(light) * bands) / (bands - 1));

                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                half3 shade = lerp(_ShadowColor.rgb, half3(1, 1, 1), band);
                return half4(albedo.rgb * shade, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                // Push the back faces out along the normal in world space, so the outline is in metres.
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = normalize(TransformObjectToWorldNormal(input.normalOS));
                output.positionCS = TransformWorldToHClip(positionWS + normalWS * _OutlineWidth);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
