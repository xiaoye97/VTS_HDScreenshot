﻿using System;
using BepInEx;
using System.IO;
using UnityEngine;
using System.Collections;
using InfoWindowNamespace;
using System.Reflection;
using System.Collections.Generic;

namespace VTS_HDScreenshot
{
    [BepInPlugin(GUID, PluginName, VERSION)]
    public class HDScreenshot : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.VTS_HDScreenshot";
        public const string PluginName = "VTS_HDScreenshot";
        public const string VERSION = "1.6.0";
        private CircleButtonController CircleButtonController;
        private RectTransform CircleButtonControllerRT;
        private Camera live2dCamera;
        private MethodInfo ShowInfoMethod;
        private List<object> showInfoOutherParams;

        private void Start()
        {
            InitShowInfo();
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
                GUILayout.BeginHorizontal($"HDScreenshot v{VERSION}", GUI.skin.window);
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
            FileInfo fileinfo = new FileInfo(fileName);
            if (!fileinfo.Directory.Exists)
            {
                fileinfo.Directory.Create();
            }
            File.WriteAllBytes(fileinfo.FullName, bytes);
            Debug.Log($"截图保存到了{fileinfo.FullName}");
            ShowInfo(string.Format(L.GetString("saved_screenshot_path", null), fileinfo.FullName));
        }

        private void InitShowInfo()
        {
            Type type = typeof(InfoWindow);
            ShowInfoMethod = type.GetMethod("ShowInfo");
            var ps = ShowInfoMethod.GetParameters();
            showInfoOutherParams = new List<object>();
            for (int i = 1; i < ps.Length; i++)
            {
                var p = ps[i];
                if (p.HasDefaultValue)
                {
                    showInfoOutherParams.Add(p.DefaultValue);
                }
                else
                {
                    if (p.ParameterType == typeof(string))
                    {
                        showInfoOutherParams.Add("");
                    }
                    else if (p.ParameterType == typeof(bool))
                    {
                        showInfoOutherParams.Add(false);
                    }
                    else if (p.ParameterType == typeof(int))
                    {
                        showInfoOutherParams.Add(0);
                    }
                    else if (p.ParameterType == typeof(float))
                    {
                        showInfoOutherParams.Add(0);
                    }
                    else if (p.ParameterType == typeof(double))
                    {
                        showInfoOutherParams.Add(0);
                    }
                }
            }
        }

        private void ShowInfo(string message)
        {
            var infoWindow = GameObject.FindObjectOfType<InfoWindow>();
            var paramList = new List<object>();
            paramList.Add(message);
            paramList.AddRange(showInfoOutherParams);
            if (infoWindow != null)
            {
                ShowInfoMethod.Invoke(infoWindow, paramList.ToArray());
            }
        }
    }
}