using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi
{
    public static class OtavjVFXResource
    {
        static OtavjExtractor _extractor;

        public static OtavjExtractor Extractor
          => _extractor != null ? _extractor :
               (_extractor = Object.FindObjectOfType<OtavjExtractor>());
    }
}

