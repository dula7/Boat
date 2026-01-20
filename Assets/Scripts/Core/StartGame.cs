using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    /// <summary>
    /// 加载场景并重置光照设置
    /// </summary>
    private void LoadSceneWithLightingReset(int sceneIndex)
    {
        // 使用协程异步加载场景，确保光照设置正确重置
        StartCoroutine(LoadSceneCoroutine(sceneIndex));
    }

    /// <summary>
    /// 异步加载场景的协程
    /// </summary>
    private IEnumerator LoadSceneCoroutine(int sceneIndex)
    {
        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
        
        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 场景加载完成后，等待一帧确保所有对象都已初始化
        yield return null;

        // 重置光照设置
        ResetLightingSettings();

        // 通知 AudioManager 切换 BGM（如果存在）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.CheckAndPlaySceneBGM();
        }
    }

    /// <summary>
    /// 重置光照设置，确保新场景的光照正确显示
    /// </summary>
    private void ResetLightingSettings()
    {
        // 强制 Unity 重新计算光照
        // 这会确保新场景的光照设置被正确应用
        LightmapSettings.lightmaps = new LightmapData[0];
        
        // 查找场景中的所有灯光并确保它们正确激活
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light != null && light.gameObject != null)
            {
                // 确保灯光对象是激活的
                if (!light.gameObject.activeInHierarchy)
                {
                    light.gameObject.SetActive(true);
                }
                // 确保灯光组件是启用的
                if (!light.enabled)
                {
                    light.enabled = true;
                }
            }
        }

        Debug.Log($"场景加载完成，已重置光照设置。找到 {lights.Length} 个灯光。");
    }

    // 跳转到索引为0的场景
    public void GoToScene0()
    {
        LoadSceneWithLightingReset(0);
    }

    // 跳转到索引为1的场景
    public void GoToScene1()
    {
        LoadSceneWithLightingReset(1);
    }

    // 跳转到索引为2的场景
    public void GoToScene2()
    {
        LoadSceneWithLightingReset(2);
    }

    // 跳转到索引为3的场景
    public void GoToScene3()
    {
        LoadSceneWithLightingReset(3);
    }

    // 跳转到索引为4的场景
    public void GoToScene4()
    {
        LoadSceneWithLightingReset(4);
    }

    // 跳转到索引为5的场景
    public void GoToScene5()
    {
        LoadSceneWithLightingReset(5);
    }

    // 跳转到索引为6的场景
    public void GoToScene6()
    {
        LoadSceneWithLightingReset(6);
    }
}
