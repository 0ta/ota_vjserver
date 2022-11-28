using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace ota.ndi
{
    public class NDIReceiver : MonoBehaviour
    {
        [field: SerializeField] private String ReceiveName;
        [SerializeField] private bool _videoEnabled = false;
        [SerializeField] private bool _audioEnabled = false;
        [SerializeField] private ComputeShader _decodeCompute;
        [SerializeField] private RawImage _image = null;

        [HideInInspector] public RenderTexture texture = null;
        [HideInInspector] public string metadatastr = null;
        [HideInInspector] public string bgmetadatastr = null;

        [Description("Does the current source support PTZ functionality?")]
        public bool IsPtz
        {
            get => _isPtz;
            set => _isPtz = value;
        }

        public bool IsRecordingSupported
        {
            get => _canRecord;
            set => _canRecord = value;
        }


        private String _receiveName = "";
        private IntPtr _recvInstancePtr = IntPtr.Zero;

        private Thread _receiveThread = null;
        private bool _exitThread = false;

        private bool _isPtz = false;
        private bool _canRecord = false;

        private SynchronizationContext _mainThreadContext = null;

        private FormatConverter _formatConverter;

        private void Awake()
        {
            //if (!NDIlib.Initialize())
            //{
            //    Debug.Log("Cannot run NDI.");
            //}
            //else
            //{
            //    Debug.Log("Initialized NDI.");

            //    _mainThreadContext = SynchronizationContext.Current;
            //}
            _formatConverter = new FormatConverter(_decodeCompute);
            _mainThreadContext = SynchronizationContext.Current;
        }

        private void OnDestroy()
        {
            Disconnect();
            ReleaseInstanceObject();
        }

        public void Connect(Source source)
        {
            Disconnect();

            if (source == null || String.IsNullOrEmpty(source.Name))
            {
                return;
            }
            if (String.IsNullOrEmpty(ReceiveName))
            {
                throw new ArgumentException($"{nameof(ReceiveName)} can not be null or empty.");
            }
            NDIlib.source_t source_t = new NDIlib.source_t()
            {
                p_ndi_name = UTF.StringToUtf8(source.Name),
            };
            NDIlib.recv_create_v3_t recvDescription = new NDIlib.recv_create_v3_t()
            {
                source_to_connect_to = source_t,
                // for uyva
                color_format = NDIlib.recv_color_format_e.recv_color_format_fastest,
                // for rgba
                //color_format = NDIlib.recv_color_format_e.recv_color_format_RGBX_RGBA,
                bandwidth = NDIlib.recv_bandwidth_e.recv_bandwidth_highest,
                allow_video_fields = false,
                p_ndi_recv_name = UTF.StringToUtf8(ReceiveName),
            };

            // Create a new instance connected to this source.
            _recvInstancePtr = NDIlib.recv_create_v3(ref recvDescription);

            Marshal.FreeHGlobal(source_t.p_ndi_name);
            Marshal.FreeHGlobal(recvDescription.p_ndi_recv_name);

            // Did it work?
            System.Diagnostics.Debug.Assert(_recvInstancePtr != IntPtr.Zero, "Failed to create NDI receive instance.");
            if (_recvInstancePtr == IntPtr.Zero)
            {
                return;
            }
            // We are now going to mark this source as being on program output for tally purposes (but not on preview)
            SetTallyIndicators(true, false);

            // Start up a thread to receive on
            _receiveThread = new Thread(ReceiveThreadProc) { IsBackground = true, Name = "NdiOtaReceiveThread" };
            _receiveThread.Start();
        }

        public void Disconnect()
        {
            Debug.Log("Disconnect!");
            SetTallyIndicators(false, false);

            // check for a running thread
            if (_receiveThread != null)
            {
                // tell it to exit
                _exitThread = true;
                // wait for it to end
                _receiveThread.Join();
            }
            // Reset thread defaults
            _receiveThread = null;
            //_exitThread = false;

            // Destroy the receiver
            NDIlib.recv_destroy(_recvInstancePtr);
            _recvInstancePtr = IntPtr.Zero;
            // set function status to defaults
            IsPtz = false;
            IsRecordingSupported = false;
        }

        void ReleaseInstanceObject()
        {
            Destroy(texture);
        }

        private void SetTallyIndicators(bool onProgram, bool onPreview)
        {
            if (_recvInstancePtr == IntPtr.Zero)
            {
                return;
            }

            NDIlib.tally_t tallyState = new NDIlib.tally_t()
            {
                on_program = onProgram,
                on_preview = onPreview,
            };

            NDIlib.recv_set_tally(_recvInstancePtr, ref tallyState);
        }

        private void ReceiveThreadProc()
        {
            while (!_exitThread && _recvInstancePtr != IntPtr.Zero)
            {
                // The descriptors.
                NDIlib.video_frame_v2_t videoFrame = new NDIlib.video_frame_v2_t();
                NDIlib.audio_frame_v2_t audioFrame = new NDIlib.audio_frame_v2_t();
                NDIlib.metadata_frame_t metadataFrame = new NDIlib.metadata_frame_t();
                switch (NDIlib.recv_capture_v2(_recvInstancePtr, ref videoFrame, ref audioFrame, ref metadataFrame, 500))
                {
                    // No data.
                    case NDIlib.frame_type_e.frame_type_none:
                        // No data received
                        break;

                    // Frame settings - check for extended functionality.
                    case NDIlib.frame_type_e.frame_type_status_change:
                        // check for PTZ
                        IsPtz = NDIlib.recv_ptz_is_supported(_recvInstancePtr);
                        IsRecordingSupported = NDIlib.recv_recording_is_supported(_recvInstancePtr);
                        IsRecordingSupported = NDIlib.recv_recording_is_supported(_recvInstancePtr);
                        break;

                    case NDIlib.frame_type_e.frame_type_video:
                        // If not enabled, just discard
                        // this can also occasionally happen when changing sources.
                        if (!_videoEnabled || videoFrame.p_data == IntPtr.Zero)
                        {
                            // always free received frames.
                            NDIlib.recv_free_video_v2(_recvInstancePtr, ref videoFrame);
                            break;
                        }
                        // We need to be on the UI thread to write to our texture.
                        // Not very efficient, but this is just an example.
                        _mainThreadContext.Post(d =>
                        {
                        // get all our info so that we can free the frame
                            int yres = videoFrame.yres;
                            int xres = videoFrame.xres;
                            //int stride = videoFrame.line_stride_in_bytes;
                            //int bufferSize = yres * stride;
                            //int bufferSize = yres * xres * 4;
                            //Debug.Log(yres + "/" + xres);
                            //Debug.Log(bufferSize);
                            //Debug.Log(videoFrame.line_stride_in_bytes);
                            //if (_texture == null)
                            //{
                            //    _texture = new Texture2D(xres, yres, TextureFormat.RGBA32, false);
                            //    _image.texture = _texture;
                            //}
                            //Debug.Log("come ok!!!");

                            if (_exitThread)
                            {
                                Debug.Log("Pre finish!!!!!!!!");
                                return;
                            }

                            //test
                            //var bytes = new byte[256 * 256 * 4];
                            //Marshal.Copy(videoFrame.p_data, bytes, 0, bytes.Length);
                            //Debug.Log("kokoko!!");
                            //StreamWriter sw = new StreamWriter("../TextData.txt", false);
                            //for (int i = 0; i < bytes.Length; i++)
                            //{
                            //    Debug.Log(bytes[i]);
                            //    sw.WriteLine(bytes[i]);
                            //}
                            //sw.Flush();
                            //sw.Close();
                            //Debug.Log("Finish!!!!!");
                            //Application.Quit();
                            //UnityEditor.EditorApplication.isPlaying = false;
                            //return;
                            //throw new SystemException();
                            //Debug.Log("Finish!!!!!");

                            //Destroy(_image.texture);
                            if (texture == null)
                            {
                                texture = new RenderTexture(xres, yres, 0);
                                texture.Create();
                            }
                            texture.Release();
                            texture = _formatConverter.Decode(xres, yres, videoFrame.p_data);
                            //_image.texture = texture;

                            //_texture.LoadRawTextureData(videoFrame.p_data, bufferSize);
                            //_texture.Apply();

                            //String metadata = UTF.Utf8ToString(videoFrame.p_data, (uint)videoFrame.length - 1);
                            if (videoFrame.p_metadata != IntPtr.Zero)
                                metadatastr = Marshal.PtrToStringAnsi(videoFrame.p_metadata);
                            else
                                metadatastr = "metadata is null";
                            NDIlib.recv_free_video_v2(_recvInstancePtr, ref videoFrame);
                        }, null);

                        break;

                    // Audio is beyond the scope of this example
                    case NDIlib.frame_type_e.frame_type_audio:
                        // if not audio or disabled, nothing to do.
                        if (!_audioEnabled || audioFrame.p_data == IntPtr.Zero || audioFrame.no_samples == 0)
                        {
                            // Always free received frames.
                            NDIlib.recv_free_audio_v2(_recvInstancePtr, ref audioFrame);
                            break;
                        }

                        // if the audio format changed, we need to reconfigure the audio device.
                        bool formatChanged = false;

                        // make sure our format has been created and matches the incomming audio.
                        // NOTE: Setup WaveAudio that is defined in NAudio.dll.

                        NDIlib.recv_free_audio_v2(_recvInstancePtr, ref audioFrame);
                        break;

                    // Metadata
                    case NDIlib.frame_type_e.frame_type_metadata:
                        // UTF-8 strings must be converted for use - length includes the terminating zero
                        // String metadata = Utf8ToString(metadataFrame.p_data, metadataFrame.length - 1);
                        // System.Diagnotics.Debug.Print(metadata);

                        bgmetadatastr = Marshal.PtrToStringAnsi(metadataFrame.p_data);
                                                
                        //Debug.Log(bgmetadatastr);
                        // free frames that were received.
                        NDIlib.recv_free_metadata(_recvInstancePtr, ref metadataFrame);
                        break;
                }
            }
        }
    }
}