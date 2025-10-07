using CozyIsland.Modules;
using System;
using System.Collections.Generic;

using UnityEngine;

namespace CozyIsland.Utils
{
    internal class GameData
    {
        private static GameData _instance;
        public static GameData Instance => _instance ??= new GameData();
        private GameData() {}

        public Player LocalPlayer = null;
        public List<Player> PlayerList = [];

        public bool IsInGame => LocalPlayer.Active;


        public void UpdatePlayerList()
        {
            if (SingletonMono<PlayerIdentifier>.Instance == null) return;

            try
            {
                LocalPlayer = new Player(SingletonMono<PlayerIdentifier>.Instance?.GetPlayer());

                if (!IsInGame) return;

                PlayerList.Clear();

                List<string> playerNames = PlayerDictionary.GetAllPlayers();
                foreach (var name in playerNames)
                {
                    if (name == LocalPlayer.Id) continue;
                    PlayerList.Add(new Player(PlayerDictionary.GetPlayer(name, true)));
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Error($"获取玩家失败: {e}");
            }
        }
    }

    public class Player
    {
        private Transform PlayerData = null;
        private Player() {}

        public Player(Transform transform)
        {
            PlayerData = transform;
        }

        public bool Active => PlayerData != null;

        public CozyPlayer CozyPlayer => PlayerData?.GetComponent<CozyPlayer>();
        public Transform Data => PlayerData;

        public Vector3 Position => PlayerData?.position ?? Vector3.zero;
        public string Id => this.CozyPlayer?.playerName ?? string.Empty;
        public string Name => this.CozyPlayer?.playerShowName ?? string.Empty;
        public GameObject Skeleton => this.CozyPlayer?.pbSkeleton;

        public Transform Find(string path)
        {
            return PlayerData != null ? PlayerData.Find(path) : null;
        }

        public T GetComponent<T>() where T : Component
        {
            return PlayerData != null ? PlayerData.GetComponent<T>() : null;
        }

        public T GetComponentInChildren<T>() where T : Component
        {
            return PlayerData != null ? PlayerData.GetComponentInChildren<T>() : null;
        }

        public T GetComponentInParent<T>() where T : Component
        {
            return PlayerData != null ? PlayerData.GetComponentInParent<T>() : null;
        }

        public void TeleportTo(Vector3 target)
        {
            var player = this.PlayerData;

            if (player == null) return;

            TeleportHelper.TeleportPlayerTo(player, target);

            //var name = this.Name;

            //Rigidbody rb = player.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            //    CozyPlayerTeleportHelper.TeleportPlayerTo(player, target);
            //    LoggerHelper.Info($"传送玩家 {name} 到 {target}");
            //}
            //else
            //{
            //    LoggerHelper.Warn($"玩家 {name} 没有 Rigidbody 组件，无法传送");
            //}
        }
    }
}
