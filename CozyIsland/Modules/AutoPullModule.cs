using CozyIsland.Utils;
using InteractSystem;
using UnityEngine;

namespace CozyIsland.Modules
{
    internal class AutoPullModule
    {
        private static AutoPullModule _instance;
        public static AutoPullModule Instance => _instance ??= new AutoPullModule();

        private AutoPullModule() { }

        public enum PullMode
        {
            Disabled,           // 不自动拔出
            PickToHand,        // 拔出到手上
            PickAndStore       // 拔出到手上并收起
        }

        public PullMode currentMode = PullMode.Disabled;
        public float checkRadius = 3.0f;
        public float checkInterval = 0.5f;
        private float checkTimer = 0f;

        public void Update()
        {
            if (!GameData.Instance.IsInGame) return;

            if (currentMode == PullMode.Disabled) return;

            checkTimer += Time.deltaTime;
            if (checkTimer < checkInterval) return;
            checkTimer = 0f;

            TryAutoPull();
        }

        private void TryAutoPull()
        {
            var playerPos = GameData.Instance.LocalPlayer.Position;
            var player = GameData.Instance.LocalPlayer;

            var picker = player.GetComponent<PBPicker>();
            if (picker == null)
            {
                picker = player.GetComponentInChildren<PBPicker>();
            }

            if (picker != null && picker.isHolding)
            {
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(playerPos, checkRadius);

            foreach (var col in colliders)
            {
                var pullable = col.GetComponentInParent<PullableObject>();
                if (pullable == null) continue;
                if (pullable.isBePulling) continue;
                if (pullable.pullOutProcess >= 1f) continue;
                if (!pullable.enabled || !pullable.gameObject.activeInHierarchy) continue;

                LoggerHelper.Info($"[AutoPull] 自动拔出: {pullable.name}");

                var dummyPuller = player.GetComponent<PCDPuller>();
                if (dummyPuller != null)
                {
                    pullable.StartPullBy(dummyPuller);

                    pullable.pullOutProcess = 1f;
                    var scaleX = pullable.transform.lossyScale.x;
                    var pullOutTimeCountField = typeof(PullableObject).GetField("pullOutTimeCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (pullOutTimeCountField != null)
                    {
                        pullOutTimeCountField.SetValue(pullable, pullable.curPullSetting.pullOutTime * scaleX + 0.01f);
                    }

                    pullable.PullOut();

                    if (picker != null)
                    {
                        IPickable targetPickable = null;

                        if (pullable.pulloutPickOPrefab != null)
                        {
                            GameObject pickedItem = SpawnMgr.SpawnGameObject(
                                pullable.pulloutPickOPrefab,
                                col.transform.position + Vector3.up * 1f,
                                Quaternion.identity,
                                null,
                                false
                            );

                            pickedItem.transform.localScale = pullable.transform.localScale;

                            targetPickable = pickedItem.GetComponent<PickableObject>();
                            if (targetPickable == null)
                            {
                                LoggerHelper.Warn($"[AutoPull] 无法找到 {pickedItem.name} 上的PickableObject组件");
                            }
                            else
                            {
                                // LoggerHelper.Info($"[AutoPull] 使用SpawnMgr生成并准备拾取: {pickedItem.name}");
                            }
                        }
                        else
                        {
                            targetPickable = pullable;
                            LoggerHelper.Info($"[AutoPull] 直接准备拾取原物体: {pullable.name}");
                        }

                        if (targetPickable != null)
                        {
                            bool pickSuccess = picker.Pick(targetPickable);
                            if (pickSuccess)
                            {
                                var resourceItem = (targetPickable as Component).GetComponent<ResourceItem>();
                                if (resourceItem != null)
                                {
                                    EasyEvent.TriggerEvent("Pick[" + resourceItem.resourceName + "]", (targetPickable as Component).transform);
                                    EasyEvent.TriggerEvent("Pick[" + resourceItem.resourceName + "]");
                                    EasyEvent.TriggerEvent("PickAnyEvent", (targetPickable as Component).transform);
                                }
                                // LoggerHelper.Info($"[AutoPull] 成功拾取到手上: {(targetPickable as Component).name}");

                                if (currentMode == PullMode.PickAndStore)
                                {
                                    TryStoreToBackpack(targetPickable as Component, picker);
                                }
                            }
                            else
                            {
                                LoggerHelper.Warn($"[AutoPull] 拾取失败: {(targetPickable as Component).name}");
                            }
                        }
                    }
                    else
                    {
                        LoggerHelper.Warn("[AutoPull] 找不到玩家的PBPicker组件，无法模拟拾取");
                    }

                    pullable.RestPull();
                }
                else
                {
                    LoggerHelper.Warn("[AutoPull] 找不到PCDPuller，无法自动拔出");
                }

                return;
            }
        }

        private void TryStoreToBackpack(Component itemComponent, PBPicker picker)
        {
            if (itemComponent == null) return;

            var backpackItem = itemComponent.GetComponent<CozyBackpackItem>();
            if (backpackItem == null)
            {
                LoggerHelper.Warn($"[AutoPull] {itemComponent.name} 不是背包物品，无法收起");
                return;
            }

            if (!CozyBackpackMgr.IsEnabled())
            {
                LoggerHelper.Warn("[AutoPull] 背包系统未启用");
                return;
            }

            var cozyPlayer = picker.GetComponent<CozyPlayer>();
            if (cozyPlayer == null || string.IsNullOrEmpty(cozyPlayer.playerName))
            {
                LoggerHelper.Warn("[AutoPull] 找不到玩家信息");
                return;
            }

            CozyBackpackMgr.PutItemIntoBackpack(cozyPlayer.playerName, backpackItem, false);
            // LoggerHelper.Info($"[AutoPull] 已收起到背包: {itemComponent.name}");
        }

        public void OnGUI()
        {
            GUILayout.Label("自动拔出模式", GUILayout.Height(25));

            PullMode selected = currentMode;

            if (GUILayout.Toggle(selected == PullMode.Disabled, "不自动拔出", GUILayout.Height(25)))
                selected = PullMode.Disabled;

            if (GUILayout.Toggle(selected == PullMode.PickToHand, "拔出到手上", GUILayout.Height(25)))
                selected = PullMode.PickToHand;

            if (GUILayout.Toggle(selected == PullMode.PickAndStore, "拔出并收起", GUILayout.Height(25)))
                selected = PullMode.PickAndStore;

            if (selected != currentMode)
            {
                currentMode = selected;
                switch (currentMode)
                {
                    case PullMode.Disabled:
                        LoggerHelper.Info("[AutoPull] 已禁用自动拔出");
                        break;
                    case PullMode.PickToHand:
                        LoggerHelper.Info("[AutoPull] 已启用：拔出到手上");
                        break;
                    case PullMode.PickAndStore:
                        LoggerHelper.Info("[AutoPull] 已启用：拔出并收起");
                        break;
                }
            }
        }

    }
}




