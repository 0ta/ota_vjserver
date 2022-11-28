using UnityEngine;
using UnityEngine.Events;

namespace ota.ndi
{

    sealed class OtavjToggleEvent : MonoBehaviour
    {
        [System.Serializable] class BoolEvent : UnityEvent<bool> { }

        [SerializeField] int _controlNumber = 0;
        [SerializeField] BoolEvent _event = null;

        bool _state;
        bool _isFirst = true;
        void Update()
        {
            var extractor = OtavjVFXResource.Extractor;
            if (extractor == null || extractor.Metadata == null) return;
            if (_isFirst)
            {
                _state = !extractor.Metadata.GetToggle(_controlNumber);
                _isFirst = false;
            }
            var newState = extractor.Metadata.GetToggle(_controlNumber);
            if (newState != _state)
            {
                _event.Invoke(newState);
                _state = newState;
            }
        }
    }
}
