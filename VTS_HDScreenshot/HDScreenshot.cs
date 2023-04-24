using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;

namespace VTS_HDScreenshot
{
    [BepInPlugin(GUID, PluginName, VERSION)]
    public class HDScreenshot : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.VTS_HDScreenshot";
        public const string PluginName = "VTS_HDScreenshot";
        public const string VERSION = "1.1.0";
        private CircleButtonController CircleButtonController;
        private RectTransform CircleButtonControllerRT;
        private Camera live2dCamera;

        private void Start()
        {
        }

        private void Update()
        {
            if (CircleButtonControllerRT == null)
            {
                CircleButtonController = GameObject.FindObjectOfType<CircleButtonController>();
                if (CircleButtonController != null)
                {
                    CircleButtonControllerRT = CircleButtonController.transform as RectTransform;
                }
            }
        }

        private void OnGUI()
        {
            if (CircleButtonControllerRT != null && CircleButtonControllerRT.anchoredPosition.x >= 0)
            {
                GUILayout.Space(30);
                GUILayout.BeginHorizontal("HDScreenshot", GUI.skin.window);
                if (GUILayout.Button("1080P"))
                {
                    Capture(1920, 1080);
                }
                if (GUILayout.Button("2K"))
                {
                    Capture(2560, 1440);
                }
                if (GUILayout.Button("4K"))
                {
                    Capture(3840, 2160);
                }
                if (GUILayout.Button("8K"))
                {
                    Capture(7680, 4320);
                }
                if (GUILayout.Button("16K"))
                {
                    Capture(15360, 8640);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void Capture(int width, int height)
        {
            if (live2dCamera == null)
            {
                live2dCamera = GameObject.Find("Cameras/Live2D Camera").GetComponent<Camera>();
            }
            string savePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}/screenshots/vts-hd-{width}x{height}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.png";
            StartCoroutine(CaptureByCamera(live2dCamera, new Rect(0, 0, width, height), savePath));
        }

        private IEnumerator CaptureByCamera(Camera mCamera, Rect mRect, string fileName)
        {
            yield return new WaitForEndOfFrame();
            RenderTexture mRender = new RenderTexture((int)mRect.width, (int)mRect.height, 0);
            mCamera.targetTexture = mRender;
            mCamera.Render();
            RenderTexture.active = mRender;
            Texture2D mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.ARGB32, false);
            mTexture.ReadPixels(mRect, 0, 0);
            mTexture.Apply();
            mCamera.targetTexture = null;
            Destroy(mRender);
            byte[] bytes = mTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(fileName, bytes);
            Debug.Log($"截图保存到了{fileName}");
        }
    }
}