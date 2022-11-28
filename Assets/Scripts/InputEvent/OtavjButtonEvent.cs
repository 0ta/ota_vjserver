using UnityEngine;
using UnityEngine.Events;

namespace ota.ndi
{

    sealed class OtavjButtonEvent : MonoBehaviour
    {
        [SerializeField] int _controlNumber = 0;
        [SerializeField] UnityEvent _onEvent = null;
        [SerializeField] UnityEvent _offEvent = null;

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
                if (newState) _onEvent.Invoke(); else _offEvent.Invoke();
                _state = newState;
            }
        }
    }

}
