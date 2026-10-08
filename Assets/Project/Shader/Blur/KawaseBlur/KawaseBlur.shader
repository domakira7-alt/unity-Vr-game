Shader "PostProcessing/KawaseBlur"
{
    Properties
    {
        _Offset  ("Offset (tap radius)", Float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        #pragma multi_compile_instancing
        #pragma multi_compile _ _STEREO_INSTANCING_ON _STEREO_MULTIVIEW_ON

        CBUFFER_START(UnityPerMaterial)
            float _Offset; // pixel radius in source RT
        CBUFFER_END

        // Provided by Blit.hlsl binding
        // TEXTURE2D_X(_BlitTexture);
        // sampler sampler_LinearClamp;
        float4 _BlitTexture_TexelSize; // (1/w, 1/h, w, h)
        ENDHLSL

        Pass
        {
            Name "Kawase"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            float4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // base UV + pixel-sized offsets based on the SOURCE texture (half/quarter res safe)
                float2 uv    = i.texcoord;
                float2 texel = _BlitTexture_TexelSize.xy * _Offset;

                // 9-tap Kawase (center + 8 around). Very fast and good-looking at half/quarter res.
                half3 c  = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

                half3 s0 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( texel.x,  0.0)).rgb;
                half3 s1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-texel.x,  0.0)).rgb;
                half3 s2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( 0.0,  texel.y)).rgb;
                half3 s3 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( 0.0, -texel.y)).rgb;

                half3 s4 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( texel.x,  texel.y)).rgb;
                half3 s5 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-texel.x,  texel.y)).rgb;
                half3 s6 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( texel.x, -texel.y)).rgb;
                half3 s7 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-texel.x, -texel.y)).rgb;

                // simple average (fast, stable)
                half3 sum = c + s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7;
                return half4(sum * (1.0h / 9.0h), 1.0h);
            }
            ENDHLSL
        }
    }
}
