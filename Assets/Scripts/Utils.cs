using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ota.ndi
{
    public static class FrameUtils
    {
        public static int FrameDataCount(int width, int height, bool alpha)
            => width * height * (alpha ? 3 : 2) / 4;
    }

    static class ProjectionUtils
    {
        public static Vector4 GetVector(in Matrix4x4 m)
        {
            return new Vector4(m[0, 2], m[1, 2], m[0, 0], m[1, 1]);
        }

        public static Vector4? VectorFromReceiver
            => PTVectorFromReceiver();

        private static Vector4? PTVectorFromReceiver()
        {
            if (OtavjVFXResource.Extractor.Metadata == null) return null;
            return GetVector(OtavjVFXResource.Extractor.Metadata.getProjectionMatrix());
        }
        public static Matrix4x4? CameraToWorldMatrix
            => CalculateCameraToWorldMatrix();
        static Matrix4x4? CalculateCameraToWorldMatrix()
        {
            if (OtavjVFXResource.Extractor.Metadata == null) return null;
            if (OtavjVFXResource.Extractor.Metadata.getArcameraPosition() == Vector3.zero) return Matrix4x4.identity;
            return Matrix4x4.TRS
              (OtavjVFXResource.Extractor.Metadata.getArcameraPosition(), OtavjVFXResource.Extractor.Metadata.getArcameraRotation(), new Vector3(1, 1, -1));
        }
    }
}
