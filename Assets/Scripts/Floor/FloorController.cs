using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi {
    public class FloorController : MonoBehaviour
    {
        public float height { get; set; } = 0;
        void Update()
        {
            this.transform.position = new Vector3(this.transform.position.x, height, this.transform.position.z);
        }
    }
}
