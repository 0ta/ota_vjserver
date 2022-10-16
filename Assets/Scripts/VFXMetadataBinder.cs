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
            //
            // èÁí∑ÇæÇ©ÇÁå„Ç≈èëÇ´ä∑Ç¶ÇÈ
            //
            if (extractor.ColorTexture != null)
            {
                component.SetTexture(_colorMapProperty, extractor.ColorTexture);
            }
            if (extractor.DepthTexture != null)
            {
                component.SetTexture(_depthMapProperty, extractor.DepthTexture);
            }
            if(prj is Vector4 prjvalue)
            {
                Debug.Log(1 + ":" + prjvalue.ToString("F5"));
                Debug.Log(2 + ":" + OtavjVFXResource.Extractor.Metadata.getProjectionMatrix());
                component.SetVector4(_projectionVectorProperty, prjvalue);
            }
            if(v2w is Matrix4x4 v2wvalue)
            {
                Debug.Log(3 + ":" + v2wvalue);
                Debug.Log(4 + ":" + OtavjVFXResource.Extractor.Metadata.getDepthRange());
                component.SetMatrix4x4(_inverseViewMatrixProperty, v2wvalue);
            }
        }
    }
}
