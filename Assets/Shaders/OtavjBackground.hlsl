#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
//#include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

sampler2D _ColorTexture;
sampler2D _DepthTexture;
float4 _ProjectionVector;
float4x4 _InverseViewMatrix;
float _DepthOffset;

float2 _Opacity; // Background, Effect
float4 _EffectParams; // param, intensity, sin(r), cos(r)

// Linear distance to Z depth
float DistanceToDepth(float d)
{
    return d < _ProjectionParams.y ? 0 :
      (0.5 / _ZBufferParams.z * (1 / d - _ZBufferParams.w));
}

// Inversion projection into the world space
float3 DistanceToWorldPosition(float2 uv, float d)
{
    float3 p = float3((uv - 0.5) * 2, -1);
    p.xy += _ProjectionVector.xy;
    p.xy /= _ProjectionVector.zw;
    return mul(_InverseViewMatrix, float4(p * d, 1)).xyz;
}

float4 taylorInvSqrt(float4 r)
{
    return (float4) 1.79284291400159 - r * 0.85373472095314;
}

float4 mod289(float4 x)
{
    return x - floor(x / 289.0) * 289.0;
}

float3 mod289(float3 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float2 mod289(float2 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float4 permute(float4 x)
{
    return mod289(((x * 34.0) + 1.0) * x);
}
float3 permute(float3 x)
{
    return mod289(((x * 34.0) + 1.0) * x);
}

float snoise(float3 v)
{
    const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
    const float4 D = float4(0.0, 0.5, 1.0, 2.0);

// First corner
    float3 i = floor(v + dot(v, C.yyy));
    float3 x0 = v - i + dot(i, C.xxx);

// Other corners
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);

  //     x0 = x0 - 0.0 + 0.0 * C.xxx;
  //     x1 = x0 - i1  + 1.0 * C.xxx;
  //     x2 = x0 - i2  + 2.0 * C.xxx;
  //     x3 = x0 - 1.0 + 3.0 * C.xxx;
    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
    float3 x3 = x0 - D.yyy; // -1.0+3.0*C.x = -0.5 = -D.y

// Permutations
    i = mod289(i);
    float4 p = permute(permute(permute(
               i.z + float4(0.0, i1.z, i2.z, 1.0))
             + i.y + float4(0.0, i1.y, i2.y, 1.0))
             + i.x + float4(0.0, i1.x, i2.x, 1.0));

// Gradients: 7x7 points over a square, mapped onto an octahedron.
// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
    float2 ns = D.wy / 7 - D.xz;

    float4 j = p - 49.0 * floor(p / 49); //  mod(p,7*7)

    float4 x_ = floor(j / 7);
    float4 y_ = floor(j - 7.0 * x_); // mod(j,N)

    float4 x = x_ * ns.x + ns.yyyy;
    float4 y = y_ * ns.x + ns.yyyy;
    float4 h = 1.0 - abs(x) - abs(y);

    float4 b0 = float4(x.xy, y.xy);
    float4 b1 = float4(x.zw, y.zw);

  //float4 s0 = float4(lessThan(b0,0.0))*2.0 - 1.0;
  //float4 s1 = float4(lessThan(b1,0.0))*2.0 - 1.0;
    float4 s0 = floor(b0) * 2.0 + 1.0;
    float4 s1 = floor(b1) * 2.0 + 1.0;
    float4 sh = -step(h, 0.0);

    float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    float3 p0 = float3(a0.xy, h.x);
    float3 p1 = float3(a0.zw, h.y);
    float3 p2 = float3(a1.xy, h.z);
    float3 p3 = float3(a1.zw, h.w);

//Normalise gradients
    float4 norm = taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;

// Mix final noise value
    float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;
    return 42.0 * dot(m * m, float4(dot(p0, x0), dot(p1, x1),
                                  dot(p2, x2), dot(p3, x3)));
}

float snoise(float2 v)
{
    const float4 C = float4(0.211324865405187, // (3.0-sqrt(3.0))/6.0
                        0.366025403784439, // 0.5*(sqrt(3.0)-1.0)
                        -0.577350269189626, // -1.0 + 2.0 * C.x
                        0.024390243902439); // 1.0 / 41.0
    float2 i = floor(v + dot(v, C.yy));
    float2 x0 = v - i + dot(i, C.xx);
    float2 i1;
    i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    float4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;
    i = mod289(i); // Avoid truncation effects in permutation
    float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
    float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
    m = m * m;
    m = m * m;
    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;
    m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
    float3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

float random(float2 p) {
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

// float2 should be normalized float2
float random_A(float2 st) {
    return frac(sin(dot(st.xy, float2(13451111.9898, 80.233))) * 43758.5453123);
}

float random_B(float2 st)
{
    return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
}

float randomSerie(float x, float freq, float t)
{
    return step(.8, random_B(floor(x * freq) - floor(t)));
}

float noise(float2 st)
{
    float2 p = floor(st);
    return random(p);
}

float noiseA(float2 st) {
    float2 i = floor(st);
    float2 f = frac(st);

    // Four corners in 2D of a tile
    float a = random(i);
    float b = random(i + float2(1.0, 0.0));
    float c = random(i + float2(0.0, 1.0));
    float d = random(i + float2(1.0, 1.0));

    // Smooth Interpolation

    // Cubic Hermine Curve.  Same as SmoothStep()
    float2 u = f * f * (3.0 - 2.0 * f);
    // u = smoothstep(0.,1.,f);

    // Mix 4 coorners percentages
    return smoothstep(a, b, u.x) +
        (c - a) * u.y * (1.0 - u.x) +
        (d - b) * u.x * u.y;
}

// Foreground effect
float3 ForegroundEffect(float3 wpos, float2 uv, float luma)
{
//#if defined(OTAVJ_FX0)

    //float seed = sin(1 + _Time.w * random(uv));
    //float c;
    //if (0.9 < seed)
    //{
    //    c = noise(uv * 64);
    //}
    //else
    //{
    //    c = random(uv);
    //}
    //float2 st = _ScreenSize.xy * (uv + 0.5);
    //float2 pos = st * 100.0;
    ////float c = noiseA(pos);
    //return float3(c, c, c);
    
    //pattern E
    //float2 st = uv;
    //st.x *= _ScreenSize.x / _ScreenSize.y;
    //float3 color = float3(0.0, 0.0, 0.0);
    //float2 pos = float2(st * 3.);

    //float DF = 0.0;

    ////// Add a random position
    //float a = 0.0;
    //float2 vel = float2(_Time.w * .1, _Time.w * .1);
    //DF += snoise(pos + vel) * .25 + .25;

    ////// Add a random position
    //a = snoise(pos * float2(cos(_Time.w * 0.15), sin(_Time.w * 0.1)) * 0.1) * 3.1415;
    //vel = float2(cos(a), sin(a));
    //DF += snoise(pos + vel) * .25 + .25;

    //float ssv = smoothstep(.7, .75, frac(DF));
    //color = float3(ssv, ssv, ssv);

    //return float3(1.0 - color);
    
    //pattern D
    //float2 st = uv;
    //st.x *= _ScreenSize.x / _ScreenSize.y;

    //float3 color = float3(0.0, 0.0, 0.0);

    //float cols = 2.;
    //float freq = random(floor(_Time.w)) + abs(atan(_Time.w) * 0.1);
    //float t = 60. + _Time.w * (1.0 - freq) * 30.;

    //if (frac(st.y * cols * 0.5) < 0.5)
    //{
    //    t *= -1.0;
    //}

    //freq += random_B(floor(st.y));

    //float offset = 0.025;
    //color = float3(randomSerie(st.x, freq * 100., t + offset),
    //             randomSerie(st.x, freq * 100., t),
    //             randomSerie(st.x, freq * 100., t - offset));
    ////color = float3(randomSerie(st.y, freq * 100., t + offset),
    ////             randomSerie(st.y, freq * 100., t),
    ////             randomSerie(st.y, freq * 100., t - offset));
    //return color;
    
    
    //pattern C
  //  const float2 vp = float2(320.0, 200.0);
  //  float t = _Time.w;
  //  float2 p0 = (uv - 0.5) * vp;
  //  float2 hvp = vp * 0.5;
  //  float2 p1d = float2(cos(t / 98.0), sin(t / 178.0)) * hvp - p0;
  //  float2 p2d = float2(sin(-t / 124.0), cos(-t / 104.0)) * hvp - p0;
  //  float2 p3d = float2(cos(-t / 165.0), cos(t / 45.0)) * hvp - p0;
  //  float sum = 0.5 + 0.5 * (
		//cos(length(p1d) / 30.0) +
		//cos(length(p2d) / 20.0) +
		//sin(length(p3d) / 25.0) * sin(p3d.x / 20.0) * sin(p3d.y / 15.0));
  //  return tex2D(_ColorTexture, frac(uv + float2(frac(sum), frac(sum)))).rgb;
    
    
    //pattern B
    //float2 st = uv * 70.0;
    //float2 ipos = floor(st);
    //float2 fpos = frac(st);
    //float d = random_B(ipos + _Time.w);
    //return float3(d, d, d);

    //pattern A
    //float c = random_A(normalize(uv.x + uv.y + _Time.w));
    //return float3(c, c, c);






    //// Animated zebra

    //// Noise field positions
    float3 np1 = float3(wpos.y * 16, 0, _Time.y);
    float3 np2 = float3(wpos.y * 32, 0, _Time.y * 2) * 0.8;

    // Potential value
    float pt = (luma - 0.5) + snoise(np1) + snoise(np2);

    // Grayscale
    float gray = abs(pt) < _EffectParams.x + 0.02;

    // Emission
    float em = _EffectParams.y * 4;

    // Output
    return gray * (1 + em);

//#endif
//
//#if defined(RCAM_FX1)
//
//    // Marble-like pattern
//
//    // Frequency
//    float freq = lerp(2.75, 20, _EffectParams.x);
//
//    // Noise field position
//    float3 np = wpos * float3(1.2, freq, 1.2);
//    np += float3(0, -0.784, 0) * _Time.y;
//
//    // Potential value
//    float pt = 0.5 + (luma - 0.5) * 0.4 + snoise(np) * 0.7;
//
//    // Random seed
//    uint seed = (uint)(pt * 5 + _Time.y * 5) * 2;
//
//    // Color
//    float3 rgb = FastSRGBToLinear(HsvToRgb(float3(Hash(seed), 1, 1)));
//
//    // Emission
//    float em = Hash(seed + 1) < _EffectParams.y * 0.5;
//
//    // Output
//    return rgb * (1 + em * 8) + em;
//
//#endif
//
//#if defined(RCAM_FX2)
//
//    // Slicer seed calculation
//
//    // Slice frequency (1/height)
//    float freq = 60;
//
//    // Per-slice random seed
//    uint seed1 = floor(wpos.y * freq + 200) * 2;
//
//    // Random slice width
//    float width = lerp(0.5, 2, Hash(seed1));
//
//    // Random slice speed
//    float speed = lerp(1.0, 5, Hash(seed1 + 1));
//
//    // Effect direction
//    float3 dir = float3(_EffectParams.z, 0, _EffectParams.w);
//
//    // Potential value (scrolling strips)
//    float pt = (dot(wpos, dir) + 100 + _Time.y * speed) * width;
//
//    // Per-strip random seed
//    uint seed2 = (uint)floor(pt) * 0x87893u;
//
//    // Color mapping with per-strip UV displacement
//    float2 disp = float2(Hash(seed2), Hash(seed2 + 1)) - 0.5;
//    float3 cm = tex2D(_ColorTexture, frac(uv + disp * 0.1)).rgb;
//
//    // Per-strip random color
//    float3 cr = HsvToRgb(float3(Hash(seed2 + 2), 1, 1));
//
//    // Color selection (color map -> random color -> black)
//    float sel = Hash(seed2 + 3);
//    float3 rgb = sel < _EffectParams.x * 2 ? cr : cm;
//    rgb = sel < _EffectParams.x * 2 - 1 ? 0 : rgb;
//
//    // Emission
//    float3 em = Hash(seed2 + 4) < _EffectParams.y * 0.5;
//
//    // Output
//    return rgb * (1 + em * 8) + em;
//
//#endif
}

void FullScreenPass(Varyings varyings,
                    out float4 outColor : SV_Target,
                    out float outDepth : SV_Depth)
{
    // Calculate the UV coordinates from varyings
    float2 uv =
      (varyings.positionCS.xy + float2(0.5, 0.5)) * _ScreenSize.zw;

    // Color/depth samples
    float4 c = tex2D(_ColorTexture, uv);
    float d = tex2D(_DepthTexture, uv).x;

    // Inverse projection
    float3 p = DistanceToWorldPosition(uv, d);

#if !defined(OTAVJ_NOFX)

    // Source pixel luma value
    float lum = Luminance(FastLinearToSRGB(c.rgb));

    // Foreground effect
    float3 eff = ForegroundEffect(p, uv, lum);
    c.rgb = lerp(c.rgb, eff, c.a * _Opacity.y);

#endif

    // BG opacity
    float3 bg = FastSRGBToLinear(FastLinearToSRGB(c.rgb) * _Opacity.x);
    c.rgb = lerp(bg, c.rgb, c.a);

    // Depth mask
    bool mask = c.a > 0.5 || _Opacity.x > 0;

    // Output
    //outColor = DistanceToDepth(d) * mask + _DepthOffset;
    outColor = c;
    outDepth = DistanceToDepth(d) * mask + _DepthOffset;
    //outDepth = 0;
}
