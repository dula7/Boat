using UnityEngine;
using System.Collections.Generic;

public class VoronoiShatter : MonoBehaviour
{
    // ����ģʽ
    public static VoronoiShatter Instance;

    [Header("Settings")]
    public int shardCount = 10;        // ��Ҫ�������ٿ���Ƭ����Χ�ھӵ�������
    public float fractureRadius = 1.0f; // ���鷶Χ�뾶
    public float explosionForce = 500f;

    // ������Ƭ��״�������
    public float irregularity = 0.5f;   // 0=�������ηֲ���1=��ȫ���

    void Awake()
    {
        Instance = this;
    }

    // ����ӿ�
    public void TriggerFracture(GameObject target, Vector3 impactPoint)
    {
        // ���ģ������������ƽ�����У���������֯������ Voronoi ��Ƭ
        GenerateVoronoiFracture(target, impactPoint);
    }

    void GenerateVoronoiFracture(GameObject target, Vector3 center)
    {
        Mesh currentMesh = target.GetComponent<MeshFilter>().mesh;
        Material mat = target.GetComponent<MeshRenderer>().material;
        Transform tr = target.transform;

        // ��¼һ�³�ʼ��������Ϣ����������������
        GameObject rootObj = target;

        // 1. ����һ�顰�ھӵ㡱 (Blocky ��)
        List<Vector3> neighbors = new List<Vector3>();

        // ����һ������İ�Χ�У��� center Ϊ���ģ�size Ϊ fractureRadius * 2
        Bounds virtualBounds = new Bounds(center, Vector3.one * fractureRadius * 2f);

        // ��ȡ��״���ӵ�
        List<Vector3> seeds = SeedGenerator.GetBlockySeeds(virtualBounds, shardCount);

        // ���˵����ĵ��Լ�����Ϊ GetBlockySeeds ���ܻ�����һ�����������ĵĵ㣩
        // ����ֻ������Щ��������һ������ĵ���Ϊ�ھ�
        foreach (var seed in seeds)
        {
            if (Vector3.Distance(seed, center) > 0.1f)
            {
                neighbors.Add(seed);
            }
        }

        // 2. ��������롱��Ƭ
        int pieceCount = 0;

        foreach (Vector3 neighbor in neighbors)
        {
            // ���ʣ�µĺ���̫С�ˣ���ֹͣ�и�
            if (currentMesh.bounds.size.magnitude < 0.1f) break;

            // === ���� Voronoi �и�ƽ�� ===
            // ƽ�涨��Ϊ�����ӡ�Բ�ġ��͡��ھӡ����߶ε��д���
            // ���߷���ָ���ھӣ���ָ����ࣩ
            // �����㣺�߶ε��е�
            Vector3 midPoint = (center + neighbor) * 0.5f;
            Vector3 normal = (neighbor - center).normalized;

            // תΪ�ֲ��ռ�ƽ��
            Vector3 localMidPoint = tr.InverseTransformPoint(midPoint);
            Vector3 localNormal = tr.InverseTransformDirection(normal);
            Plane cutPlane = new Plane(localNormal, localMidPoint);

            // ִ���и�
            // PositiveMesh (���) -> �����ȥ��Ϊ��Ƭ
            // NegativeMesh (�ڲ�) -> ����������Ϊ�µĺ���
            var result = MeshSlicer.SliceMesh(currentMesh, cutPlane);

            if (result != null)
            {
                // A. ���ɰ����ȥ����Ƭ (Positive Side)
                if (result.PositiveMesh != null)
                {
                    GameObject shard = MeshSlicer.CreateObj(rootObj, result.PositiveMesh, "Shard_" + pieceCount);
                    // ��������Ƭ��Shard���ĵط���
                    if (shard != null)
                    {
                        SetupPiece(shard, center); 
                        pieceCount++;
                    }
                }

                // B. ���º��� (Negative Side)
                // ����п��ˣ�����ƽ����ȫ���������棩�����ı��ֲ���
                if (result.NegativeMesh != null)
                {
                    currentMesh = result.NegativeMesh;
                }
            }
        }

        // 3. ���ʣ�µ� currentMesh �������ĵġ�Voronoi ���ġ� (Ҳ����ʣ�µ�ǽ��)
        GameObject core = MeshSlicer.CreateObj(rootObj, currentMesh, "Core_Piece");
        if (core != null)
        {
            // ע�⣺���ﴫ true����ʾ���Ǻ��ģ���Ҫ����
            SetupPiece(core, center);
        }

        // 4. ����ԭʼ����
        Destroy(rootObj);
    }

    void SetupPiece(GameObject obj, Vector3 explosionCenter)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb)
        {
            // ��Сһ��㣬������϶����ֹ����������Ϊ���ǿ�ס��
            obj.transform.localScale *= 0.95f;

            // ����������ײ��⣬��ֹ��Ƭ��Ϊ̫����̫�����������
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // ȷ������������
            rb.isKinematic = false;
            rb.useGravity = true;

            // ʩ�ӱ�ը��
            rb.AddExplosionForce(explosionForce * 1.5f, explosionCenter, fractureRadius * 2f);

            // 5�������
            Destroy(obj, 1.5f);
        }
    }
}