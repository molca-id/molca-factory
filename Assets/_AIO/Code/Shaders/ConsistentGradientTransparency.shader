Shader "Custom/GradientTransparency"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _TopFade ("Fade Top", Float) = 1.0
        _BottomFade ("Fade Bottom", Float) = 0.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 worldPos : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float _TopFade;
            float _BottomFade;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate normalized height
                float height = i.worldPos.y;
                float alpha = saturate((height - _BottomFade) / (_TopFade - _BottomFade));
                return float4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
