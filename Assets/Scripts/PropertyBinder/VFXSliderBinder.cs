using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace ota.ndi
{
    [VFXBinder("Otavj/Slider")]
    public class VFXSliderBinder : VFXBinderBase
    {
        public string Property
        {
            get => (string)_property;
            set => _property = value;
        }

        [VFXPropertyBinding("System.Single"), SerializeField]
        ExposedProperty _property = "Property name";

        public int ControlNumber = 0;

        public override bool IsValid(VisualEffect component)
          => component.HasFloat(_property);

        public override void UpdateBinding(VisualEffect component)
        {
            var extractor = OtavjVFXResource.Extractor;
            if (extractor == null || extractor.Metadata == null) return;

            component.SetFloat
              (_property, extractor.Metadata.GetSlider(ControlNumber));
        }

        public override string ToString()
          => $"Property name: '{_property}' -> Slider {ControlNumber}";
    }
}



