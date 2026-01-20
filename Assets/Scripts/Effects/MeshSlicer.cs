using UnityEngine;
using System.Collections.Generic;

public class MeshSlicer
{
    // === 切割网格 ===
    public static SlicedMeshData SliceMesh(Mesh mesh, Plane localPlane)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector2[] uvs = mesh.uv;

        // 防止原始网格没有 UV 导致后续处理出现问题
        if (uvs == null || uvs.Length < vertices.Length)
            uvs = new Vector2[vertices.Length];

        MeshBuilder posSide = new MeshBuilder();
        MeshBuilder negSide = new MeshBuilder();

        // 用于封顶的顶点列表
        List<VertexData> capVerts = new List<VertexData>();

        // 1. 遍历所有三角形
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // 防止数组越界
            if (i + 2 >= triangles.Length) break;

            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector2 uv1 = uvs[i1];
            Vector2 uv2 = uvs[i2];
            Vector2 uv3 = uvs[i3];

            bool side1 = localPlane.GetSide(v1);
            bool side2 = localPlane.GetSide(v2);
            bool side3 = localPlane.GetSide(v3);

            if (side1 == side2 && side2 == side3)
            {
                if (side1) posSide.AddTriangle(v1, v2, v3, uv1, uv2, uv3);
                else negSide.AddTriangle(v1, v2, v3, uv1, uv2, uv3);
            }
            else
            {
                // 切割三角形，同时处理 UV 插值
                CutTriangle(localPlane, v1, v2, v3, uv1, uv2, uv3, side1, side2, side3, posSide, negSide, capVerts);
            }
        }

        // 2. 封顶 (使用单色 UV)
        if (capVerts.Count >= 3)
            CapMesh(capVerts, posSide, negSide, localPlane);

        return new SlicedMeshData()
        {
            PositiveMesh = posSide.ToMesh(),
            NegativeMesh = negSide.ToMesh()
        };
    }

    // === 数据结构 ===
    public class SlicedMeshData
    {
        public Mesh PositiveMesh;
        public Mesh NegativeMesh;
    }

    // 用于封顶的简单结构
    struct VertexData
    {
        public Vector3 Position;
        // 封顶不需要 UV，因为封顶使用固定颜色，所以这里只记录位置即可
    }

    // === 切割逻辑 (带 UV 插值) ===
    private static void CutTriangle(Plane plane, Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3, bool s1, bool s2, bool s3, MeshBuilder pos, MeshBuilder neg, List<VertexData> capVerts)
    {
        Vector3 solo, pair1, pair2;
        Vector2 uvSolo, uvPair1, uvPair2;
        bool soloSide;

        if (s1 != s2 && s1 != s3)
        {
            solo = v1; pair1 = v2; pair2 = v3;
            uvSolo = uv1; uvPair1 = uv2; uvPair2 = uv3;
            soloSide = s1;
        }
        else if (s2 != s1 && s2 != s3)
        {
            solo = v2; pair1 = v3; pair2 = v1;
            uvSolo = uv2; uvPair1 = uv3; uvPair2 = uv1;
            soloSide = s2;
        }
        else
        {
            solo = v3; pair1 = v1; pair2 = v2;
            uvSolo = uv3; uvPair1 = uv1; uvPair2 = uv2;
            soloSide = s3;
        }

        float ent1, ent2;
        Vector3 dir1 = pair1 - solo;
        Vector3 dir2 = pair2 - solo;
        plane.Raycast(new Ray(solo, dir1), out ent1);
        plane.Raycast(new Ray(solo, dir2), out ent2);

        Vector3 inter1 = solo + dir1.normalized * ent1;
        Vector3 inter2 = solo + dir2.normalized * ent2;

        // --- 修复：UV 插值 (线性插值，根据距离比例计算) ---
        float t1 = ent1 / dir1.magnitude;
        float t2 = ent2 / dir2.magnitude;
        Vector2 uvInter1 = Vector2.Lerp(uvSolo, uvPair1, t1);
        Vector2 uvInter2 = Vector2.Lerp(uvSolo, uvPair2, t2);

        // 添加到封顶列表
        AddUnique(capVerts, inter1);
        AddUnique(capVerts, inter2);

        if (soloSide)
        {
            pos.AddTriangle(solo, inter1, inter2, uvSolo, uvInter1, uvInter2);
            neg.AddTriangle(inter1, pair1, pair2, uvInter1, uvPair1, uvPair2);
            neg.AddTriangle(inter1, pair2, inter2, uvInter1, uvPair2, uvInter2);
        }
        else
        {
            neg.AddTriangle(solo, inter1, inter2, uvSolo, uvInter1, uvInter2);
            pos.AddTriangle(inter1, pair1, pair2, uvInter1, uvPair1, uvPair2);
            pos.AddTriangle(inter1, pair2, inter2, uvInter1, uvPair2, uvInter2);
        }
    }

    // === 封顶逻辑 (生成封顶面和透明碎片) ===
    private static void CapMesh(List<VertexData> verts, MeshBuilder pos, MeshBuilder neg, Plane localPlane)
    {
        // 1. 计算中心点
        Vector3 center = Vector3.zero;
        foreach (var v in verts) center += v.Position;
        center /= verts.Count;

        // 2. 构建局部坐标系 (使用 Atan2 算法替代 SignedAngle 更稳定，防止翻转)
        Vector3 forward = Vector3.Cross(localPlane.normal, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.Cross(localPlane.normal, Vector3.right);
        forward.Normalize();
        Vector3 right = Vector3.Cross(localPlane.normal, forward).normalized;

        // 3. 排序顶点 (这是防止碎片出现"支离破碎"的关键)
        verts.Sort((a, b) => {
            float angleA = Mathf.Atan2(Vector3.Dot(a.Position - center, right), Vector3.Dot(a.Position - center, forward));
            float angleB = Mathf.Atan2(Vector3.Dot(b.Position - center, right), Vector3.Dot(b.Position - center, forward));
            return angleA.CompareTo(angleB);
        });

        // 4. 生成封顶面
        // --- 修复：使用单色 UV ---
        // 这里统一使用 (0,0) 作为封顶的 UV，
        // 确保封顶贴图 左下角 的颜色（通常需要是内部颜色，比如木头色）
        Vector2 solidUV = Vector2.zero;

        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 p1 = verts[i].Position;
            Vector3 p2 = verts[(i + 1) % verts.Count].Position;

            // 过滤极短边 (防止细碎的几何计算)
            if ((p1 - p2).sqrMagnitude < 0.000001f) continue;

            // 正式指定三角形方向，确保封顶面能正确显示
            pos.AddTriangle(center, p2, p1, solidUV, solidUV, solidUV);
            neg.AddTriangle(center, p1, p2, solidUV, solidUV, solidUV);
        }
    }

    // 去重逻辑 (防止重复点，防止 QuickHull 错误)
    private static void AddUnique(List<VertexData> list, Vector3 p)
    {
        if (!list.Exists(v => (v.Position - p).sqrMagnitude < 0.001f))
            list.Add(new VertexData { Position = p });
    }

    /// <summary>
    /// 检查Mesh是否适合创建Convex MeshCollider
    /// </summary>
    private static bool IsMeshSuitableForConvex(Mesh mesh)
    {
        if (mesh == null || mesh.vertexCount < 4) return false;

        Vector3[] vertices = mesh.vertices;
        if (vertices == null || vertices.Length < 4) return false;

        // 检查Mesh的厚度（三个维度都要有足够的厚度）
        Bounds bounds = mesh.bounds;
        float minThickness = 0.02f; // 最小厚度2cm
        if (bounds.size.x < minThickness || bounds.size.y < minThickness || bounds.size.z < minThickness)
        {
            return false; // Mesh太薄，不适合Convex
        }

        // 检查顶点是否共面（简化检查：计算顶点的分布范围）
        Vector3 center = bounds.center;
        float maxDistance = 0f;
        float minDistance = float.MaxValue;

        foreach (Vector3 vertex in vertices)
        {
            float distance = Vector3.Distance(vertex, center);
            if (distance > maxDistance) maxDistance = distance;
            if (distance < minDistance) minDistance = distance;
        }

        // 如果所有顶点距离中心都很接近，可能是共面的
        if (maxDistance - minDistance < 0.01f)
        {
            return false; // 顶点可能共面
        }

        return true;
    }

    // === 创建对象 (生成碎片对象) ===
    public static GameObject CreateObj(GameObject original, Mesh m, string name)
    {
        // 1. 验证网格
        if (m == null || m.vertexCount < 4) return null;
        m.RecalculateBounds();

        // 2. 强制过滤，防止太小或太薄的碎片
        // 这是减少和过滤细碎碎片和"PhysX错误"的有效手段
        if (m.bounds.size.magnitude < 0.05f) return null; // 碎片直径小于 5cm 就丢弃
        if (m.bounds.size.x < 0.01f || m.bounds.size.y < 0.01f || m.bounds.size.z < 0.01f) return null; // 防止纸一样薄的碎片

        GameObject obj = new GameObject(name);

        // 继承 Transform
        obj.transform.SetParent(original.transform.parent);
        obj.transform.localPosition = original.transform.localPosition;
        obj.transform.localRotation = original.transform.localRotation;
        obj.transform.localScale = original.transform.localScale;

        // 继承 Tag（重要：让碎片继承原始对象的标签，这样船撞到碎片时也能检测到）
        obj.tag = original.tag;

        obj.AddComponent<MeshFilter>().mesh = m;
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mr.material = original.GetComponent<MeshRenderer>().material;

        // 3. 安全创建碰撞体（优先使用MeshCollider，如果Mesh不适合则使用BoxCollider）
        Collider collider = null;
        
        // 先检查Mesh是否适合创建Convex MeshCollider
        bool useConvexMesh = IsMeshSuitableForConvex(m);
        bool meshColliderSuccess = false;
        
        if (useConvexMesh)
        {
            try
            {
                var mc = obj.AddComponent<MeshCollider>();
                mc.sharedMesh = m;
                
                // 尝试设置convex，Unity可能会输出警告但不抛出异常
                // 使用try-catch捕获可能的异常
                try
                {
                    mc.convex = true;
                    meshColliderSuccess = true;
                    collider = mc;
                }
                catch
                {
                    // 如果设置convex时抛出异常，移除MeshCollider
                    Object.DestroyImmediate(mc);
                    meshColliderSuccess = false;
                }
            }
            catch
            {
                // 如果添加MeshCollider组件时抛出异常
                meshColliderSuccess = false;
            }
        }
        
        // 如果MeshCollider失败或不适合，使用BoxCollider
        if (!meshColliderSuccess || collider == null)
        {
            // 移除可能存在的MeshCollider
            MeshCollider existingMC = obj.GetComponent<MeshCollider>();
            if (existingMC != null)
            {
                Object.DestroyImmediate(existingMC);
            }
            
            try
            {
                var bc = obj.AddComponent<BoxCollider>();
                bc.size = m.bounds.size;
                bc.center = m.bounds.center;
                collider = bc;
            }
            catch
            {
                // 如果BoxCollider也失败，销毁对象
                Object.Destroy(obj);
                return null;
            }
        }

        // 添加刚体
        float scaleAvg = (obj.transform.localScale.x + obj.transform.localScale.y + obj.transform.localScale.z) / 3.0f;
        var rb = obj.AddComponent<Rigidbody>();
        rb.mass = Mathf.Max(0.1f, m.bounds.size.magnitude * scaleAvg * 2.0f);

        return obj;
    }

    // === Mesh构建器 (简化三角形构建) ===
    class MeshBuilder
    {
        List<Vector3> v = new List<Vector3>();
        List<Vector2> u = new List<Vector2>();
        List<int> t = new List<int>();

        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            // 修复过滤，如果三角形面积接近0，就不添加，
            // 这样可以防止生成那些"支离破碎"的透明碎片
            if (Vector3.Cross(v2 - v1, v3 - v1).sqrMagnitude < 0.0000001f) return;

            int i = v.Count;
            v.Add(v1); v.Add(v2); v.Add(v3);
            u.Add(uv1); u.Add(uv2); u.Add(uv3);
            t.Add(i); t.Add(i + 1); t.Add(i + 2);
        }

        public Mesh ToMesh()
        {
            if (v.Count < 3) return null;
            Mesh m = new Mesh();
            m.vertices = v.ToArray();
            m.uv = u.ToArray();
            m.triangles = t.ToArray();
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }
}
