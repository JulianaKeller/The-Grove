Shader "Custom/URP/SkinnedOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,0.8,0,1)
        _OutlineWidth ("Outline Width", Float) = 0.04
        _Alpha ("Alpha", Range(0,1)) = 0.95
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
#if defined(UNITY_INSTANCING_ENABLED)
                UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float4 _OutlineColor;
            float _OutlineWidth;
            float _Alpha;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Object-space position to world-space
                float3 worldPos = mul(unity_ObjectToWorld, IN.positionOS).xyz;

                // Object-space normal to world-space
                float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, IN.normalOS));

                // Extrude along normal
                worldPos += worldNormal * _OutlineWidth;

                // Transform to homogeneous clip space
                OUT.positionHCS = TransformWorldToHClip(worldPos);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = _OutlineColor;
                col.a = _Alpha;
                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
