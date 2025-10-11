using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using CozyIsland.HarmonyPatches;
using CozyIsland.Modules;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CozyIsland
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private bool showGUI = false;
        private Rect windowRect = new(100, 100, 500, 300);
        private Rect chatWindowRect = new(100, 100, 500, 300);

        private bool wasCursorVisible = false;
        private CursorLockMode previousCursorLockState;

        private Canvas canvas;

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

        private void Start()
        {
            ShowToast("混混沌沌小岛时光加载成功！ F1打开菜单 F2打开聊天记录 F5重置菜单", 5);
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
            ChatLogger.Instance.Update();
            DestroyItem.Instance.Update();
        }

        private void OnGUI()
        {
            ChatLogger.Instance.OnGUI();

            if (!showGUI) return;

            windowRect = GUILayout.Window(
                114514,
                windowRect,
                WindowFunc,
                "混混沌沌小岛时光"
            );

        }

        public void ShowToast(string message, float duration = 3f)
        {
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("ToastCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                canvasGO.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasGO);
            }

            StartCoroutine(ShowToastCoroutine(message, duration));
        }

        private IEnumerator ShowToastCoroutine(string message, float duration)
        {
            GameObject textGO = new GameObject("ToastText");
            textGO.transform.SetParent(canvas.transform);
            var text = textGO.AddComponent<Text>();
            text.text = message;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 24;
            text.color = Color.red;
            text.alignment = TextAnchor.UpperLeft;

            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 24;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -50);
            rect.sizeDelta = new Vector2(800, 50);

            Canvas.ForceUpdateCanvases();
            text.SetAllDirty();

            yield return null;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            yield return new WaitForSeconds(duration);
            if (textGO != null)
                GameObject.Destroy(textGO);
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
                    MiniMapTeleport.Instance.OnGUI();
                    DestroyItem.Instance.OnGUI();

                    //if (GUILayout.Button("获取房间"))
                    //{
                    //    Task.Run(async () =>
                    //    {
                    //        LoggerHelper.Info("正在请求房间列表...");
                    //        var rooms = await GetRooms.GetPublicLobbies();
                    //        LoggerHelper.Info($"已获取到 {rooms.Count} 个房间。");

                    //        foreach (var room in rooms)
                    //        {
                    //            LoggerHelper.Info(
                    //                $"房间: ID={room.LobbyId}, 玩家={room.MemberCount}/{room.MaxMembers}"
                    //            );
                    //        }
                    //    });
                    //}
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