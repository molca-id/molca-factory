Shader "UI/Unlit/AlwaysOnTopWithTextureAndShinyOutline"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.03
        _Shininess ("Shininess", Range(0, 1)) = 0.5 // Adjust the shininess

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Offset -1, -1
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _Shininess;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 mainTexColor = tex2D(_MainTex, i.texcoord);
                half4 color = mainTexColor * i.color;

                half4 outline = (1 - step(1 - _OutlineWidth, mainTexColor));

                // Create the precise outline by checking neighboring pixels
                float2 pixel = _MainTex_ST.xy * i.texcoord;
                half4 outlinePrecise = (tex2D(_MainTex, i.texcoord + float2(0, _OutlineWidth)) +
                                       tex2D(_MainTex, i.texcoord + float2(0, -_OutlineWidth)) +
                                       tex2D(_MainTex, i.texcoord + float2(_OutlineWidth, 0)) +
                                       tex2D(_MainTex, i.texcoord + float2(-_OutlineWidth, 0))) / 4;

                // Introduce specular highlights based on shininess value
                half3 normal = normalize(half3(0, 0, 1)); // Simple normal for 2D UI
                half3 viewDir = normalize(half3(0, 0, -1)); // Simple view direction for 2D UI
                half3 reflectDir = reflect(-viewDir, normal);
                half specular = pow(saturate(dot(reflectDir, viewDir)), _Shininess);

                // Combine the main texture color with the precise outline and specular highlight
                half4 finalColor = lerp(color, outlinePrecise * _OutlineColor, outline.a);
                finalColor.rgb += specular; // Add the specular highlight

                clip(finalColor.a - 0.01);
                return finalColor;
            }
            ENDCG
        }
    }
}
