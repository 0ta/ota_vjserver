using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ota.ndi
{
    public class NDIFinder : MonoBehaviour
    {
        [SerializeField] private NDIReceiver _ndiReceiver = null;
        public string ConnectedNdiName = null;
        private IntPtr _findInstancePtr = IntPtr.Zero;

        private Thread _receiveThread = null;
        private bool _exitThread = false;
        private bool _stopFinder = false;

        private ObservableCollection<Source> _sourceList = new ObservableCollection<Source>();

        private void Awake()
        {
            if (!NDIlib.Initialize())
            {
                Debug.Log("Cannot run NDI.");
                return;
            }

            Debug.Log("Initialized NDI.");

            Find();
        }

        private void OnDestroy()
        {
            _stopFinder = true;
        }

        private void Find()
        {
            IntPtr groupsPtr = IntPtr.Zero;
            IntPtr extraIpsPtr = IntPtr.Zero;

            NDIlib.find_create_t findDesc = new NDIlib.find_create_t()
            {
                p_groups = groupsPtr,
                show_local_sources = true,
                p_extra_ips = extraIpsPtr,
            };

            // Create out find instance.
            _findInstancePtr = NDIlib.find_create_v2(ref findDesc);

            // Free out UTF-8 buffer if we created one.
            if (groupsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(groupsPtr);
            }

            if (extraIpsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(extraIpsPtr);
            }

            // Did it succeed?
            System.Diagnostics.Debug.Assert(_findInstancePtr != IntPtr.Zero, "Failed to create NDI find instance.");

            Task.Run(() => { SearchForWhile(1.0f); });
        }

        private void SearchForWhile(float minutes)
        {
            DateTime startTime = DateTime.Now;

            while (!_stopFinder && DateTime.Now - startTime < TimeSpan.FromMinutes(minutes))
            {
                if (!NDIlib.find_wait_for_sources(_findInstancePtr, 1000))
                {
                    Debug.Log("No change to the sources found.");
                    continue;
                }

                // Get the updated list of sources.
                uint numSources = 0;
                IntPtr p_sources = NDIlib.find_get_current_sources(_findInstancePtr, ref numSources);

                // Display all the sources.
                Debug.Log($"Network sources ({numSources} found).");

                // Continue if no device is found.
                if (numSources == 0)
                {
                    continue;
                }

                int sourceSizeInBytes = Marshal.SizeOf(typeof(NDIlib.source_t));

                for (int i = 0; i < numSources; i++)
                {
                    IntPtr p = IntPtr.Add(p_sources, (i * sourceSizeInBytes));

                    NDIlib.source_t src = (NDIlib.source_t)Marshal.PtrToStructure(p, typeof(NDIlib.source_t));

                    // .Net doesn't handle marshaling UTF-8 strings properly.
                    String name = UTF.Utf8ToString(src.p_ndi_name);

                    //　とりあえず。。。。
                    if (!name.Contains("otatestndi")) continue;
                    
                    
                    if (_sourceList.All(item => item.Name != name))
                    {
                        Source source = new Source(src);
                        _sourceList.Add(source);
                        Debug.Log(source.Name);
                        ConnectedNdiName = source.Name;
                        _ndiReceiver.Connect(source);
                    }
                }
            }
        }
    }
}
