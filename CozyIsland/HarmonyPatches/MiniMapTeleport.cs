using CozyIsland.Utils;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CozyIsland.HarmonyPatches
{
    internal class MiniMapTeleport
    {
        private static MiniMapTeleport _instance;
        public static MiniMapTeleport Instance => _instance ??= new MiniMapTeleport();
        private MiniMapTeleport() { }

        public bool enabled = false;
        public bool IsEnabled => enabled;

        public void OnGUI()
        {
            enabled = GUILayout.Toggle(enabled, "Ctrl+点击小地图传送");
        }
    }

    [HarmonyPatch(typeof(CozyMiniMapV2), "OnPointerDown")]
    public class OnPointerDownPatch
    {
        static bool Prefix(CozyMiniMapV2 __instance, PointerEventData eventData)
        {
            if (MiniMapTeleport.Instance.IsEnabled && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                TeleportToClickPosition(__instance, eventData);
                return false;
            }

            return true;
        }

        static void TeleportToClickPosition(CozyMiniMapV2 miniMap, PointerEventData eventData)
        {
            try
            {
                var CozyPlayer = GameData.Instance.LocalPlayer;
                Transform player = CozyPlayer.Data;
                if (player == null)
                {
                    LoggerHelper.Warn("Player not found!");
                    return;
                }

                RectTransform miniMapRect = miniMap.miniMapObject.GetComponent<RectTransform>();
                if (miniMapRect == null)
                {
                    LoggerHelper.Warn("MiniMap RectTransform not found!");
                    return;
                }

                float miniMapScale = GetFieldValue<float>(miniMap, "miniMapScale");
                Vector2 miniMapActualOffset = GetFieldValue<Vector2>(miniMap, "miniMapActualOffset");
                Vector2 baseOffset = GetFieldValue<Vector2>(miniMap, "baseOffset");
                float miniMapScaleToWorld = GetFieldValue<float>(miniMap, "miniMapScaleToWorld");

                Vector3 playerWorldPos = player.position;


                Vector2 localPoint;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    miniMapRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint))
                {
                    LoggerHelper.Warn("Failed to convert screen point to local point!");
                    return;
                }


                Vector2 miniMapPos = localPoint;
                Vector2 worldPos2D = (miniMapPos - baseOffset) / miniMapScaleToWorld;
                Vector3 worldPos = new(worldPos2D.x, playerWorldPos.y + 2.0f, worldPos2D.y);

                CozyPlayer.TeleportTo(worldPos);
            }
            catch (System.Exception e)
            {
                LoggerHelper.Error($"Teleport failed: {e.Message}\n{e.StackTrace}");
            }
        }

        static T GetFieldValue<T>(object obj, string fieldName)
        {
            FieldInfo field = obj.GetType().GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                return (T)field.GetValue(obj);
            }

            LoggerHelper.Warn($"Field {fieldName} not found!");
            return default(T);
        }
    }
}