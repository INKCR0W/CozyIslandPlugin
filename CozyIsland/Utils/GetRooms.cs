using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CozyIsland.Utils
{
    internal class GetRooms
    {
        public static async Task<List<LobbyInfo>> GetPublicLobbies()
        {
            var lobbies = new List<LobbyInfo>();

            if (!SteamClient.IsValid)
            {
                LoggerHelper.Info("Steam未初始化，无法获取房间列表");
                return lobbies;
            }

            try
            {
                var steamLobbies = await SteamMatchmaking.LobbyList
                    .WithMaxResults(50)
                    .WithSlotsAvailable(1)
                    .RequestAsync();

                if (steamLobbies != null)
                {
                    foreach (var lobby in steamLobbies)
                    {
                        lobbies.Add(new LobbyInfo
                        {
                            LobbyId = lobby.Id,
                            MemberCount = lobby.MemberCount,
                            MaxMembers = lobby.MaxMembers,
                            LobbyName = lobby.GetData("name") ?? "未命名房间",
                            GameMode = lobby.GetData("gameMode") ?? "未知模式",
                            MapName = lobby.GetData("map") ?? "未知地图",
                            HasPassword = !string.IsNullOrEmpty(lobby.GetData("password"))
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Info($"获取房间列表失败: {ex.Message}");
            }

            return lobbies;
        }

        public class LobbyInfo
        {
            public ulong LobbyId { get; set; }
            public string LobbyName { get; set; }
            public int MemberCount { get; set; }
            public int MaxMembers { get; set; }
            public string GameMode { get; set; }
            public string MapName { get; set; }
            public bool HasPassword { get; set; }
        }
    }
}