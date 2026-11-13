Shader "Hidden/WakeTheWalls/PeopleCut"
{
    Properties
    {
        [HDR] _RimColor ("Rim Colour", Color) = (1, 0.81, 0.36, 1)
        _RimStrength ("Rim Strength", Range(0, 2)) = 0.7
        _Softness ("Edge Softness", Range(0, 4)) = 1.5
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "PeopleCut"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Camera image and person outline, shared by PeopleOcclusion.cs every frame.
            TEXTURE2D(_textureY);
            SAMPLER(sampler_textureY);
            TEXTURE2D(_textureCbCr);
            SAMPLER(sampler_textureCbCr);
            TEXTURE2D(_WtwHumanStencil);
            SAMPLER(sampler_WtwHumanStencil);
            TEXTURE2D(_WtwHumanDepth);
            SAMPLER(sampler_WtwHumanDepth);

            float4x4 _WtwDisplayTransform;
            float4 _WtwStencilTexel;
            float _WtwPeopleCut;
            float _WtwFlip;
            float _WtwRim;
            float4 _WtwWallPlane;
            float _WtwHasWall;
            float _WtwWallGap;

            half4 _RimColor;
            half _RimStrength;
            half _Softness;

            static const half4x4 YCbCrToSRGB = half4x4(
                half4(1.0, 0.0000, 1.4020, -0.7010),
                half4(1.0, -0.3441, -0.7141, 0.5291),
                half4(1.0, 1.7720, 0.0000, -0.8860),
                half4(0.0, 0.0000, 0.0000, 1.0000));

            half Person(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_WtwHumanStencil, sampler_WtwHumanStencil, uv).r;
            }

            half InFront(float2 uv, float2 screen)
            {
                if (_WtwHasWall < 0.5) return 1.0;
                float human = SAMPLE_TEXTURE2D(_WtwHumanDepth, sampler_WtwHumanDepth, uv).r;
                if (human <= 0.0) return 1.0;

                float3 farPoint = ComputeWorldSpacePosition(screen, UNITY_RAW_FAR_CLIP_VALUE, UNITY_MATRIX_I_VP);
                float3 dir = normalize(farPoint - _WorldSpaceCameraPos);
                float facing = dot(_WtwWallPlane.xyz, dir);
                if (abs(facing) < 1e-4) return 1.0;
                float hit = -(dot(_WtwWallPlane.xyz, _WorldSpaceCameraPos) + _WtwWallPlane.w) / facing;
                if (hit <= 0.0) return 1.0;

                float3 forward = -UNITY_MATRIX_V[2].xyz;
                float wallDepth = hit * dot(dir, forward);
                return smoothstep(0.0, 0.1, wallDepth - human - _WtwWallGap);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                if (_WtwPeopleCut < 0.5) return scene;

                float2 screen = input.texcoord;
                if (_WtwFlip > 0.5) screen.y = 1.0 - screen.y;
                float2 uv = mul(float4(screen, 1.0, 1.0), _WtwDisplayTransform).xy;

                // The outline is low resolution, so average a few samples to soften its edge.
                float2 step = _WtwStencilTexel.xy * _Softness;
                half mask = Person(uv) * 0.4
                    + (Person(uv + float2(step.x, 0)) + Person(uv - float2(step.x, 0))
                    + Person(uv + float2(0, step.y)) + Person(uv - float2(0, step.y))) * 0.15;
                half front = InFront(uv, input.texcoord);
                half person = smoothstep(0.3, 0.7, mask) * front;

                half4 ycbcr = half4(SAMPLE_TEXTURE2D(_textureY, sampler_textureY, uv).r,
                    SAMPLE_TEXTURE2D(_textureCbCr, sampler_textureCbCr, uv).rg, 1.0);
                half3 video = mul(YCbCrToSRGB, ycbcr).rgb;
            #if !UNITY_COLORSPACE_GAMMA
                video = FastSRGBToLinear(video);
            #endif

                // People always sit in front of the wall, so put the camera image back wherever a person is.
                half3 colour = lerp(scene.rgb, video, person);

                half edge = smoothstep(0.04, 0.4, mask) * (1.0 - smoothstep(0.45, 0.95, mask));
                colour += _RimColor.rgb * edge * front * _RimStrength * _WtwRim;
                return half4(colour, scene.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
