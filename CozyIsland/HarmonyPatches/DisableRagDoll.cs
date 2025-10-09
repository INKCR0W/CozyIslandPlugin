using CozyIsland.Utils;
using HarmonyLib;
using UnityEngine;

namespace CozyIsland.HarmonyPatches
{
    public class DisableRagDoll
    {
        private static DisableRagDoll _instance;
        public static DisableRagDoll Instance => _instance ??= new DisableRagDoll();

        private DisableRagDoll() { }

        private bool enabled = false;

        public bool IsEnabled => enabled;
        public void OnGUI()
        {
            enabled = GUILayout.Toggle(enabled, "不会摔倒");
        }

        public void Update()
        {
            if (!IsEnabled || !GameData.Instance.IsInGame) return;
            //SingletonMono<PBRagDollSwitcher>.Instance.SwitchToOriginal(); 
        }

    }

    [HarmonyPatch(typeof(PBRagDollSwitcher), nameof(PBRagDollSwitcher.SwitchToRagDoll))]
    internal static class PBRagDollSwitcher_SwitchToRagDoll_Patch
    {
        public static bool Prefix()
        {
            return !DisableRagDoll.Instance.IsEnabled;
        }
    }
}
