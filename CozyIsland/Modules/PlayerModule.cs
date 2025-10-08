using CozyIsland.UI;
using CozyIsland.Utils;
using LightReflectiveMirror;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace CozyIsland.Modules
{
    internal class PlayerModule
    {
        private static PlayerModule _instance;
        public static PlayerModule Instance => _instance ??= new PlayerModule();
        private PlayerModule() { }

        private Vector2 scrollPos = Vector2.zero;
        private int selectedIndex = -1;

        public void Update()
        {
            GameData.Instance.UpdatePlayerList();

        }

        public void OnGUI()
        {
            if (!GameData.Instance.IsInGame)
            {
                GUILayout.Label("尚未进入游戏");
                return;
            }

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

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("传送到该玩家") && selectedIndex >= 0)
            {
                var target = players[selectedIndex];
                GameData.Instance.LocalPlayer.TeleportTo(new Vector3 (target.Position.x, target.Position.y + 2.0f, target.Position.z));
            }

            if (GUILayout.Button("视奸该玩家") && selectedIndex >= 0)
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
