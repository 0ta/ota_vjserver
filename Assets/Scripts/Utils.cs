using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ota.ndi
{
    public static class Utils
    {
        public static int FrameDataCount(int width, int height, bool alpha)
            => width * height * (alpha ? 3 : 2) / 4;

    }
}
