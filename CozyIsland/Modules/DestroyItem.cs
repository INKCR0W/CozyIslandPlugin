using CozyIsland.Utils;
using UnityEngine;

namespace CozyIsland.Modules
{
    internal class DestroyItem
    {
        private static DestroyItem _instance;
        public static DestroyItem Instance => _instance ??= new DestroyItem();
        private DestroyItem() { }

        private bool enable = false;
        private bool IsEnabled => enable;

        public float checkInterval = 0.1f;
        private float checkTimer = 0f;

        public void Update()
        {
            if (!GameData.Instance.IsInGame) return;

            if (!IsEnabled) return;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                checkTimer += Time.deltaTime;
                if (checkTimer < checkInterval) return;
                checkTimer = 0f;

                TryDestroyItems();
            }
        }

        public void OnGUI()
        {
            enable = GUILayout.Toggle(enable, "按住CTRL销毁周围手持相同物品");
        }

        private string GetHoldingItemName()
        {
            var player = GameData.Instance.LocalPlayer;
            var picker = player.GetComponent<PBPicker>();
            if (picker == null)
            {
                picker = player.GetComponentInChildren<PBPicker>();
            }

            if (picker != null && picker.isHolding && picker.holdingObj != null)
            {
                string name = picker.holdingObj.name;
                int lastParen = name.LastIndexOf('(');

                if (lastParen == -1)
                    return name;

                return name.Substring(0, lastParen);
            }
            return null;
        }

        void TryDestroyItems()
        {
            string itemName = GetHoldingItemName();

            if (string.IsNullOrEmpty(itemName))
                return;

            var player = GameData.Instance.LocalPlayer;

            var picker = player.GetComponent<PBPicker>();

            if (picker == null)
                picker = player.GetComponentInChildren<PBPicker>();

            GameObject holdingObj = picker.holdingObj;

            var playerPos = player.Position;
            Collider[] colliders = Physics.OverlapSphere(playerPos, 3.0f);

            foreach (var col in colliders)
            {
                if (col.gameObject == holdingObj)
                    continue;

                if (col.name.Contains(itemName))
                {
                    GameObject.Destroy(col.gameObject);
                }
            }
        }


    }
}
