Shader "otavj/Demux"
{
    Properties
    {
        _MainTex("", 2D) = "black"{}
     }

     SubShader
     {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            //#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
            #define OTA_DEMUX_COLOR
            #include "Demux.hlsl"
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #define OTA_DEMUX_DEPTH
            #include "Demux.hlsl"
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }
}
