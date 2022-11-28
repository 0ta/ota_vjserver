using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ota.ndi
{
    public class VFXActivator : MonoBehaviour
    {
        VisualEffect vfx;
        float _currentThrottle = 1.0f;
        bool _activecommand = false;

        const string THROTTLE = "Throttle";
        // Start is called before the first frame update
        void Start()
        {
            vfx = this.GetComponent<VisualEffect>();
        }

        void Update()
        {
            if (!vfx.HasFloat(THROTTLE)) return;

            var isActive = vfx.enabled;
            if (isActive && !_activecommand)
            {
                var delta = Time.deltaTime * 10;
                _currentThrottle -= delta;
                vfx.SetFloat(THROTTLE, _currentThrottle);
                if (_currentThrottle < 0)
                {
                    Invoke("DoDeactivate", 3);
                }
            } else if (!isActive && _activecommand)
            {
                _currentThrottle = 1.0f;
                vfx.SetFloat(THROTTLE, _currentThrottle);
                vfx.enabled = true;
            }

        }

        void DoDeactivate()
        {
            vfx.enabled = false;
        }

        public void doActivate(bool b)
        {
            if (!vfx.HasFloat(THROTTLE))
            {
               vfx.enabled = b;
               return;
            }
            _activecommand = b;
        }
    }
}

