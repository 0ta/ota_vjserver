Shader "otavj/BackGroundMeshGeometry"
{
    Properties
    {
        //_Color("Color", Color) = (1, 1, 1, 1)
        //_MainTex("Albedo", 2D) = "white" {}

        //[Space]
        //_LocalTime("Animation Time", Float) = 0.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }

            Pass
            {
                CGPROGRAM

                // Geometry Shaderを使うために4.0に設定
                #pragma target 4.0

                #pragma vertex Vertex
                #pragma geometry Geometry
                #pragma fragment Fragment

                #include "UnityCG.cginc"
                #include "UnityStandardUtils.cginc"
            #include "OtaVjNoiseCommon.hlsl"

                // Shader uniforms
                //float4 _Color;
                //sampler2D _MainTex;
                //float _LocalTime;

                // Vertex Shader → Geometry Shader に渡すための構造体
                struct Attributes
                {
                    float4 position : POSITION;
                    float3 normal : NORMAL;
                };

                // Geometry Shader → Fragment Shader に渡すための構造体
                struct Varyings
                {
                    float4 position : SV_POSITION;
                    float3 normal : NORMAL;
                    float2 addeduv : TEXCOORD0;
                };

                //
                // Vertex shader
                //
                Attributes Vertex(Attributes input)
                {
                    // Only do object space to world space transform.
                    input.position = mul(unity_ObjectToWorld, input.position);
                    input.normal = UnityObjectToWorldNormal(input.normal);
                    return input;
                }

                // Geometry Shaderの出力用の構造体を用意するための関数
                Varyings VertexOutput(float3 wpos, half3 wnrm, float3 owpos)
                {
                    Varyings o;
                    o.position = UnityWorldToClipPos(float4(wpos, 1));
                    o.normal = wnrm;
                    float4 tmpvertex = UnityWorldToClipPos(float4(owpos, 1));
                    o.addeduv = float2(tmpvertex.x / tmpvertex.w, tmpvertex.y / tmpvertex.w);
                    return o;
                }


                // 3点から法線ベクトルを求めるための関数
                float3 ConstructNormal(float3 v1, float3 v2, float3 v3)
                {
                    return normalize(cross(v2 - v1, v3 - v1));
                }

                static float delta = 1.0f / 10.0f;
                float getTickedTime(float delta)
                {
                    float time = _Time.y;
                    float garbage = fmod(time, delta);
                    return time - garbage;
                }

                float _Intensity;
                //
                // Geometry shader
                //
                [maxvertexcount(15)]
                void Geometry(triangle Attributes input[3], uint pid : SV_PrimitiveID, inout TriangleStream<Varyings> outStream)
                {
                    // Vertex inputs
                    float3 wp0 = input[0].position.xyz;
                    float3 wp1 = input[1].position.xyz;
                    float3 wp2 = input[2].position.xyz;

                    // Extrusion amount
                    //float ext = saturate(0.4 - cos(_LocalTime * UNITY_PI * 2) * 0.41);
                    //ext *= 1 + 0.3 * sin(pid * 832.37843 + _LocalTime * 88.76);

                    // Extrusion points
                    //float3 offs = ConstructNormal(wp0, wp1, wp2) * ext;

                    //float rdm = random(getTickedTime(delta));
                    //float rdm = random(pid);
                    //float z = lerp(0, 0.05, rdm);
                    
                    float zext = saturate(0.4 - cos(_Time.y * UNITY_PI * 2) * 0.41);
                    zext *= 1 + 0.3 * sin(pid * 832.37843 + _Time.y * 88.76);

                    float tz = cos(_Time.y * UNITY_PI * 2);
                    float r = random(pid);
                    //float max = lerp(0, 0.08, r);
                    float max = lerp(0, 0.08, _Intensity * 10);
                    float z = lerp(0, max, abs(tz));
                    float3 offs = ConstructNormal(wp0, wp1, wp2) * z;

                    float3 wp3 = wp0 + offs;
                    float3 wp4 = wp1 + offs;
                    float3 wp5 = wp2 + offs;

                    // Cap triangle
                    float3 wn = ConstructNormal(wp3, wp4, wp5);
                    float np = saturate(zext * 10);
                    //各頂点での法線を再計算
                    float3 wn0 = lerp(input[0].normal, wn, np);
                    float3 wn1 = lerp(input[1].normal, wn, np);
                    float3 wn2 = lerp(input[2].normal, wn, np);

                    outStream.Append(VertexOutput(wp3, wn0, wp0));
                    outStream.Append(VertexOutput(wp4, wn1, wp1));
                    outStream.Append(VertexOutput(wp5, wn2, wp2));
                    outStream.RestartStrip();

                    float3 tmpwp = float3(0, 0, 0);
                    // Side faces
                    wn = ConstructNormal(wp3, wp0, wp4);
                    outStream.Append(VertexOutput(wp3, wn, wp0));
                    outStream.Append(VertexOutput(wp0, wn, wp0));
                    outStream.Append(VertexOutput(wp4, wn, wp0));
                    outStream.Append(VertexOutput(wp1, wn, wp0));
                    outStream.RestartStrip();

                    wn = ConstructNormal(wp4, wp1, wp5);
                    outStream.Append(VertexOutput(wp4, wn, wp1));
                    outStream.Append(VertexOutput(wp1, wn, wp1));
                    outStream.Append(VertexOutput(wp5, wn, wp1));
                    outStream.Append(VertexOutput(wp2, wn, wp1));
                    outStream.RestartStrip();

                    wn = ConstructNormal(wp5, wp2, wp3);
                    outStream.Append(VertexOutput(wp5, wn, wp2));
                    outStream.Append(VertexOutput(wp2, wn, wp2));
                    outStream.Append(VertexOutput(wp3, wn, wp2));
                    outStream.Append(VertexOutput(wp0, wn, wp2));
                    outStream.RestartStrip();
                }

                sampler2D _ColorTexture;

                //
                // Fragment shader
                //
                float4 Fragment(Varyings input) : COLOR
                {
                    float4 color = tex2D(_ColorTexture, float2(input.addeduv.x * 0.5 + 0.5, (1 - (input.addeduv.y * 0.5 + 0.5))));
                    return color;

                    //float4 col = _Color;
                    //col.rgb *= input.normal;
                    //return col;
                }

                ENDCG
            }
        }
}