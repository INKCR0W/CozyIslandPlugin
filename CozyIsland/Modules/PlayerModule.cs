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

        private Vector2 scrollPos = Vector2.zero;
        private int selectedIndex = -1;

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


        public void OnGUI()
        {
            if (!GameData.Instance.IsInGame)
            {
                GUILayout.Label("尚未进入游戏");
                return;
            }

            /* 玩家列表 */
            GUILayout.Label("在线玩家", GUI.skin.box);
            var players = GameData.Instance.PlayerList;
            if (players.Count == 0)
            {
                GUILayout.Label("暂无其他玩家");
                return;
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                string text = $"{p.Name}  ({p.Position.x:F1}, {p.Position.y:F1}, {p.Position.z:F1})";
                if (GUILayout.Toggle(selectedIndex == i, text, "Button"))
                    selectedIndex = i;
            }
            GUILayout.EndScrollView();
            if (selectedIndex >= 0 && selectedIndex < players.Count)
                GUILayout.Label($"当前选中：{players[selectedIndex].Name}");
            else
                GUILayout.Label("当前选中：无");

            /* 操作按钮 */
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("传送到该玩家") && selectedIndex >= 0)
            {
                var target = players[selectedIndex];
                GameData.Instance.LocalPlayer.TeleportTo(target.Position);
            }

            if (GUILayout.Button("旁观该玩家") && selectedIndex >= 0)
            {
                var target = players[selectedIndex];
                SpectateCamera.Instance.BeginWatch(target);
            }

            if (SpectateCamera.Instance.IsSpectating)
            {
                if (GUILayout.Button("停止旁观"))
                    SpectateCamera.Instance.StopWatch();
            }
            GUILayout.EndHorizontal();
        }
    }
}
