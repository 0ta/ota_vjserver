#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
#include "OtaVjNoiseCommon.hlsl"
#include "OtaVjColorCommon.hlsl"
//#include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

sampler2D _ColorTexture;
sampler2D _DepthTexture;
float4 _ProjectionVector;
float4x4 _InverseViewMatrix;
float _DepthOffset;

float2 _Opacity; // Background, Effect
float4 _EffectParams; // param, intensity, sin(r), cos(r)
int _BgPattern; // 0: Normal, 1: Noise, 2: Distortion

static float delta = 1.0f / 15.0f;

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

float getTickedTime(float delta)
{
    float time = _Time.y;
    float garbage = fmod(time, delta);
    return time - garbage;
}

float2 remap(float2 In, float2 InMinMax, float2 OutMinMax)
{
    return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

float3 effectMonoPatternA(float3 wpos, float2 uv, float luma)
{
    float c = random_A(normalize(uv.y + _Time.w));
    return float3(c, c, c);
}

float3 effectMonoPatternAd(float3 wpos, float2 uv, float luma)
{
    float c1 = random_A(normalize(uv.x + _Time.w));
    float c2 = random_A(normalize(uv.x + _Time.x));
    float c3 = random_A(normalize(uv.x + _Time.z));
    return float3(c1, c2, c3) * 1.3f;
}

float3 effectMonoPatternB(float3 wpos, float2 uv, float luma)
{
    float2 st = uv * 70.0;
    float2 ipos = floor(st);
    float2 fpos = frac(st);
    float c1 = random(ipos + _Time.w);
    float c2 = random(ipos + _Time.x);
    float c3 = random(ipos + _Time.z);
    return float3(c1, c2, c3);
}

float3 effectMonoPatternC(float3 wpos, float2 uv, float luma)
{
    const float2 vp = float2(320.0, 200.0);
    float t = _Time.w;
    float2 p0 = (uv - 0.5) * vp;
    float2 hvp = vp * 0.5;
    float2 p1d = float2(cos(t / 98.0), sin(t / 178.0)) * hvp - p0;
    float2 p2d = float2(sin(-t / 124.0), cos(-t / 104.0)) * hvp - p0;
    float2 p3d = float2(cos(-t / 165.0), cos(t / 45.0)) * hvp - p0;
    float sum = 0.5 + 0.5 * (
		cos(length(p1d) / 30.0) +
		cos(length(p2d) / 20.0) +
		sin(length(p3d) / 25.0) * sin(p3d.x / 20.0) * sin(p3d.y / 15.0));
    return tex2D(_ColorTexture, frac(uv + float2(frac(sum), frac(sum)))).rgb;
}

float3 effectMonoPatternD(float3 wpos, float2 uv, float luma)
{
    float2 st = uv;
    st.x *= _ScreenSize.x / _ScreenSize.y;

    float3 color = float3(0.0, 0.0, 0.0);

    float cols = 2.;
    float freq = random(floor(_Time.w)) + abs(atan(_Time.w) * 0.1);
    float t = 60. + _Time.w * (1.0 - freq) * 30.;

    if (frac(st.y * cols * 0.5) < 0.5)
    {
        t *= -1.0;
    }

    freq += random(floor(st.y));

    float offset = 0.025;
    color = float3(randomSerie(st.x, freq * 100., t + offset),
                 randomSerie(st.x, freq * 100., t),
                 randomSerie(st.x, freq * 100., t - offset));
    //color = float3(randomSerie(st.y, freq * 100., t + offset),
    //             randomSerie(st.y, freq * 100., t),
    //             randomSerie(st.y, freq * 100., t - offset));
    return color;
}

float3 effectMonoPatternE(float3 wpos, float2 uv, float luma)
{
    float2 st = uv;
    st.x *= _ScreenSize.x / _ScreenSize.y;
    float3 color = float3(0.0, 0.0, 0.0);
    float2 pos = float2(st * 3.);

    float DF = 0.0;

    //// Add a random position
    float a = 0.0;
    float2 vel = float2(_Time.w * .1, _Time.w * .1);
    DF += snoise(pos + vel) * .25 + .25;

    //// Add a random position
    a = snoise(pos * float2(cos(_Time.w * 0.15), sin(_Time.w * 0.1)) * 0.1) * 3.1415;
    vel = float2(cos(a), sin(a));
    DF += snoise(pos + vel) * .25 + .25;

    float ssv = smoothstep(.7, .75, frac(DF));
    color = float3(ssv, ssv, ssv);

    return float3(1.0 - color);
}

float3 effectMonoPatternF(float3 wpos, float2 uv, float luma)
{
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
}

float4 effectMonoPatternG_alpha(float3 wpos, float2 uv, float luma)
{
    float glitchStep = lerp(4.0f, 32.0f, random(getTickedTime(delta)));
    
    float4 screenColor = tex2D(_ColorTexture, uv);
    
    uv.x = round(uv.x * glitchStep) / glitchStep;
    float4 glitchColor = tex2D(_ColorTexture, uv);
    
    return lerp(screenColor, glitchColor, float4(0.3f, 0.3f, 0.3f, 0.3f));
}

float3 effectMonoPatternG(float3 wpos, float2 uv, float luma)
{
    return effectMonoPatternG_alpha(wpos, uv, luma).xyz;
}

float3 effectMonoPatternH(float3 wpos, float2 uv, float luma)
{
    //float intensity = clamp(2.0f * sin(getTickedTime(delta)), 0.0f, 10.0f);
    //intensity *= 1.5f;
    float intensity = 1.5f;
    
    float texc1 = tex2D(_ColorTexture, frac(uv + random(float2(getTickedTime(delta), 0.25f)) * 10.0f) * 0.75f).r;
    float texc2 = tex2D(_ColorTexture, frac(uv + random(float2(getTickedTime(delta), 0.78f)) * 10.0f) * 0.5f).r;
    
    // recalculate intensity
    float prechrOffset = step(0.5f * (texc1 + texc2), 0.5f);
    float chrOffset = (2.0f * prechrOffset + 1.0f) * 0.005f * intensity;
    
    float4 screenColor = tex2D(_ColorTexture, uv);
    float chrColR = tex2D(_ColorTexture, float2(uv.x + chrOffset, uv.y)).r;
    float chrColB = tex2D(_ColorTexture, float2(uv.x - chrOffset, uv.y)).b;
    return float3(chrColR, screenColor.g, chrColB);
}

float3 effectMonoPatternI(float3 wpos, float2 uv, float luma)
{
    float n = snoise((uv * 10) + _Time.y);
    //float rmn = remap(n, float2(0, 1), float2(0, 0.03));
    float rmn = remap(n, float2(0, 1), float2(0, 0.01));
    uv += rmn;
    return tex2D(_ColorTexture, uv).xyz;
}

// Foreground effect
float3 ForegroundEffect(float3 wpos, float2 uv, float luma)
{
    
#if defined(OTAVJ_FX0)
    
    float rdm = random(getTickedTime(delta));
    rdm *= 10.0;
    if (rdm < 1.0f)
    {
        return effectMonoPatternA(wpos, uv, luma);
    }
    else if (rdm < 4.0f)
    {
        return effectMonoPatternG(wpos, uv, luma);
    }
    else
    {
        return effectMonoPatternH(wpos, uv, luma);
    }
    
#endif

#if defined(OTAVJ_FX1)

    float rdm = random(getTickedTime(delta));
    rdm *= 10.0;
    if (rdm < 2.0f)
    {
        return effectMonoPatternAd(wpos, uv, luma);
    }
    else if (rdm < 4.0f)
    {
        return effectMonoPatternB(wpos, uv, luma);
    }
    else if (rdm < 6.0f)
    {
        return effectMonoPatternC(wpos, uv, luma);
    }
    else 
    {
        return effectMonoPatternD(wpos, uv, luma);
    }
    
#endif

#if defined(OTAVJ_FX2)

    // Marble-like pattern

    // Frequency
    float freq = lerp(2.75, 20, _EffectParams.x);

    // Noise field position
    float3 np = wpos * float3(1.2, freq, 1.2);
    np += float3(0, -0.784, 0) * _Time.y;

    // Potential value
    float pt = 0.5 + (luma - 0.5) * 0.4 + snoise(np) * 0.7;

    // Random seed
    uint seed = (uint) (pt * 5 + _Time.y * 5) * 2;

    // Color
    //float3 rgb = FastSRGBToLinear(hsv2rgb(float3(Hash(seed), 1, 1)));
    float3 rgb = hsv2rgb(float3(hashIQf(seed), 1, 1));

    // Emission
    float em = hashIQf(seed + 1) < _EffectParams.y * 0.5;

    // Output
    return rgb * (1 + em * 8) + em;

#endif

#if defined(OTAVJ_FX3)

    // Slicer seed calculation

    // Slice frequency (1/height)
    float freq = 60;

    // Per-slice random seed
    uint seed1 = floor(wpos.y * freq + 200) * 2;

    // Random slice width
    float width = lerp(0.5, 2, Hash(seed1));

    // Random slice speed
    float speed = lerp(1.0, 5, Hash(seed1 + 1));

    // Effect direction
    float3 dir = float3(_EffectParams.z, 0, _EffectParams.w);

    // Potential value (scrolling strips)
    float pt = (dot(wpos, dir) + 100 + _Time.y * speed) * width;

    // Per-strip random seed
    uint seed2 = (uint) floor(pt) * 0x87893u;

    // Color mapping with per-strip UV displacement
    float2 disp = float2(Hash(seed2), Hash(seed2 + 1)) - 0.5;
    float3 cm = tex2D(_ColorTexture, frac(uv + disp * 0.1)).rgb;

    // Per-strip random color
    float3 cr = HsvToRgb(float3(Hash(seed2 + 2), 1, 1));

    // Color selection (color map -> random color -> black)
    float sel = Hash(seed2 + 3);
    float3 rgb = sel < _EffectParams.x * 2 ? cr : cm;
    rgb = sel < _EffectParams.x * 2 - 1 ? 0 : rgb;

    // Emission
    float3 em = Hash(seed2 + 4) < _EffectParams.y * 0.5;

    // Output
    return rgb * (1 + em * 8) + em;

#endif
}

float3 BackgroundEffect1(float3 wpos, float2 uv, float luma)
{
    float rdm = random(getTickedTime(delta));
    rdm *= 10.0;
    if (rdm < 1.0f)
    {
        return effectMonoPatternH(wpos, uv, luma);
    }
    else
    {
        return effectMonoPatternG(wpos, uv, luma);
    }
}

float3 BackgroundEffect2(float3 wpos, float2 uv, float luma)
{
    float rdm = random(getTickedTime(delta));
    rdm *= 10.0;
    if (rdm < 1.0f)
    {
        return effectMonoPatternH(wpos, uv, luma);
    }
    else
    {
        return effectMonoPatternI(wpos, uv, luma);
    }
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
#if defined(OTAVJ_NOFX)
    float3 bg = FastSRGBToLinear(FastLinearToSRGB(c.rgb) * _Opacity.x);
#endif
    
#if !defined(OTAVJ_NOFX)
    float3 bgeff;
    if (_BgPattern == 1)
    {
        bgeff = BackgroundEffect1(p, uv, lum);
    }
    else if (_BgPattern == 2)
    {
        bgeff = BackgroundEffect2(p, uv, lum);
    }
    else
    {
        bgeff = c.rgb;
    }
    float3 bg = FastSRGBToLinear(FastLinearToSRGB(bgeff) * _Opacity.x);
#endif

    c.rgb = lerp(bg, c.rgb, c.a);
    
    // Depth mask
    bool mask = c.a > 0.5 || _Opacity.x > 0;

    // Output
    outColor = c;
    //outColor = DistanceToDepth(d) * mask + _DepthOffset;
    outDepth = DistanceToDepth(d) * mask + _DepthOffset;
}