using CozyIsland.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using LightReflectiveMirror;
using Mirror;


namespace CozyIsland.Modules
{
    internal class PlayerModule
    {
        private static PlayerModule _instance;
        public static PlayerModule Instance => _instance ??= new PlayerModule();

        private PlayerModule() { }

        private bool tp = false;
        private bool getPlayer = false;

        public void Update()
        {
            GameData.Instance.UpdatePlayerList();

            if (tp)
            {
                TryTeleport();
                tp = false;
            }

            if (getPlayer)
            {
                TryGetPlayers();
                getPlayer = false;
            }
        }

        public void OnGUI()
        {
            tp = GUILayout.Button("传送到 (0,50,0)");
            getPlayer = GUILayout.Button("获取所有玩家位置");
        }

        private void TryTeleport()
        {
            if (!GameData.Instance.IsInGame)
            {
                LoggerHelper.Warn("不在游戏中，无法传送");
                return;
            }

            try
            {
                Vector3 targetPosition = new Vector3(0, 50, 0);
                GameData.Instance.LocalPlayer.TeleportTo(targetPosition);
                LoggerHelper.Info($"传送到 {targetPosition}");
            }
            catch (Exception e)
            {
                LoggerHelper.Error($"传送失败: {e}");
            }
        }

        private void TryGetPlayers()
        {
            try
            {
                List<string> playerNames = PlayerDictionary.GetAllPlayers();
                foreach (var name in playerNames)
                {
                    Transform t = PlayerDictionary.TryGetPlayer(name);
                    if (t != null)
                        LoggerHelper.Info($"玩家: {name}, 名字：{t.GetComponent<CozyPlayer>().playerShowName}， 位置: {t.position}");
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Error($"获取玩家失败: {e}");
            }
        }
    }
}
