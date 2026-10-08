Shader "PostProcessing/GaussianBlur"
{
    Properties
    {
        _Spread  ("Standard Deviation", Float) = 2.0
        _GridSize("Grid Size", Int)           = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        // Core + Blit helpers (provides Vert/Varyings and binds _BlitTexture in Blitter)
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        // XR variants
        #pragma multi_compile_instancing
        #pragma multi_compile _ _STEREO_INSTANCING_ON _STEREO_MULTIVIEW_ON

        // Do NOT redeclare _BlitTexture or samplers here; Blit.hlsl provides them.

        CBUFFER_START(UnityPerMaterial)
            float _Spread;
            uint  _GridSize;
        CBUFFER_END

        static const float E = 2.71828183f;

        float GaussianW(int x)
        {
            float sigma2 = _Spread * _Spread;
            return (1.0 / sqrt(2.0 * PI * sigma2)) * pow(E, -(x * x) / (2.0 * sigma2));
        }
        ENDHLSL

        // ---------- Horizontal ----------
        Pass
        {
            Name "Horizontal"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            float4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv    = i.texcoord;
                float2 texel = 1.0 / _ScreenParams.xy;

                int upper = (int)((_GridSize - 1) / 2);
                int lower = -upper;

                float3 col = 0;
                float  sum = 0;

                [loop]
                for (int x = lower; x <= upper; x++)
                {
                    float w = GaussianW(x);
                    sum += w;
                    float2 suv = uv + float2(texel.x * x, 0);
                    // _BlitTexture & sampler_LinearClamp are provided by Blit.hlsl
                    float3 s = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, suv).rgb;
                    col += w * s;
                }

                return float4(col / max(sum, 1e-6), 1);
            }
            ENDHLSL
        }

        // ---------- Vertical ----------
        Pass
        {
            Name "Vertical"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            float4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv    = i.texcoord;
                float2 texel = 1.0 / _ScreenParams.xy;

                int upper = (int)((_GridSize - 1) / 2);
                int lower = -upper;

                float3 col = 0;
                float  sum = 0;

                [loop]
                for (int y = lower; y <= upper; y++)
                {
                    float w = GaussianW(y);
                    sum += w;
                    float2 suv = uv + float2(0, texel.y * y);
                    float3 s = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, suv).rgb;
                    col += w * s;
                }

                return float4(col / max(sum, 1e-6), 1);
            }
            ENDHLSL
        }
    }
}
