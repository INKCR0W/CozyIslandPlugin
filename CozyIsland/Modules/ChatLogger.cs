using BepInEx;
using BepInEx.Logging;
using CozyIsland.HarmonyPatches;
using CozyIsland.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CozyIsland.Modules
{
    internal class ChatLogger
    {
        private static ChatLogger _instance;
        public static ChatLogger Instance => _instance ??= new ChatLogger();
        private ChatLogger() { }

        private static readonly List<ChatEntry> ChatHistory = [];

        public Vector2 _scrollPos = Vector2.zero;
        private bool _showWindow = false;
        private const int MaxHistory = 1000;

        public class ChatEntry
        {
            public DateTime Time;
            public string PlayerName;
            public string Text;
            public bool IsLocal;

            public override string ToString()
            {
                return $"[{Time:HH:mm:ss}] {PlayerName}: {Text}";
            }
        }
        public static void AddChat(string player, string text, bool isLocal)
        {
            var entry = new ChatEntry
            {
                Time = DateTime.Now,
                PlayerName = player,
                Text = text,
                IsLocal = isLocal
            };

            ChatHistory.Add(entry);
            if (ChatHistory.Count > MaxHistory)
                ChatHistory.RemoveAt(0);

            LoggerHelper.Info(entry.ToString());
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
                _showWindow = !_showWindow;
        }
        public void OnGUI()
        {
            if (!_showWindow) return;

            GUILayout.BeginArea(new Rect(20, 150, 500, 320), GUI.skin.window);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                alignment = TextAnchor.UpperCenter
            };

            GUILayout.Label("聊天记录（按 F2 隐藏）", titleStyle);

            GUIStyle chatStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                wordWrap = true,
                richText = true,
                normal = { textColor = Color.black }
            };

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(480), GUILayout.Height(250));

            foreach (var entry in ChatHistory)
            {
                if (entry.IsLocal)
                {
                    continue;
                }

                string line = $"<color=#888888>[{entry.Time:HH:mm:ss}]</color> " +
                              $"<color=#4A90E2>{entry.PlayerName}</color>: {entry.Text}";
                GUILayout.Label(line, chatStyle);
            }

            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }
    }


    [HarmonyPatch(typeof(CozyPlayerChat))]
    class Patch_CozyPlayerChat
    {
        [HarmonyPostfix]
        [HarmonyPatch("UserCode_RpcPlayerSay__String__Single__Boolean")]
        static void Post_PlayerSay(CozyPlayerChat __instance, string text, float duration, bool isLocalOnly)
        {
            try
            {
                string playerName = TryGetPlayerShowName(__instance);
                ChatLogger.AddChat(playerName, text, isLocalOnly);
            }
            catch (Exception e)
            {
                LoggerHelper.Error($"[ChatLogger] Hook failed: {e}");
            }

            ChatLogger.Instance._scrollPos.y = Mathf.Infinity;
        }
        static string TryGetPlayerShowName(CozyPlayerChat instance)
        {
            try
            {
                var m = typeof(CozyPlayerChat).GetMethod("GetChatPlayerShowName", BindingFlags.NonPublic | BindingFlags.Instance);
                if (m != null)
                {
                    return (string)m.Invoke(instance, null);
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Warn($"[ChatLogger] GetChatPlayerShowName failed: {e.Message}");
            }
            return "未知玩家";
        }
    }
}
