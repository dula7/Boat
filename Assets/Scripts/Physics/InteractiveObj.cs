using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObj : MonoBehaviour
{
    public Vector3 lastPos;

    private Renderer mRenderer;
    // Start is called before the first frame update
    void Start()
    {
        // 保存之前位置
        lastPos = transform.position;

        mRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(mRenderer.enabled)
        {
            mRenderer.enabled = false;
        }
        if((transform.position - lastPos).sqrMagnitude > 0.01f)
        {
            lastPos = transform.position;
            // 显示物体渲染
            mRenderer.enabled = true;
        }
    }
}
