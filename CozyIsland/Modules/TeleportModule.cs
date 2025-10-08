using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CozyIsland.Utils;
using UnityEngine;

namespace CozyIsland.Modules
{
    internal class TeleportModule
    {
        private static TeleportModule _instance;
        public static TeleportModule Instance => _instance ??= new TeleportModule();

        private TeleportModule()
        {
            InitializePersistence();
            LoadWaypoints();
        }

        private Vector2 scrollPosition = Vector2.zero;
        private string newWaypointName = "";

        private string waypointsFilePath;
        private const string WAYPOINTS_FILENAME = "waypoints.json";

        [Serializable]
        public class Waypoint
        {
            public string Name;
            public Vector3 Position;
            public string SceneName;
            public DateTime CreatedTime;

            public Waypoint(string name, Vector3 position, string sceneName)
            {
                Name = name;
                Position = position;
                SceneName = sceneName;
                CreatedTime = DateTime.Now;
            }
        }

        [Serializable]
        public class WaypointsData
        {
            public WaypointSerializable[] Waypoints;
        }

        [Serializable]
        public class WaypointSerializable
        {
            public string Name;
            public float PosX;
            public float PosY;
            public float PosZ;
            public string SceneName;
            public string CreatedTime;

            public WaypointSerializable() { }

            public WaypointSerializable(Waypoint waypoint)
            {
                Name = waypoint.Name;
                PosX = waypoint.Position.x;
                PosY = waypoint.Position.y;
                PosZ = waypoint.Position.z;
                SceneName = waypoint.SceneName;
                CreatedTime = waypoint.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss");
            }

            public Waypoint ToWaypoint()
            {
                var waypoint = new Waypoint(Name, new Vector3(PosX, PosY, PosZ), SceneName);
                if (DateTime.TryParse(CreatedTime, out DateTime time))
                {
                    waypoint.CreatedTime = time;
                }
                return waypoint;
            }
        }

        private List<Waypoint> waypoints = new List<Waypoint>();

