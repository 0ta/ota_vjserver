using UnityEngine;
using UnityEngine.Events;

namespace ota.ndi
{

    sealed class OtavjSliderEvent : MonoBehaviour
    {
        [System.Serializable] class FloatEvent : UnityEvent<float> { }

        [SerializeField] int _controlNumber = 0;
        [SerializeField] FloatEvent _event = null;

        float _value;
        bool _isFirst = true;

        void Update()
        {
            var extractor = OtavjVFXResource.Extractor;
            if (extractor == null || extractor.Metadata == null) return;
            if (_isFirst)
            {
                _value = extractor.Metadata.GetSlider(_controlNumber) + 1;
                _isFirst = false;
            }
            var newValue = extractor.Metadata.GetSlider(_controlNumber);
            if (newValue != _value)
            {
                _event.Invoke(newValue);
                _value = newValue;
            }
        }
    }

}
