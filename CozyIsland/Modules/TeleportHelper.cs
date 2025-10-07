using UnityEngine;


namespace CozyIsland.Modules
{
    internal class TeleportHelper
    {
        public static void TeleportPlayerTo(Transform player, Vector3 target)
        {
            if (player == null) return;

            player.TeleportPositionTo(target);

            //Rigidbody rb = player.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            //    CozyPlayerTeleportHelper.TeleportPlayerTo(player, target);

            //}
        }
    }
}
