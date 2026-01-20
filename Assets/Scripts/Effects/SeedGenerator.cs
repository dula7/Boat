using UnityEngine;
using System.Collections.Generic;

public class SeedGenerator
{
    public static List<Vector3> GetBlockySeeds(Bounds bounds, int count)
    {
        List<Vector3> seeds = new List<Vector3>();
        float cubicRoot = Mathf.Pow(count, 1f / 3f);

        // 防止物体本身尺寸为0导致除零错误
        float sizeX = Mathf.Max(0.01f, bounds.size.x);
        float sizeY = Mathf.Max(0.01f, bounds.size.y);
        float sizeZ = Mathf.Max(0.01f, bounds.size.z);
        float maxSize = Mathf.Max(sizeX, Mathf.Max(sizeY, sizeZ));

        int gridX = Mathf.Max(1, Mathf.CeilToInt(cubicRoot * (sizeX / maxSize)));
        int gridY = Mathf.Max(1, Mathf.CeilToInt(cubicRoot * (sizeY / maxSize)));
        int gridZ = Mathf.Max(1, Mathf.CeilToInt(cubicRoot * (sizeZ / maxSize)));

        Vector3 cellSize = new Vector3(sizeX / gridX, sizeY / gridY, sizeZ / gridZ);

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                for (int z = 0; z < gridZ; z++)
                {
                    Vector3 cellCenter = bounds.min + new Vector3(
                        (x + 0.5f) * cellSize.x,
                        (y + 0.5f) * cellSize.y,
                        (z + 0.5f) * cellSize.z
                    );

                    // 这里的 0.4f 可以提取为参数，控制随机性
                    // 0.0f = 完美网格, 0.5f = 最大随机但不重叠
                    Vector3 jitter = new Vector3(
                        Random.Range(-0.4f, 0.4f) * cellSize.x,
                        Random.Range(-0.4f, 0.4f) * cellSize.y,
                        Random.Range(-0.4f, 0.4f) * cellSize.z
                    );

                    seeds.Add(cellCenter + jitter);
                }
            }
        }
        return seeds;
    }
}