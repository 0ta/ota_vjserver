using ota.ndi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OtaImageReceiver : MonoBehaviour
{
    [SerializeField] NDIReceiver _ndiReceiver = null;
    [SerializeField] Shader _demuxShader = null;

    [SerializeField] RawImage _tmpcolorrawimage = null;
    [SerializeField] RawImage _tmpdepthrawimage = null;

    RenderTexture ColorTexture;
    RenderTexture DepthTexture;
    Material _demuxMaterial;

    // Start is called before the first frame update
    void Start()
    {
        _demuxMaterial = new Material(_demuxShader);
    }

    // Update is called once per frame
    void Update()
    {
        ExtractTextures();
    }

    void ExtractTextures()
    {
        var source = _ndiReceiver.texture;
        if (source == null) return;

        // Lazy initialization
        if (ColorTexture == null) InitializeTextures(source);

        // Parameters from metadata
        //_demuxMaterial.SetVector("", _metadata.DepthRange);

        // Blit (color/depth)
        ColorTexture.Release();
        Graphics.Blit(source, ColorTexture, _demuxMaterial, 0);
        _tmpcolorrawimage.texture = ColorTexture;
        DepthTexture.Release();
        Graphics.Blit(source, DepthTexture, _demuxMaterial, 1);
        _tmpdepthrawimage.texture = DepthTexture;
    }

    void InitializeTextures(RenderTexture source)
    {
        //var w = source.width / 2;
        //var h = source.height / 2;
        //ColorTexture = new RenderTexture(w, h * 2, 0);

        var w = source.width;
        var h = source.height;
        ColorTexture = new RenderTexture(w, h, 0);
        ColorTexture.Create();
        DepthTexture = new RenderTexture(w, h, 0, RenderTextureFormat.RHalf);
        DepthTexture.Create();
    }
}
