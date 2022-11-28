using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace ota.ndi
{
    public class OtavjBackgroundController : MonoBehaviour
    {
        public bool BackFill { get; set; }
        public int EffectNumber { get; set; }
        public int BgEffectNumber { get; set; }
        public float EffectParameter { get; set; } = 0.8f;
        public float EffectIntensity { get; set; }
        public float EffectDirection { get; set; }

        float _backOpacity;
        float _effectOpacity;
        int _currentEffect;

        [HideInInspector]
        public MaterialPropertyBlock PropertyBlock => UpdatePropertyBlock();
        MaterialPropertyBlock _props;
        public int PassNumber => _currentEffect;
        [HideInInspector]
        MaterialPropertyBlock UpdatePropertyBlock()
        {
            if (_props == null) _props = new MaterialPropertyBlock();

            var oparams = new Vector2(_backOpacity, _effectOpacity);
            var phi = EffectDirection * Mathf.PI * 2;
            var eparams = new Vector4
              (EffectParameter, EffectIntensity, Mathf.Sin(phi), Mathf.Cos(phi));

            _props.SetVector("_Opacity", oparams);
            _props.SetVector("_EffectParams", eparams);
            _props.SetInt("_BgPattern", BgEffectNumber);

            return _props;
        }
        // Update is called once per frame
        void Update()
        {
            var delta = Time.deltaTime * 10;

            // BG opacity animation
            var dir = BackFill ? 1 : -1;
            _backOpacity = Mathf.Clamp01(_backOpacity + dir * delta);

            // Effect opacity animation
            dir = _currentEffect == EffectNumber ? 1 : -1;
            _effectOpacity = Mathf.Clamp01(_effectOpacity + dir * delta);

            // We can switch the effect when the opacity becomes zero.
            if (_currentEffect != EffectNumber && _effectOpacity == 0)
                _currentEffect = EffectNumber;
        }

    }
}
