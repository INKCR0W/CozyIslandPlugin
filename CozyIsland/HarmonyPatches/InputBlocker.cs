using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CozyIsland.HarmonyPatches
{
    internal class InputBlocker
    {
        private static InputBlocker _instance;
        public static InputBlocker Instance => _instance ??= new InputBlocker();

        private InputBlocker() { }

        public bool IsGUIActive { get; set; }

        public static bool IsWhitelistedKey(KeyCode key)
        {
            return key == KeyCode.F1 || key == KeyCode.F5;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKey), typeof(KeyCode))]
    internal class Input_GetKey_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(KeyCode key, ref bool __result)
        {
            if (InputBlocker.Instance.IsGUIActive && !InputBlocker.IsWhitelistedKey(key))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), typeof(KeyCode))]
    internal class Input_GetKeyDown_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(KeyCode key, ref bool __result)
        {
            if (InputBlocker.Instance.IsGUIActive && !InputBlocker.IsWhitelistedKey(key))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), typeof(KeyCode))]
    internal class Input_GetKeyUp_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(KeyCode key, ref bool __result)
        {
            if (InputBlocker.Instance.IsGUIActive && !InputBlocker.IsWhitelistedKey(key))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButton), typeof(int))]
    internal class Input_GetMouseButton_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (InputBlocker.Instance.IsGUIActive)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonDown), typeof(int))]
    internal class Input_GetMouseButtonDown_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (InputBlocker.Instance.IsGUIActive)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp), typeof(int))]
    internal class Input_GetMouseButtonUp_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (InputBlocker.Instance.IsGUIActive)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.mouseScrollDelta), MethodType.Getter)]
    internal class Input_MouseScrollDelta_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Vector2 __result)
        {
            if (InputBlocker.Instance.IsGUIActive)
            {
                __result = Vector2.zero;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetAxis), typeof(string))]
    internal class Input_GetAxis_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result)
        {
            if (InputBlocker.Instance.IsGUIActive)
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetAxisRaw), typeof(string))]
    internal class Input_GetAxisRaw_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result)
        {
            if (InputBlocker.Instance.IsGUIActive)
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.anyKey), MethodType.Getter)]
    internal class Input_AnyKey_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (InputBlocker.Instance.IsGUIActive)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.anyKeyDown), MethodType.Getter)]
    internal class Input_AnyKeyDown_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (InputBlocker.Instance.IsGUIActive)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch]
    internal class NewInputSystem_Patch
    {
        static bool Prepare()
        {
            try
            {
                var type = Type.GetType("UnityEngine.InputSystem.InputActionState, Unity.InputSystem");
                if (type != null)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        static MethodBase TargetMethod()
        {
            try
            {
                var type = Type.GetType("UnityEngine.InputSystem.InputActionState, Unity.InputSystem");
                if (type != null)
                {
                    var method = type.GetMethod("ProcessControlStateChange",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (method != null)
                    {
                        Plugin.Log.LogInfo($"找到新Input System方法: {method.Name}");
                        return method;
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"无法找到新Input System方法: {e.Message}");
            }
            return null;
        }

        [HarmonyPrefix]
        static bool Prefix()
        {
            return !InputBlocker.Instance.IsGUIActive;
        }
    }
}