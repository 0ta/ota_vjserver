using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi {
    public class FlowerManager : MonoBehaviour
    {
        public GameObject[] flowers;
        void Start()
        {
            var parent = this.transform.parent;
            for (int i = 0; i < 1000; i++)
            {
                var x = Random.Range(-25f, 25f);
                var z = Random.Range(-25f, 25f);
                var fnum = Random.Range(0, flowers.Length - 1);

                var flowerobj = Instantiate(flowers[fnum], parent.Find("FlowerMesh").transform);

                Vector3 pos = flowerobj.transform.position;
                pos.x += x;
                pos.z += z;

                flowerobj.transform.position = pos;
            }

        }

        //void Update()
        //{

        //}
    }
}


