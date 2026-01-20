using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 水波系统
/// 处理鼠标点击和船体移动产生的水波效果
/// </summary>
public class Ripple : MonoBehaviour
{
    public Camera mainCamera;
    public RenderTexture InteractiveRT; // 交互RT（由俯视摄像头渲染船体位置）
    public RenderTexture PrevRT; // 上一帧
    public RenderTexture CurrentRT; //当前帧
    public RenderTexture TempRT; // 临时RT
    public Shader DrawShader; // 绘制 Shader
    public Shader RippleShader; // 水波扩散Shader
    public Shader AddShader; // 加法
    private Material RippleMat;
    private Material DrawMat;
    private Material AddMat;

    [Range(0, 1.0f)]
    public float DrewRadius = 0.2f;
    public int TextureSize = 512;
    
    private Vector3 lastMousePos;

    void Start()
    {
        // 初始化
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        CurrentRT = CreateRT();
        PrevRT = CreateRT();
        TempRT = CreateRT();

        DrawMat = new Material(DrawShader);
        RippleMat = new Material(RippleShader);
        AddMat = new Material(AddShader);

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = CurrentRT;
        }
    }

    public RenderTexture CreateRT()
    {
        RenderTexture rt = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.RFloat);
        rt.Create();
        return rt;
    }

    /// <summary>
    /// 在指定UV坐标绘制水波（公开方法，供外部调用）
    /// </summary>
    /// <param name="x">UV坐标X (0-1)</param>
    /// <param name="y">UV坐标Y (0-1)</param>
    /// <param name="radius">水波半径</param>
    public void DrawAt(float x, float y, float radius)
    {
        DrawMat.SetTexture("_SourceTex", CurrentRT);
        DrawMat.SetVector("_Pos", new Vector4(x, y, radius));
        Graphics.Blit(null, TempRT, DrawMat);

        RenderTexture rt = TempRT;
        TempRT = CurrentRT;
        CurrentRT = rt;
    }

    void Update()
    {
        // 鼠标左键点击水面时绘制水波
        if (Input.GetMouseButton(0))
        {
            if (mainCamera != null)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    // 检查是否点击了足够远的距离
                    if ((hit.point - lastMousePos).sqrMagnitude > 0.1f)
                    {
                        lastMousePos = hit.point;
                        // 绘制水波
                        DrawAt(hit.textureCoord.x, hit.textureCoord.y, DrewRadius);
                    }
                }
            }
        }

        // 合并交互RT和当前RT
        if (InteractiveRT != null)
        {
            AddMat.SetTexture("_Tex1", InteractiveRT);
            AddMat.SetTexture("_Tex2", CurrentRT);
            Graphics.Blit(null, TempRT, AddMat);
            
            // 交换CurrentRT
            RenderTexture rt0 = TempRT;
            TempRT = CurrentRT;
            CurrentRT = rt0;
        }

        // 水波扩散
        RippleMat.SetTexture("_PrevRT", PrevRT);
        RippleMat.SetTexture("_CurrentRT", CurrentRT);
        Graphics.Blit(null, TempRT, RippleMat);

        // 复制到PrevRT
        Graphics.Blit(TempRT, PrevRT);

        // 交换
        RenderTexture rt = PrevRT;
        PrevRT = CurrentRT;
        CurrentRT = rt;
    }
}
