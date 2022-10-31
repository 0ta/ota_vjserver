using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ota.ndi {
    public class OtavjBgMeshManager : MonoBehaviour
    {
        public MeshFilter m_BackgroundMeshPrefab;
        readonly Dictionary<string, MeshFilter> m_MeshMap = new Dictionary<string, MeshFilter>();

        // Update is called once per frame
        void Update()
        {
            var bgmetadata = OtavjVFXResource.Extractor.BgMetadata;
            if (bgmetadata == null) return;
            var meshkeyList = bgmetadata.meshKeyList;
            var meshList = bgmetadata.getSentMeshList();
            for (int i = 0; i < meshkeyList.Count; i++)
            {
                var meshkey = meshkeyList[i];
                if (m_MeshMap.ContainsKey(meshkey))
                {
                    if (m_MeshMap[meshkey].mesh.vertices.Length != meshList.Count)
                    {
                        m_MeshMap[meshkey].mesh = meshList[i];

                        //Debug.Log("here");
                        ////test
                        //var it = meshList[i].vertices.GetEnumerator();
                        //StringBuilder sb = new StringBuilder();
                        //while (it.MoveNext())
                        //{
                        //    Vector3 vec3 = (Vector3)it.Current;
                        //    sb.Append(",");
                        //    sb.Append(vec3.x.ToString("F9"));
                        //    sb.Append(",");
                        //    sb.Append(vec3.y.ToString("F9"));
                        //    sb.Append(",");
                        //    sb.Append(vec3.z.ToString("F9"));
                        //}
                        //Debug.Log(sb.ToString());
                    }
                } else
                {
                    var parent = this.transform.parent;
                    var bgmeshfilter = Instantiate(m_BackgroundMeshPrefab, parent);
                    bgmeshfilter.mesh = meshList[i];
                    m_MeshMap.Add(meshkey, bgmeshfilter);
                }
            }

            var deletedeList = bgmetadata.deletedMeshKeyList;
            var deletedit = deletedeList.GetEnumerator();
            while (deletedit.MoveNext())
            {
                var deletedkey = deletedit.Current;
                RemoveMesh(deletedkey);
            }
            
        }

        void RemoveMesh(string meshId)
        {
            var bgmeshfilter = m_MeshMap[meshId];
            Object.Destroy(bgmeshfilter);
            m_MeshMap.Remove(meshId);
        }
    }
}


