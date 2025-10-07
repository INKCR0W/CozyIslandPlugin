using UnityEngine;


namespace CozyIsland.Modules
{
    internal class TeleportHelper
    {
        public static void TeleportPlayerTo(GameObject player, Vector3 target)
        {
            if (player == null) return;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                CozyPlayerTeleportHelper.TeleportPlayerTo(player, target);
            }
        }
    }
}
