Shader "otavj/FloorWave4LineMesh"
{
    Properties
    {
        _Speed("Speed ",Range(0, 100)) = 1
        _Frequency("Frequency ", Range(0, 3)) = 1
        _Amplitude("Amplitude", Range(0, 1)) = 0.5
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag

           #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float _Speed;
            float _Frequency;
            float _Amplitude;

            v2f vert(appdata v)
            {
                v2f o;

                float2 factors = _Time.x * _Speed + v.vertex.xz * _Frequency;
                float2 offsetYFactors = sin(factors) * _Amplitude;
                v.vertex.y += offsetYFactors.x + offsetYFactors.y;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // ñ@ê¸Çï‚ê≥
                float2 normalOffsets = -cos(factors) * _Amplitude;
                v.normal.xyz = normalize(half3(normalOffsets.x, 1, normalOffsets.y));

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = 1;
                return col;
            }
            ENDCG
        }
    }
}