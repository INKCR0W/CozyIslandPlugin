using Cinemachine;
using CozyIsland.Utils;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CozyIsland.Modules
{
    internal class SpectateCamera
    {
        private static SpectateCamera _instance;
        public static SpectateCamera Instance => _instance ??= new SpectateCamera();
        private SpectateCamera() { }

        private CozyCameraHandler Handler = SingletonMono<CozyCameraHandler>.Instance;
        private bool Inited = false;


        CinemachineVirtualCamera MyCamera;
        private Transform OriginalFollow;
        private bool OriginalEnabled;

        private Player currentTarget;
        public bool IsSpectating => currentTarget != null;

        void Update()
        {
            if (Handler == null)
                Handler = SingletonMono<CozyCameraHandler>.Instance;

            bool hasHandler = Handler != null;

            if (hasHandler == Inited)
                return;

            Inited = hasHandler;

            if (hasHandler)
            {
                MyCamera = Traverse.Create(Handler).Field<CinemachineVirtualCamera>("usedCamera").Value;
                OriginalFollow = MyCamera.Follow;
                OriginalEnabled = MyCamera.enabled;
            }
            else
            {
                MyCamera = null;
            }
        }

        void SetTarget(Transform target, bool enabled)
        {
            if (GameData.Instance.IsInGame == false)
            {
                LoggerHelper.Warn("玩家不在游戏中，无法视奸");
            }

            if (MyCamera == null)
            {
                LoggerHelper.Warn("摄像机未初始化，无法视奸");
                return;
            }

            MyCamera.Follow = target;
            MyCamera.enabled = enabled;
        }

        public void BeginWatch(Player target)
        {
            if (target == null || !target.Active) return;

            if (MyCamera == null)
            {
                LoggerHelper.Warn("摄像机未初始化，无法旁观");
                return;
            }

            currentTarget = target;
            MyCamera.Follow = target.Data;
            MyCamera.enabled = true;
            LoggerHelper.Info($"开始旁观玩家：{target.Name}");
        }

        public void StopWatch()
        {
            if (MyCamera == null) return;

            currentTarget = null;
            MyCamera.Follow = OriginalFollow;
            MyCamera.enabled = OriginalEnabled;
            LoggerHelper.Info("停止旁观");
        }
    }
}