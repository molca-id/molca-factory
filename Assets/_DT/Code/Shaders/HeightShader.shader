Shader "Custom/DoubleSidedHeightBasedShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 0, 1, 1) // Default to blue
        _HeightThreshold ("Height Threshold", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Cull Off                    // Render both frontface and backface
            Blend SrcAlpha OneMinusSrcAlpha // Transparency blending
            ZWrite Off                      // For transparency
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Properties
            fixed4 _BaseColor;
            float _HeightThreshold;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float worldPosY : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPosY = mul(unity_ObjectToWorld, v.vertex).y; // Get world Y position
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Check if the vertex is below the height threshold
                if (i.worldPosY <= _HeightThreshold)
                {
                    return _BaseColor; // Render with base color (blue)
                }
                else
                {
                    // Transparent for parts above the threshold
                    return fixed4(0, 0, 0, 0); // Alpha = 0 (fully transparent)
                }
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
