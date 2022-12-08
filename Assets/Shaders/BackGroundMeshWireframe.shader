Shader "otavj/BackGroundMeshWireframe"
{
    Properties
    {
        _LineColor("LineColor", Color) = (0.4529,0.4529,0.492,0.8)
        //_LineColor("LineColor", Color) = (0,0,0,0.8)
        //_FillColor("FillColor", Color) = (0,0,0,0)
        _WireThickness("Wire Thickness", RANGE(0, 800)) = 100
        //[MaterialToggle] UseDiscard("Discard Fill", Float) = 1
        _UseDiscard("Discard Fill", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            // Wireframe shader based on the the following
            // http://developer.download.nvidia.com/SDK/10/direct3d/Source/SolidWireframe/Doc/SolidWireframe.pdf

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile _ USEDISCARD_ON
            #include "UnityCG.cginc"
            #include "OtaVjNoiseCommon.hlsl"

            float _WireThickness;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 projectionSpaceVertex : SV_POSITION;
                float4 worldSpacePosition : TEXCOORD1;
                float2 addeduv : TEXCOORD2;
            };

            struct g2f
            {
                float4 projectionSpaceVertex : SV_POSITION;
                float4 worldSpacePosition : TEXCOORD0;
                float4 dist : TEXCOORD1;
                float2 addeduv : TEXCOORD2;
            };

            static float delta = 1.0f / 10.0f;

            float getTickedTime(float delta)
            {
                float time = _Time.y;
                float garbage = fmod(time, delta);
                return time - garbage;
            }

            float _Intensity;

            v2g vert(appdata v)
            {
                v2g o;

                float rdm = random(getTickedTime(delta));
                float max = lerp(0, 0.07, _Intensity * 30);
                float z = lerp(0, max, step(0.5, rdm));


                o.projectionSpaceVertex = UnityObjectToClipPos(v.vertex + float3(0, 0, z));
                o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex + float3(0, 0, z));

                float4 tmpvertex = UnityObjectToClipPos(v.vertex);
                o.addeduv = float2(tmpvertex.x / tmpvertex.w, tmpvertex.y / tmpvertex.w);
                //o.addeduv = float2(o.projectionSpaceVertex.x, o.projectionSpaceVertex.y);
                //o.projectionSpaceVertex = UnityObjectToClipPos(v.vertex - float3(0 ,0, 0.1));
                //o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex - float3(0, 0, 0.1));
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
            {
                float2 p0 = i[0].projectionSpaceVertex.xy / i[0].projectionSpaceVertex.w;
                float2 p1 = i[1].projectionSpaceVertex.xy / i[1].projectionSpaceVertex.w;
                float2 p2 = i[2].projectionSpaceVertex.xy / i[2].projectionSpaceVertex.w;

                float2 edge0 = p2 - p1;
                float2 edge1 = p2 - p0;
                float2 edge2 = p1 - p0;

                // To find the distance to the opposite edge, we take the
                // formula for finding the area of a triangle Area = Base/2 * Height, 
                // and solve for the Height = (Area * 2)/Base.
                // We can get the area of a triangle by taking its cross product
                // divided by 2.  However we can avoid dividing our area/base by 2
                // since our cross product will already be double our area.
                float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
                float wireThickness = 800 - _WireThickness;

                g2f o;

                o.worldSpacePosition = i[0].worldSpacePosition;
                o.projectionSpaceVertex = i[0].projectionSpaceVertex;
                o.dist.xyz = float3((area / length(edge0)), 0.0, 0.0) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                o.addeduv = i[0].addeduv;
                triangleStream.Append(o);

                o.worldSpacePosition = i[1].worldSpacePosition;
                o.projectionSpaceVertex = i[1].projectionSpaceVertex;
                o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                o.addeduv = i[1].addeduv;
                triangleStream.Append(o);

                o.worldSpacePosition = i[2].worldSpacePosition;
                o.projectionSpaceVertex = i[2].projectionSpaceVertex;
                o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                o.addeduv = i[2].addeduv;
                triangleStream.Append(o);
            }

            uniform fixed4 _LineColor;
            float _UseDiscard;

            sampler2D _ColorTexture;

            fixed4 frag(g2f i) : SV_Target
            {
                float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];

                // Early out if we know we are not on a line segment.
                if (minDistanceToEdge > 0.5)
                {
                    //#ifdef USEDISCARD_ON
                    //discard;
                    //#else
                    //float4 color = tex2D(_ColorTexture, float2(i.addeduv.x * 0.5 + 0.5, (1 - (i.addeduv.y * 0.5 + 0.5))));
                    //return float4(color.x - 0.3, color.y - 0.2, color.z - 0.2, 0.3);
                    ////return _FillColor;
                    //#endif
                    if (_UseDiscard == 1) {
                        discard;
                    }
                    else {
                        float4 color = tex2D(_ColorTexture, float2(i.addeduv.x * 0.5 + 0.5, (1 - (i.addeduv.y * 0.5 + 0.5))));
                        return float4(color.x - 0.3, color.y - 0.2, color.z - 0.2, 0.3);
                    }
                }

                return _LineColor;
            }
            ENDCG
        }
    }
}