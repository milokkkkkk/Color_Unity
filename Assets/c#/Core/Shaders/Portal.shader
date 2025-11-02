Shader "Custom/Portal"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "black" {}
        _InactiveColour ("Inactive Colour", Color) = (1, 1, 1, 1)
        displayMask ("Display Mask", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        Cull Off

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _InactiveColour;
                float displayMask; // 1 显示纹理，否则显示占位色
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // 使用 URP 工具函数获取 0..1 的屏幕 UV，自动处理平台翻转
                float2 uv = GetNormalizedScreenSpaceUV(i.pos);

                half4 portalCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                return portalCol * displayMask + _InactiveColour * (1 - displayMask);
            }
            ENDHLSL
        }

        // 复用 URP Lit 的 ShadowCaster 以获得阴影投射（与内置管线的 Fallback 等价意图）
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
