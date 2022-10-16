using Newtonsoft.Json;
using ota.ndi;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace ota.ndi
{
    public class OtavjExtractor : MonoBehaviour
    {
        [SerializeField] NDIReceiver _ndiReceiver = null;
        [SerializeField] Shader _demuxShader = null;

        [SerializeField] RawImage _tmpcolorrawimage = null;
        [SerializeField] RawImage _tmpdepthrawimage = null;

        [HideInInspector] public RenderTexture ColorTexture;
        [HideInInspector] public RenderTexture DepthTexture;
        [HideInInspector] public MetadataInfo Metadata;

        Material _demuxMaterial;

        // Start is called before the first frame update
        void Start()
        {
            _demuxMaterial = new Material(_demuxShader);
        }

        private void OnDestroy()
        {
            ReleaseInstanceObject();
        }

        void ReleaseInstanceObject()
        {
            Destroy(ColorTexture);
            Destroy(DepthTexture);
        }

        // Update is called once per frame
        void Update()
        {
            ExtractMetadata();
            ExtractTextures();
        }

        void ExtractTextures()
        {
            var source = _ndiReceiver.texture;
            if (source == null) return;

            // Lazy initialization
            if (ColorTexture == null) InitializeTextures(source);

            // Parameters from metadata
            Debug.Log("DepthRange!!!");
            Debug.Log(Metadata.maxDepth);
            Debug.Log(Metadata.minDepth);
            _demuxMaterial.SetVector("_DepthRange", Metadata.getDepthRange());

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

        void ExtractMetadata()
        {
            Regex reg = new Regex("\\{\"arcameraPosition\".+\\}", RegexOptions.Singleline);
            var match = reg.Match(_ndiReceiver.metadatastr);
            var json = match.ToString();
            if (json != null)
            {
                Metadata = JsonConvert.DeserializeObject<MetadataInfo>(json);
            }
        }
    }
}
