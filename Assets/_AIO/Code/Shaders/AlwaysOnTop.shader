Shader "Custom/AlwaysOnTop"
{
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            ZTest Always
        }
    }
}