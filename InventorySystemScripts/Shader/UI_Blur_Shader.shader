Shader "Custom/SeparableGaussianBlur_URP_Original" // shader path
{
    // 
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0.0, 10.0)) = 1.0 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Cull Off ZWrite Off ZTest Always

        // Pass 0: Horizontal
        Pass
        {
            Name "HORIZONTAL"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv[5] : TEXCOORD0; // 5 sampling points
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                float2 uv = v.uv;

                o.uv[0] = uv;
                o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0 * _BlurSize, 0);
                o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0 * _BlurSize, 0);
                o.uv[3] = uv + float2(_MainTex_TexelSize.x * 2.0 * _BlurSize, 0);
                o.uv[4] = uv - float2(_MainTex_TexelSize.x * 2.0 * _BlurSize, 0);
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float weights[3] = { 0.227027, 0.316216, 0.070270 };

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[0]) * weights[0];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[1]) * weights[1];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[2]) * weights[1];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[3]) * weights[2];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[4]) * weights[2];
                
                return col; 
            }
            ENDHLSL
        }
        
        // Pass 1: Vertical
        Pass
        {
            Name "VERTICAL"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv[5] : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                float2 uv = v.uv;

                o.uv[0] = uv;
                o.uv[1] = uv + float2(0, _MainTex_TexelSize.y * 1.0 * _BlurSize);
                o.uv[2] = uv - float2(0, _MainTex_TexelSize.y * 1.0 * _BlurSize);
                o.uv[3] = uv + float2(0, _MainTex_TexelSize.y * 2.0 * _BlurSize);
                o.uv[4] = uv - float2(0, _MainTex_TexelSize.y * 2.0 * _BlurSize);
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float weights[3] = { 0.227027, 0.316216, 0.070270 };

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[0]) * weights[0];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[1]) * weights[1];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[2]) * weights[1];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[3]) * weights[2];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[4]) * weights[2];
                
                return col; 
            }
            ENDHLSL
        }
    }
}