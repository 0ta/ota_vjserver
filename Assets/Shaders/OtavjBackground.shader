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
        //Pass
        //{
        //    Cull Off ZWrite On ZTest LEqual
        //    HLSLPROGRAM
        //    #pragma vertex Vert
        //    #pragma fragment FullScreenPass
        //    #define RCAM_FX1
        //    #include "RcamBackground.hlsl"
        //    ENDHLSL
        //}
        //Pass
        //{
        //    Cull Off ZWrite On ZTest LEqual
        //    HLSLPROGRAM
        //    #pragma vertex Vert
        //    #pragma fragment FullScreenPass
        //    #define RCAM_FX2
        //    #include "RcamBackground.hlsl"
        //    ENDHLSL
        //}
    }
    Fallback Off
}
