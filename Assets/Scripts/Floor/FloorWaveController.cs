using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi
{
    public class FloorWaveController : MonoBehaviour
    {
        public MeshFilter m_floorwaveMeshFilter;
        // Start is called before the first frame update
        void Start()
        {
            if (m_floorwaveMeshFilter == null) return;
            //m_floorwaveMeshFilter.mesh.SetIndices(m_floorwaveMeshFilter.mesh.triangles, MeshTopology.Lines, 0);
        }

        // Update is called once per frame
        //void Update()
        //{

        //}
    }
}

