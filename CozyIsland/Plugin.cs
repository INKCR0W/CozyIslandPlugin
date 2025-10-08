using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using CozyIsland.HarmonyPatches;
using CozyIsland.Modules;
using CozyIsland.Utils;
using HarmonyLib;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace CozyIsland
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private bool showGUI = false;
        private Rect windowRect = new Rect(100, 100, 500, 300);

        private bool wasCursorVisible = false;
        private CursorLockMode previousCursorLockState;

        private enum Toolbar
        {
            Player,
            Teleport,
            Pull,
            Misc
        }
        private string[] ToolbarStrings = { "玩家", "传送", "采收", "杂项" };
        private static int ToolbarIndex = 0;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo(@"
  .oooooo.                                       
 d8P'  `Y8b                                      
888          oooo d8b  .ooooo.  oooo oooo    ooo 
888          `888""8P d88' `88b  `88. `88.  .8'  
888           888     888   888   `88..]88..8'   
`88b    ooo   888     888   888    `888'`888'    
 `Y8bood8P'  d888b    `Y8bod8P'     `8'  `8'     
");

            try
            {
                var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.LogInfo("Harmony补丁应用成功");
                Log.LogInfo($"插件 {MyPluginInfo.PLUGIN_GUID} 加载完成");
            }
            catch (System.Exception e)
            {
                Log.LogError($"插件加载失败: {e}");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showGUI = !showGUI;
                InputBlocker.Instance.IsGUIActive = showGUI;
                UpdateCursorState();
                Log.LogInfo($"GUI状态: {(showGUI ? "显示" : "隐藏")}");
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                windowRect = new Rect(100, 100, 500, 300);
                Log.LogInfo("窗口位置已重置");
            }

            PlayerModule.Instance.Update();
            TeleportModule.Instance.Update();
            AutoPullModule.Instance.Update();
            SpectateCamera.Instance.Update();
            DisableRagDoll.Instance.Update();
        }

        private void OnGUI()
        {
            if (!showGUI) return;

            windowRect = GUILayout.Window(
                114514,
                windowRect,
                WindowFunc,
                //"❤ 最爱阿凌了嘿嘿嘿 ❤"
                "混混沌沌小岛生活"
            );

        }

        private void WindowFunc(int windowID)
        {
            ToolbarIndex = GUILayout.Toolbar(ToolbarIndex, ToolbarStrings);

            switch (ToolbarIndex)
            {
                case (int)Toolbar.Player:
                    PlayerModule.Instance.OnGUI();
                    break;

                case (int)Toolbar.Teleport:
                    TeleportModule.Instance.OnGUI();
                    break;

                case (int)Toolbar.Pull:
                    AutoPullModule.Instance.OnGUI();
                    break;

                case (int)Toolbar.Misc:
                    AutoBoxVehicle.Instance.OnGUI();
                    DisableRagDoll.Instance.OnGUI();

                    if (GUILayout.Button("获取房间"))
                    {
                        Task.Run(async () =>
                        {
                            LoggerHelper.Info("正在请求房间列表...");
                            var rooms = await GetRooms.GetPublicLobbies();
                            LoggerHelper.Info($"已获取到 {rooms.Count} 个房间。");

                            foreach (var room in rooms)
                            {
                                LoggerHelper.Info(
                                    $"房间: ID={room.LobbyId}, 玩家={room.MemberCount}/{room.MaxMembers}"
                                );
                            }
                        });
                    }

                    break;
            }

            GUI.DragWindow();
        }

        private void UpdateCursorState()
        {
            if (showGUI)
            {
                wasCursorVisible = Cursor.visible;
                previousCursorLockState = Cursor.lockState;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = wasCursorVisible;
                Cursor.lockState = previousCursorLockState;
            }
        }

        private void OnDisable()
        {
            if (showGUI)
            {
                showGUI = false;
                InputBlocker.Instance.IsGUIActive = false;
                Cursor.visible = wasCursorVisible;
                Cursor.lockState = previousCursorLockState;
            }
        }

        private void OnDestroy()
        {
            if (showGUI)
            {
                showGUI = false;
                InputBlocker.Instance.IsGUIActive = false;
                Cursor.visible = wasCursorVisible;
                Cursor.lockState = previousCursorLockState;
            }
        }
    }
}