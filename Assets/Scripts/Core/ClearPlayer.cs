using UnityEngine;

public class ClearPlayer : MonoBehaviour
{
    [ContextMenu("清空所有玩家数据")]
    public void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        // 重置ScoreManager的内存数据
        ResetScoreManager();
        
        Debug.Log("? 所有玩家数据已清空！游戏将恢复到新用户状态。");
    }
    
    [ContextMenu("只清空积分")]
    public void ClearPoints()
    {
        PlayerPrefs.DeleteKey("PlayerPoints");
        PlayerPrefs.Save();
        
        // 重置ScoreManager的积分数据
        ResetScoreManager();
        
        Debug.Log("? 积分已清空！");
    }

    /// <summary>
    /// 重置ScoreManager的内存数据（重新从PlayerPrefs读取）
    /// </summary>
    void ResetScoreManager()
    {
        if (ScoreManager.Instance != null)
        {
            // 调用ScoreManager的重新初始化方法
            ScoreManager.Instance.ReinitializeFromPlayerPrefs();
            Debug.Log("? ScoreManager已重新初始化，积分已重置！");
        }
        else
        {
            Debug.LogWarning("?? ScoreManager.Instance为空，无法重置。请重新加载场景。");
        }
    }
}
