using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ota.ndi
{
    [System.Serializable]
    sealed class OtavjBackgroundPass : CustomPass
    {

        public OtavjBackgroundController _controller = null;
        public bool _depthOffset = false;
        public Shader _shader = null;

        Material _material;        

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            //var shader = Resources.Load<Shader>("OtavjBackground");
            _material = new Material(_shader);
            _material.hideFlags = HideFlags.DontSave;
        }

        protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd,
           HDCamera hdCamera, CullingResults cullingResult)
        {
            if (_controller == null) return;

            var extractor = OtavjVFXResource.Extractor;
            var prj = ProjectionUtils.VectorFromReceiver;
            var v2w = ProjectionUtils.CameraToWorldMatrix;

            if (extractor == null || extractor.Metadata == null) return;

            _material.SetVector("_ProjectionVector", prj.Value);
            _material.SetMatrix("_InverseViewMatrix", v2w.Value);
            _material.SetFloat("_DepthOffset", _depthOffset ? -1e-7f : 0);
            _material.SetTexture("_ColorTexture", extractor.ColorTexture);
            _material.SetTexture("_DepthTexture", extractor.DepthTexture);

            //
            //for test
            //
            //_Opacity:x ”wŒi‚ð‚ð•\Ž¦‚·‚é‚©”Û‚©   0:Black -> 1:Original color
            //_Opacity:y Human effect‚Ì‚©‚©‚è‹ï‡ 0:Oricinal color -> 1:Effect
            //_material.SetVector("_Opacity", new Vector2(1.0f, 0.0f));
            //var eparams = new Vector4(0.8f, 0.8f, 0, 0);
            //_material.SetVector("_EffectParams", eparams);
            //_material.SetInt("_BgPattern", 2);
            //
            //CoreUtils.DrawFullScreen(cmd, _material, null, 1);

            CoreUtils.DrawFullScreen(cmd, _material, _controller.PropertyBlock, _controller.PassNumber);

        }

        protected override void Cleanup()
        {
            if (Application.isPlaying)
                Object.Destroy(_material);
            else
                Object.DestroyImmediate(_material);
        }
    }
}

