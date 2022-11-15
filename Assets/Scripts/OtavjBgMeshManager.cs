using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ota.ndi {
    public class OtavjBgMeshManager : MonoBehaviour
    {
        public MeshFilter m_BackgroundMeshPrefab;
        public MeshFilter m_BackgroundLineMeshPrefab;
        readonly Dictionary<string, MeshFilter> m_MeshMap = new Dictionary<string, MeshFilter>();
        const string linemesh_Suffix = "_line";

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
                        PreRenderingMesh(m_MeshMap[meshkey]);
                        m_MeshMap[meshkey].mesh.Clear();
                        //Object.Destroy(m_MeshMap[meshkey].mesh);
                        //m_MeshMap[meshkey].mesh = CreateMesh(meshList[i].Item1, meshList[i].Item2);
                        m_MeshMap[meshkey].mesh.vertices = meshList[i].Item1;
                        m_MeshMap[meshkey].mesh.triangles = meshList[i].Item2;

                        // manage line mesh as well
                        PreRenderingMesh(m_MeshMap[meshkey + linemesh_Suffix]);
                        m_MeshMap[meshkey + linemesh_Suffix].mesh.Clear();
                        //Object.Destroy(m_MeshMap[meshkey + linemesh_Suffix].mesh);
                        //m_MeshMap[meshkey + linemesh_Suffix].mesh = CreateLineMesh(meshList[i].Item1, meshList[i].Item2);
                        m_MeshMap[meshkey + linemesh_Suffix].mesh.vertices = meshList[i].Item1;
                        m_MeshMap[meshkey + linemesh_Suffix].mesh.SetIndices(meshList[i].Item2, MeshTopology.Lines, 0);

                        //Debug.Log("here");
                        ////test
                        //var it = meshList[i].normals.GetEnumerator();
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
                    var bgmeshfilter = Instantiate(m_BackgroundMeshPrefab, parent.Find("BgMesh").transform);
                    PreRenderingMesh(bgmeshfilter);
                    bgmeshfilter.mesh = CreateMesh(meshList[i].Item1, meshList[i].Item2);
                    //Debug.Log("Added: " + meshkey);
                    m_MeshMap.Add(meshkey, bgmeshfilter);

                    // manage line mesh as well
                    var bglinemeshfilter = Instantiate(m_BackgroundLineMeshPrefab, parent.Find("BgLineMesh").transform);
                    PreRenderingLineMesh(bglinemeshfilter);
                    bglinemeshfilter.mesh = CreateLineMesh(meshList[i].Item1, meshList[i].Item2);
                    m_MeshMap.Add(meshkey + linemesh_Suffix, bglinemeshfilter);
                }
            }

            var deletedeList = bgmetadata.deletedMeshKeyList;
            var deletedit = deletedeList.GetEnumerator();
            while (deletedit.MoveNext())
            {
                var deletedkey = deletedit.Current;
                //Debug.Log("Deleted: " + deletedkey);
                RemoveMesh(deletedkey);
            }
        }

        Mesh CreateMesh(Vector3[] vertices, int[] index)
        {
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = index;
            return mesh;
        }

        Mesh CreateLineMesh(Vector3[] vertices, int[] index)
        {
            var linemesh = new Mesh();
            linemesh.vertices = vertices;
            linemesh.SetIndices(index, MeshTopology.Lines, 0);
            return linemesh;
        }

        void PreRenderingMesh(MeshFilter meshfilter)
        {
            var extractor = OtavjVFXResource.Extractor;
            var prj = ProjectionUtils.VectorFromReceiver;
            var v2w = ProjectionUtils.CameraToWorldMatrix;

            if (extractor == null || extractor.Metadata == null) return;

            Material _material = meshfilter.GetComponentInParent<Renderer>().material;
            //_material.SetVector("_ProjectionVector", prj.Value);
            //_material.SetMatrix("_InverseViewMatrix", v2w.Value);
            _material.SetFloat("_Intensity", 1.0f);
            _material.SetTexture("_ColorTexture", extractor.ColorTexture);
            //_material.SetTexture("_DepthTexture", extractor.DepthTexture);
        }

        void PreRenderingLineMesh(MeshFilter meshfilter)
        {
            //var extractor = OtavjVFXResource.Extractor;
            //var prj = ProjectionUtils.VectorFromReceiver;
            //var v2w = ProjectionUtils.CameraToWorldMatrix;

            //if (extractor == null || extractor.Metadata == null) return;

            Material _material = meshfilter.GetComponentInParent<Renderer>().material;
            //_material.SetVector("_ProjectionVector", prj.Value);
            //_material.SetMatrix("_InverseViewMatrix", v2w.Value);
            _material.SetFloat("_Intensity", 1.0f);
            //_material.SetTexture("_ColorTexture", extractor.ColorTexture);
            //_material.SetTexture("_DepthTexture", extractor.DepthTexture);
        }

        void RemoveMesh(string meshId)
        {
            if (m_MeshMap.ContainsKey(meshId))
            {
                var bgmeshfilter = m_MeshMap[meshId];
                bgmeshfilter.mesh.Clear();
                Object.Destroy(bgmeshfilter);
                m_MeshMap.Remove(meshId);

                // manage line mesh as well
                var bglinemeshfilter = m_MeshMap[meshId + linemesh_Suffix];
                bglinemeshfilter.mesh.Clear();
                Object.Destroy(bglinemeshfilter);
                m_MeshMap.Remove(meshId + linemesh_Suffix);
            }

        }
    }
}