        private void InitializePersistence()
        {
            try
            {
                string configPath = BepInEx.Paths.ConfigPath;

                string pluginConfigDir = Path.Combine(configPath, "Crow");
                if (!Directory.Exists(pluginConfigDir))
                {
                    Directory.CreateDirectory(pluginConfigDir);
                    LoggerHelper.Info($"创建配置目录: {pluginConfigDir}");
                }

                waypointsFilePath = Path.Combine(pluginConfigDir, WAYPOINTS_FILENAME);
                LoggerHelper.Info($"路径点文件路径: {waypointsFilePath}");
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"初始化持久化路径失败: {ex.Message}");
            }
        }
        private void LoadWaypoints()
        {
            try
            {
                if (string.IsNullOrEmpty(waypointsFilePath))
                {
                    LoggerHelper.Warn("路径点文件路径未初始化");
                    return;
                }

                if (!File.Exists(waypointsFilePath))
                {
                    LoggerHelper.Info("路径点文件不存在，将在添加路径点后自动创建");
                    return;
                }

                string json = File.ReadAllText(waypointsFilePath);

                WaypointsData data = JsonUtility.FromJson<WaypointsData>(json);

                if (data != null && data.Waypoints != null && data.Waypoints.Length > 0)
                {
                    LoggerHelper.Info($"JsonUtility反序列化成功，获得 {data.Waypoints.Length} 个路径点");
                    waypoints.Clear();
                    foreach (var serializableWaypoint in data.Waypoints)
                    {
                        waypoints.Add(serializableWaypoint.ToWaypoint());
                    }
                    LoggerHelper.Info($"成功加载 {waypoints.Count} 个路径点");
                }
                else
                {
                    LoggerHelper.Warn("JsonUtility反序列化失败，尝试手动解析");
                    waypoints.Clear();
                    ManualDeserialize(json);
                    LoggerHelper.Info($"手动解析完成，加载 {waypoints.Count} 个路径点");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"加载路径点失败: {ex.Message}\n{ex.StackTrace}");
                waypoints.Clear();
            }
        }
        private void ManualDeserialize(string json)
        {
            try
            {
                string[] lines = json.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                WaypointSerializable currentWaypoint = null;

                foreach (string line in lines)
                {
                    string trimmed = line.Trim();

                    if (trimmed == "{" && currentWaypoint == null)
                    {
                        currentWaypoint = new WaypointSerializable();
                    }
                    else if ((trimmed == "}" || trimmed == "},") && currentWaypoint != null)
                    {
                        if (!string.IsNullOrEmpty(currentWaypoint.Name))
                        {
                            waypoints.Add(currentWaypoint.ToWaypoint());
                            // LoggerHelper.Info($"手动解析路径点: {currentWaypoint.Name}");
                        }
                        currentWaypoint = null;
                    }

                    else if (currentWaypoint != null && trimmed.Contains(":"))
                    {
                        string[] parts = trimmed.Split(new[] { ':' }, 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim().Trim('"');
                            string value = parts[1].Trim().TrimEnd(',').Trim('"');

                            switch (key)
                            {
                                case "Name":
                                    currentWaypoint.Name = value;
                                    break;
                                case "PosX":
                                    float.TryParse(value, System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out currentWaypoint.PosX);
                                    break;
                                case "PosY":
                                    float.TryParse(value, System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out currentWaypoint.PosY);
                                    break;
                                case "PosZ":
                                    float.TryParse(value, System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out currentWaypoint.PosZ);
                                    break;
                                case "SceneName":
                                    currentWaypoint.SceneName = value;
                                    break;
                                case "CreatedTime":
                                    currentWaypoint.CreatedTime = value;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"手动解析JSON失败: {ex.Message}");
            }
        }
        private void SaveWaypoints()
        {
            try
            {
                if (string.IsNullOrEmpty(waypointsFilePath))
                {
                    LoggerHelper.Error("路径点文件路径未初始化，无法保存");
                    return;
                }

                LoggerHelper.Info($"当前路径点数量: {waypoints.Count}");

                WaypointsData data = new WaypointsData();
                data.Waypoints = new WaypointSerializable[waypoints.Count];

                for (int i = 0; i < waypoints.Count; i++)
                {
                    data.Waypoints[i] = new WaypointSerializable(waypoints[i]);
                    LoggerHelper.Info($"序列化路径点 {i}: {data.Waypoints[i].Name}");
                }

                string json = JsonUtility.ToJson(data, true);

                LoggerHelper.Info($"准备保存JSON ({json.Length} 字符): {json}");

                if (json == "{}" || string.IsNullOrEmpty(json))
                {
                    LoggerHelper.Warn("JsonUtility序列化失败，使用手动序列化");
                    json = ManualSerialize(data);
                    // LoggerHelper.Info($"手动序列化结果: {json}");
                }

                File.WriteAllText(waypointsFilePath, json);

                LoggerHelper.Info($"成功保存 {waypoints.Count} 个路径点到 {waypointsFilePath}");
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"保存路径点失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
        private string ManualSerialize(WaypointsData data)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("    \"Waypoints\": [");

            for (int i = 0; i < data.Waypoints.Length; i++)
            {
                var wp = data.Waypoints[i];
                sb.AppendLine("        {");
                sb.AppendLine($"            \"Name\": \"{wp.Name}\",");
                sb.AppendLine($"            \"PosX\": {wp.PosX},");
                sb.AppendLine($"            \"PosY\": {wp.PosY},");
                sb.AppendLine($"            \"PosZ\": {wp.PosZ},");
                sb.AppendLine($"            \"SceneName\": \"{wp.SceneName}\",");
                sb.AppendLine($"            \"CreatedTime\": \"{wp.CreatedTime}\"");
                sb.Append("        }");

                if (i < data.Waypoints.Length - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }

            sb.AppendLine("    ]");
            sb.Append("}");

            return sb.ToString();
        }

        public void Update()
        {
        }

        private void DrawWaypointItem(Waypoint waypoint, int index)
        {
            GUILayout.BeginVertical("box");

            GUILayout.Label($"{waypoint.Name}");
            GUILayout.Label($"位置: ({waypoint.Position.x:F1}, {waypoint.Position.y:F1}, {waypoint.Position.z:F1})");
            GUILayout.Label($"场景: {waypoint.SceneName}");
            GUILayout.Label($"创建: {waypoint.CreatedTime:yyyy-MM-dd HH:mm}");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("传送", GUILayout.Width(60)))
            {
                TeleportToWaypoint(waypoint);
            }

            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                waypoints.RemoveAt(index);
                SaveWaypoints();
                LoggerHelper.Info($"已删除路径点: {waypoint.Name}");
            }

            if (GUILayout.Button("更新", GUILayout.Width(60)))
            {
                UpdateWaypoint(waypoint);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        private void AddNewWaypoint()
        {
            if (string.IsNullOrEmpty(newWaypointName))
            {
                LoggerHelper.Warn("请输入路径点名称");
                return;
            }

            if (!GameData.Instance.IsInGame || GameData.Instance.LocalPlayer == null)
            {
                LoggerHelper.Warn("玩家不在游戏中，无法添加路径点");
                return;
            }

            if (waypoints.Any(w => w.Name == newWaypointName))
            {
                LoggerHelper.Warn($"路径点名称 '{newWaypointName}' 已存在");
                return;
            }

            Vector3 playerPosition = GameData.Instance.LocalPlayer.Position;
            string sceneName = GetCurrentSceneName();

            Waypoint newWaypoint = new Waypoint(newWaypointName, playerPosition, sceneName);
            waypoints.Add(newWaypoint);

            SaveWaypoints();

            LoggerHelper.Info($"已添加路径点: {newWaypointName} - 位置: {playerPosition}");

            newWaypointName = "";
        }

        private void TeleportToWaypoint(Waypoint waypoint)
        {
            if (!GameData.Instance.IsInGame || GameData.Instance.LocalPlayer == null)
            {
                LoggerHelper.Warn("玩家不在游戏中，无法传送");
                return;
            }

            try
            {
                GameData.Instance.LocalPlayer.TeleportTo(waypoint.Position);
                LoggerHelper.Info($"已传送到路径点: {waypoint.Name}");
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"传送失败: {ex.Message}");
            }
        }

        private void UpdateWaypoint(Waypoint waypoint)
        {
            if (!GameData.Instance.IsInGame || GameData.Instance.LocalPlayer == null)
            {
                LoggerHelper.Warn("玩家不在游戏中，无法更新路径点");
                return;
            }

            Vector3 newPosition = GameData.Instance.LocalPlayer.Position;
            waypoint.Position = newPosition;
            waypoint.SceneName = GetCurrentSceneName();
            waypoint.CreatedTime = DateTime.Now;

            SaveWaypoints();

            LoggerHelper.Info($"已更新路径点 '{waypoint.Name}' 到当前位置");
        }

        private string GetCurrentSceneName()
        {
            try
            {
                return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }
            catch
            {
                return "未知场景";
            }
        }
        public void ExportWaypoints(string filePath)
        {
            try
            {
                WaypointsData data = new WaypointsData();
                data.Waypoints = new WaypointSerializable[waypoints.Count];

                for (int i = 0; i < waypoints.Count; i++)
                {
                    data.Waypoints[i] = new WaypointSerializable(waypoints[i]);
                }

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(filePath, json);

                LoggerHelper.Info($"已导出 {waypoints.Count} 个路径点到: {filePath}");
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"导出路径点失败: {ex.Message}");
            }
        }

        public void ImportWaypoints(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    LoggerHelper.Warn($"文件不存在: {filePath}");
                    return;
                }

                string json = File.ReadAllText(filePath);
                WaypointsData data = JsonUtility.FromJson<WaypointsData>(json);

                if (data != null && data.Waypoints != null)
                {
                    int importedCount = 0;
                    foreach (var serializableWaypoint in data.Waypoints)
                    {
                        if (!waypoints.Any(w => w.Name == serializableWaypoint.Name))
                        {
                            waypoints.Add(serializableWaypoint.ToWaypoint());
                            importedCount++;
                        }
                    }

                    SaveWaypoints();
                    LoggerHelper.Info($"成功导入 {importedCount} 个路径点（跳过 {data.Waypoints.Count() - importedCount} 个重复）");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"导入路径点失败: {ex.Message}");
            }
        }
        public void OnGUI()
        {
            if (GameData.Instance.IsInGame && GameData.Instance.LocalPlayer != null)
            {
                GUILayout.Label($"当前位置: {GameData.Instance.LocalPlayer.Position}", GUI.skin.box);
                GUILayout.Space(5);
            }

            GUILayout.BeginHorizontal();
            newWaypointName = GUILayout.TextField(newWaypointName, GUILayout.Width(200));
            if (GUILayout.Button("新增路径点", GUILayout.Width(100)))
            {
                AddNewWaypoint();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label($"路径点列表 ({waypoints.Count}):");

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            for (int i = 0; i < waypoints.Count; i++)
            {
                DrawWaypointItem(waypoints[i], i);
            }

            if (waypoints.Count == 0)
            {
                GUILayout.Label("暂无路径点，使用上方功能添加");
            }

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存路径点"))
            {
                SaveWaypoints();
            }

            if (GUILayout.Button("重新加载"))
            {
                LoadWaypoints();
            }

            //if (GUILayout.Button("清除所有路径点"))
            //{
            //    if (waypoints.Count > 0)
            //    {
            //        waypoints.Clear();
            //        SaveWaypoints();
            //        LoggerHelper.Info("已清除所有路径点");
            //    }
            //}
            GUILayout.EndHorizontal();
        }
    }
}