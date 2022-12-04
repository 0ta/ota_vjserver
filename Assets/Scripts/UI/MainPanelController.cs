using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ota.ndi
{
    public class MainPanelController : MonoBehaviour
    {
        public RawImage NDIGreenImage;
        public RawImage NDIRedImage;
        public RawImage MusicGreenImage;
        public RawImage MusicRedImage;

        public TextMeshProUGUI NDISourceText;

        public MusicController MusicController;
        public NDIFinder NDIFinder;

        public GameObject MainPanel;    

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                if (mousePosition.x < 300 && mousePosition.y < 300)
                {
                    MainPanel.SetActive(!MainPanel.active);
                }
            }

            if (MusicController.volume != 0)
            {
                MusicGreenImage.enabled = true;
                MusicRedImage.enabled = false;
            } else
            {
                MusicGreenImage.enabled = false;
                MusicRedImage.enabled = true;
            }

            if (!string.IsNullOrWhiteSpace(NDIFinder.ConnectedNdiName))
            {
                NDIGreenImage.enabled = true;
                NDIRedImage.enabled = false;
                NDISourceText.text = NDIFinder.ConnectedNdiName;

            } else
            {
                NDIGreenImage.enabled = false;
                NDIRedImage.enabled = true;
            }

        }
    }
}

