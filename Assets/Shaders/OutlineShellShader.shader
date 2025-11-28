Shader "Custom/OutlineShell"
{
    Properties
    {
        _Color ("Outline Color", Color) = (1,1,0,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }

        // Force render on top
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Front  // flip normals for shell

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float4 _Color;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                // Expand shell outward along normals
                // IMPORTANT: because this shell mesh is pre-expanded,
                // we don't need normals here — only transform position.
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
