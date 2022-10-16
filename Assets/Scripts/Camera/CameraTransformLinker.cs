using UnityEngine;

namespace ota.ndi
{

    sealed class CameraTransformLinker : MonoBehaviour
    {
        void Update()
        {
            var metadata = OtavjVFXResource.Extractor.Metadata;
            if(metadata != null)
            {
                transform.position = metadata.getArcameraPosition();
                transform.rotation = metadata.getArcameraRotation();
            }
        }
    }

} // namespace Rcam2
