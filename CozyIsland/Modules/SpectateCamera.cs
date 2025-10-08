using Cinemachine;
using CozyIsland.Utils;
using UnityEngine;

namespace CozyIsland.Modules
{
    internal class SpectateCamera
    {
        private static SpectateCamera _instance;
        public static SpectateCamera Instance => _instance ??= new SpectateCamera();

        private CinemachineVirtualCamera activeVcam;
        private Transform originalFollow;
        private Player currentTarget;

        public bool IsSpectating => currentTarget != null;

        public void Update()
        {
            if (activeVcam != null) return;

            var go = GameObject.Find("NormalCamera");
            if (go == null) return;

            activeVcam = go.GetComponent<CinemachineVirtualCamera>();
            if (activeVcam == null) return;

            originalFollow = activeVcam.Follow;
            LoggerHelper.Info($"[Spectate] 已绑定 NormalCamera，原始 Follow={originalFollow}");
        }

        public void BeginWatch(Player target)
        {
            if (target == null || !target.Active) return;
            if (activeVcam == null)
            {
                LoggerHelper.Warn("没有可用的 VirtualCamera，无法旁观");
                return;
            }

            originalFollow = activeVcam.Follow;

            currentTarget = target;
            activeVcam.Follow = target.Data;
            LoggerHelper.Info($"开始旁观玩家：{target.Name}");
        }

        public void StopWatch()
        {
            if (activeVcam != null && originalFollow != null)
                activeVcam.Follow = GameData.Instance.LocalPlayer.Data;

            currentTarget = null;
            LoggerHelper.Info("停止旁观");
        }
    }
}