Shader "Custom/AlwaysOnTopRounded"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Corner Radius", Float) = 20
        _ImageSize ("Image Size", Vector) = (100,100,0,0) // Lebar & tinggi UI
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            // Uniforms
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
            float2 _ImageSize; // Ukuran gambar agar proporsional

            // Vertex shader input
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            // Vertex-to-fragment data
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            // Vertex Shader
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            // Fragment Shader
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // Hitung jarak dari setiap sudut (bukan sisi)
                float2 cornerDist = min(i.uv * _ImageSize, (1 - i.uv) * _ImageSize);

                // Terapkan radius hanya pada area sudut
                float dist = length(cornerDist);
                float mask = smoothstep(_Radius - 1.0, _Radius + 1.0, dist);

                // Masking alpha agar hanya sudut yang terpengaruh
                col.a *= mask;

                return col;
            }
            ENDCG
        }
    }
}
