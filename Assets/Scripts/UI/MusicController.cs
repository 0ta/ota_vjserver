using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

namespace ota.ndi
{
    public class MusicController : MonoBehaviour
    {
        public AudioSource audioSource;  //インスペクタで AudioSource をセット
        //public string path = "C:\\otavjtmpmusic\\Protection.wav";
        public float volume = 0;
        public TMP_InputField  musicpathtext;

        private float[] _waveData = new float[1024];

        //外部からの呼び出し用メソッド
        public void StartAudio()
        {
            if (musicpathtext.text == null)
            {
                Debug.Log("File name is Null!!");
                return;
            }
            string path = musicpathtext.text;
            if (Path.GetExtension(path) == ".m4a")  //※"m4a"は再生できないっぽい
            {
                Debug.Log("Not supported audio format.");
                return;
            }

            StartCoroutine(LoadToAudioClipAndPlay(path));
        }

        //ファイルの読み込み（ダウンロード）と再生
        IEnumerator LoadToAudioClipAndPlay(string path)
        {
            if (audioSource == null || string.IsNullOrEmpty(path))
                yield break;

            if (!File.Exists(path))
            {
                //ここにファイルが見つからない処理
                Debug.Log("File not found.");
                yield break;
            }

            using (WWW www = new WWW("file://" + path))  //※あくまでローカルファイルとする
            {
                while (!www.isDone)
                    yield return null;

                AudioClip audioClip = www.GetAudioClip(false, true);
                if (audioClip.loadState != AudioDataLoadState.Loaded)
                {
                    //ここにロード失敗処理
                    Debug.Log("Failed to load AudioClip.");
                    yield break;
                }

                //ここにロード成功処理
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }

        public void StopAudio()
        {
            audioSource.Stop();
        }

        void Update()
        {
            audioSource.GetOutputData(_waveData, 1);
            volume = _waveData.Select(x => x * x).Sum() / _waveData.Length;
            //Debug.Log(volume);
        }
    }

}
