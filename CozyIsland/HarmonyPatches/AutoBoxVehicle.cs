using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using CozyIsland.Utils;

namespace CozyIsland.HarmonyPatches
{
    public class AutoBoxVehicle
    {
        private static AutoBoxVehicle _instance;
        public static AutoBoxVehicle Instance => _instance ??= new AutoBoxVehicle();

        private AutoBoxVehicle() { }

        private bool autoBox = false;

        public bool IsEnabled => autoBox;
        public void OnGUI()
        {
            autoBox = GUILayout.Toggle(autoBox, "下车自动打包载具");
        }
    }

    [HarmonyPatch(typeof(PBVehicle), nameof(PBVehicle.KickOutRider))]
    internal static class PBVehicle_KickOutRider_Patch
    {
        private static HashSet<PBVehicle> _processingVehicles = new HashSet<PBVehicle>();

        public static void Postfix(PBVehicle __instance)
        {
            if (!AutoBoxVehicle.Instance.IsEnabled) return;

            if (_processingVehicles.Contains(__instance))
                return;

            _processingVehicles.Add(__instance);

            GameObject vehicleGO = __instance.gameObject;
            if (vehicleGO == null)
            {
                _processingVehicles.Remove(__instance);
                return;
            }

            CozyVehicleBoxHelper vehicleHelper = vehicleGO.GetComponent<CozyVehicleBoxHelper>();
            if (vehicleHelper == null)
            {
                LoggerHelper.Warn("[AutoBox] 载具没有 CozyVehicleBoxHelper 组件");
                _processingVehicles.Remove(__instance);
                return;
            }

            if (!CheckBoxCondition(vehicleGO))
            {
                LoggerHelper.Warn("[AutoBox] 打包条件不满足");
                _processingVehicles.Remove(__instance);
                return;
            }

            ExecuteBoxing(vehicleHelper);
            LoggerHelper.Info($"[AutoBox] {vehicleHelper.name}自动打包完成");

            _processingVehicles.Remove(__instance);
        }

        private static bool CheckBoxCondition(GameObject vehicleGO)
        {
            PBVehicle vehicle = vehicleGO.GetComponentInChildren<PBVehicle>(true);
            return vehicle == null || vehicle.rider == null;
        }

        private static void ExecuteBoxing(CozyVehicleBoxHelper helper)
        {
            if (helper.boxedVehiclePrefab == null)
            {
                LoggerHelper.Warn("[AutoBox] boxedVehiclePrefab 为空");
                return;
            }

            PBVehicle vehicleInChildren = helper.GetComponentInChildren<PBVehicle>(true);
            if (vehicleInChildren != null)
            {
                vehicleInChildren.cantBeRidden = true;
            }

            Vector3 position = helper.transform.position;
            Quaternion rotation = helper.transform.rotation;

            GameObject boxedVehicle = SpawnMgr.SpawnGameObject(
                helper.boxedVehiclePrefab,
                position,
                rotation,
                null,
                false
            );

            if (boxedVehicle != null)
            {
                LoggerHelper.Info($"[AutoBox] 已生成打包载具: {boxedVehicle.name}");

                GameObject vehicleObj = helper.vehicleObj != null ? helper.vehicleObj : helper.gameObject;
                Object.Destroy(vehicleObj);
            }
            else
            {
                LoggerHelper.Warn("[AutoBox] 生成打包载具失败");
            }
        }
    }
}