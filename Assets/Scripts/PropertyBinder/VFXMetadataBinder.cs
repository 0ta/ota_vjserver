using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace ota.ndi
{
    [VFXBinder("Otavj/Metadata")]
    public class VFXMetadataBinder : VFXBinderBase
    {
        public string ColorMapProperty
        {
            get => (string)_colorMapProperty;
            set => _colorMapProperty = value;
        }

        public string DepthMapProperty
        {
            get => (string)_depthMapProperty;
            set => _depthMapProperty = value;
        }

        public string ProjectionVectorProperty
        {
            get => (string)_projectionVectorProperty;
            set => _projectionVectorProperty = value;
        }

        public string InverseViewMatrixProperty
        {
            get => (string)_inverseViewMatrixProperty;
            set => _inverseViewMatrixProperty = value;
        }

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _colorMapProperty = "ColorMap";

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _depthMapProperty = "DepthMap";

        [VFXPropertyBinding("UnityEngine.Vector4"), SerializeField]
        ExposedProperty _projectionVectorProperty = "ProjectionVector";

        [VFXPropertyBinding("UnityEngine.Matrix4x4"), SerializeField]
        ExposedProperty _inverseViewMatrixProperty = "InverseViewMatrix";

        public override bool IsValid(VisualEffect component)
      => component.HasTexture(_colorMapProperty) &&
         component.HasTexture(_depthMapProperty) &&
         component.HasVector4(_projectionVectorProperty) &&
         component.HasMatrix4x4(_inverseViewMatrixProperty);

        public override void UpdateBinding(VisualEffect component)
        {
            var extractor = OtavjVFXResource.Extractor;
            var prj = ProjectionUtils.VectorFromReceiver;
            var v2w = ProjectionUtils.CameraToWorldMatrix;

            if (extractor == null || extractor.Metadata == null) return;

            component.SetTexture(_colorMapProperty, extractor.ColorTexture);
            component.SetTexture(_depthMapProperty, extractor.DepthTexture);
            component.SetVector4(_projectionVectorProperty, prj.Value);
            component.SetMatrix4x4(_inverseViewMatrixProperty, v2w.Value);
        }
    }
}
