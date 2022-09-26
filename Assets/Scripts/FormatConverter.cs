using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ota.ndi
{

    sealed class FormatConverter : System.IDisposable
    {
        private ComputeBuffer _decoderInput;
        private RenderTexture _decoderOutput;
        private ComputeShader _decoderCompute;

        public FormatConverter(ComputeShader decoderCompute)
        {
            _decoderCompute = decoderCompute;
        }

        public void Dispose() => ReleaseBuffers();

        private void ReleaseBuffers()
        {
            _decoderInput?.Dispose();
            _decoderInput = null;
        }

        public RenderTexture Decode(int width, int height, IntPtr data, bool enableAlpha = true)
        {
            var dataCount = Utils.FrameDataCount(width, height, enableAlpha);
            // Input buffer allocation
            if (_decoderInput == null)
            {
                _decoderInput = new ComputeBuffer(dataCount, 4);
            }

            // Output buffer allocation
            if (_decoderOutput == null)
            {
                //_decoderOutput = new RenderTexture(width, height, 0, GraphicsFormat.R8G8B8A8_UNorm);
                _decoderOutput = new RenderTexture(width, height, 0);
                _decoderOutput.enableRandomWrite = true;
                _decoderOutput.Create();
            }
            
            // ��x�ă}�[�V�������O���Ă��炶��Ȃ���Shader�ɓn���Ȃ��񂶂�Ȃ��H�H
            // ��ł�����x�`�F�b�N 
            var bytes = new byte[dataCount * 4];
            Marshal.Copy(data, bytes, 0, bytes.Length);
            // Input buffer update
            _decoderInput.SetData(bytes);

            // �O�̂��ߕϐ�
            int pass = 0;

            // Compute thread dispatching
            _decoderCompute.SetBuffer(pass, "Source", _decoderInput);
            _decoderCompute.SetTexture(pass, "Destination", _decoderOutput);
            _decoderCompute.Dispatch(pass, width / 16, height / 8, 1);
            //for debug
            //GetPixels();
            //UnityEditor.EditorApplication.isPlaying = false;
            return _decoderOutput;
        }

        private Color[] GetPixels()
        {
            // �A�N�e�B�u�ȃ����_�[�e�N�X�`�����L���b�V�����Ă���
            var currentRT = RenderTexture.active;

            // �A�N�e�B�u�ȃ����_�[�e�N�X�`�����ꎞ�I��Target�ɕύX����
            RenderTexture.active = _decoderOutput;

            // Texture2D.ReadPixels()�ɂ��A�N�e�B�u�ȃ����_�[�e�N�X�`���̃s�N�Z�������e�N�X�`���Ɋi�[����
            var texture = new Texture2D(_decoderOutput.width, _decoderOutput.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, _decoderOutput.width, _decoderOutput.height), 0, 0);
            texture.Apply();

            // �s�N�Z�������擾����
            var colors = texture.GetPixels();

            // �A�N�e�B�u�ȃ����_�[�e�N�X�`�������ɖ߂�
            RenderTexture.active = currentRT;
            printColorsInfo(colors);
            return colors;
        }

        private void printColorsInfo(Color[] colors)
        {
            for(int i = 0; i < colors.Length; i++)
            {
                Debug.Log(colors[i]);
            }
        }
    }
}