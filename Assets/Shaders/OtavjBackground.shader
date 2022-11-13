Shader "FullScreen/OtavjBackground"
{
    SubShader
    {
        Pass
        {
            Cull Off ZWrite On ZTest LEqual
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FullScreenPass
            #define OTAVJ_NOFX
            #include "OtavjBackground.hlsl"
            ENDHLSL
        }
        Pass
        {
            Cull Off ZWrite On ZTest LEqual
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FullScreenPass
            #define OTAVJ_FX0
            #include "OtavjBackground.hlsl"
            ENDHLSL
        }
        Pass
        {
            Cull Off ZWrite On ZTest LEqual
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FullScreenPass
            #define OTAVJ_FX1
            #include "OtavjBackground.hlsl"
            ENDHLSL
        }
        Pass
        {
            Cull Off ZWrite On ZTest LEqual
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FullScreenPass
            #define OTAVJ_FX2
            #include "OtavjBackground.hlsl"
            ENDHLSL
        }
        Pass
        {
            Cull Off ZWrite On ZTest LEqual
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FullScreenPass
            #define OTAVJ_FX3
            #include "OtavjBackground.hlsl"
            ENDHLSL
        }

    }
    Fallback Off
}
