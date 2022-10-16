using Newtonsoft.Json;
using ota.ndi;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class test : MonoBehaviour
{
    MetadataInfo metadata = null;
    // Start is called before the first frame update
    void Start()
    {
        //Matrix4x4 m = Matrix4x4.identity;
        //m[1, 0] = 1;
        //Debug.Log(m);
    }

    // Update is called once per frame
    void Update()
    {
        string xmlstr = "<metadata><![CDATA[{\"arcameraPosition\":\"(0.00, 0.00, 0.00)\",\"arcameraRotation\":\"(-0.20138, 0.00338, -0.00690, -0.97948)\",\"projectionMatrix\":\"1.67576\t0.00000\t0.00786\t0.00000\n0.00000\t2.39912\t-0.00349\t0.00000\n0.00000\t0.00000\t-1.01005\t-0.20101\n0.00000\t0.00000\t-1.00000\t0.00000\n\"}]]></metadata>";     
        Regex reg = new Regex("\\{\"arcameraPosition\".+\"\\}", RegexOptions.Singleline);
        var match = reg.Match(xmlstr);
        var json = match.ToString();
        if (json != null)
        {
            metadata = JsonConvert.DeserializeObject<MetadataInfo>(json);
        }
        Debug.Log(metadata.projectionMatrix);
        Debug.Log(metadata.getProjectionMatrix());
        //var pmarray = metadata.convertStr2FloatArray(metadata.projectionMatrix);
        //foreach (var pm in pmarray)
        //{
        //    Debug.Log(pm);
        //}






    }
}
