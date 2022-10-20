using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunckData {

    public int cntTriangles;

    public float[,] terrainNoiseMap, treesNoiseMap;
    public byte[,,] blockType;

    public bool[,,,] existFace;
    public List<Vector3Int>[] blockTriangles = new List<Vector3Int>[2];

    public List<byte>[] faceDirection = new List<byte>[2];
    public List<int>[] triangles = new List<int>[2];
    public List<Vector2> uvs;
    public List<Vector3> vertices;

    private int maxWaterLevel = 10;

    public ChunckData(int chunckArea, int chunckHeight, float[,] heightMap){

        blockType = new byte[chunckArea + 2, chunckHeight + 1, chunckArea + 2];
        terrainNoiseMap = heightMap;

        for (int x = 0; x <= chunckArea + 1; x++)
            for (int z = 0; z <= chunckArea + 1; z++) {
                int hAdd = getHeightTerrain(heightMap[x, z]);

                for (int y = 1; y <= hAdd; y++) {
                    blockType[x, y, z] = 2;
                }
            }

        for (int x = 0; x <= chunckArea + 1; x++)
            for (int z = 0; z <= chunckArea + 1; z++) {
                for (int y = 1; y < chunckHeight; y++) {
                    if (blockType[x, y + 1, z] == 0) {
                        blockType[x, y - 1, z] = 3;
                        blockType[x, y - 2, z] = 3;

                        if (y > maxWaterLevel + 2)
                            blockType[x, y, z] = 4;
                        else {
                            blockType[x, y, z] = 5;
                            blockType[x, y - 1, z] = 5;
                        }

                        break;
                    }
                }
                for(int y = maxWaterLevel + 1; y > 0; y--) {
                    if (blockType[x, y, z] > 1)
                        break;
                    blockType[x, y, z] = 1;
                }
            }

        byte temp;
        for (int x = 0; x <= chunckArea + 1; x++)
            for (int z = 0; z <= (chunckArea + 1) / 2; z++)
                for (int y = 1; y < chunckHeight; y++) {
                    temp = blockType[x, y, z];
                    blockType[x, y, z] = blockType[chunckArea - x + 1, y, chunckArea - z + 1];
                    blockType[chunckArea - x + 1, y, chunckArea - z + 1] = temp;
                }
    }

    public int getHeightTerrain(float x) {
        if (x < 0.06f)
            return (maxWaterLevel - 6 + Mathf.RoundToInt(x * 100));
        else if (x <= 0.12f)
            return (maxWaterLevel + Mathf.RoundToInt(x / 6f * 100));
        else if (x <= 0.42f)
            return (maxWaterLevel + 1 + Mathf.RoundToInt(x / 6f * 100));
        else if (x <= 0.60f)
            return (maxWaterLevel + 2 + Mathf.RoundToInt(x / 6f * 100));
        return (maxWaterLevel - 7 + Mathf.RoundToInt(x / 3f * 100));
    }

}
