using UnityEngine;
using UnityEditor;

/// <summary>
/// ClearPlayer脚本的自定义Inspector编辑器
/// 在Inspector中显示按钮，方便清空玩家数据
/// </summary>
[CustomEditor(typeof(ClearPlayer))]
public class ClearPlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认Inspector（显示脚本引用等）
        DrawDefaultInspector();

        // 添加分隔线
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 显示说明文字
        EditorGUILayout.HelpBox("点击下方按钮清空玩家数据", MessageType.Info);

        // 获取目标对象
        ClearPlayer clearPlayer = (ClearPlayer)target;

        // 显示按钮
        EditorGUILayout.BeginHorizontal();

        // 清空所有数据按钮
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("清空所有玩家数据", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认清空", 
                "确定要清空所有玩家数据吗？\n这将删除所有存档数据，包括积分、已购买的物品等。", 
                "确定", "取消"))
            {
                clearPlayer.ClearAllData();
            }
        }

        // 只清空积分按钮
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("只清空积分", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认清空", 
                "确定要清空积分吗？\n这将只删除积分数据，保留已购买的物品。", 
                "确定", "取消"))
            {
                clearPlayer.ClearPoints();
            }
        }

        GUI.backgroundColor = Color.white;  // 恢复默认颜色
        EditorGUILayout.EndHorizontal();

        // 显示当前积分信息
        EditorGUILayout.Space();
        if (ScoreManager.Instance != null)
        {
            EditorGUILayout.HelpBox($"当前积分: {ScoreManager.Instance.currentPoints}", MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox("ScoreManager未初始化", MessageType.Warning);
        }
    }
}

