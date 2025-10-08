using CozyIsland.Utils;
using System.Xml.Linq;
using UnityEngine;


namespace CozyIsland.Modules
{
    internal class TeleportHelper
    {
        public static void TeleportPlayerTo(Player player, Vector3 target)
        {
            if (player == null) return;

            // player.TeleportPositionTo(target);

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                CozyPlayerTeleportHelper.TeleportPlayerTo(player.Object, target);
                LoggerHelper.Info($"传送玩家 {player.Name} 到 {target}");
            }
            else
            {
                LoggerHelper.Warn($"玩家 {player.Name} 没有 Rigidbody 组件，无法传送");
            }
        }
    }
}
