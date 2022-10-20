using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreesGeneration: MonoBehaviour {

    private float[,] noiseMap;

    public int octaves;
    public float persistance, lacunarity, scale;
    public float frequency;

    private int[] dx = new int[8] { -1, 0, 1, 1, 1, 0, -1, -1};
    private int[] dy = new int[8] { 1, 1, 1, 0, -1, -1, -1, 0 };

    private int chunckArea, chunckHeight;

    public float[,] generateTrees(int A, int H, int seed, Vector2 addOffSet, ChunckData chunckData) {
        chunckArea = A;
        chunckHeight = H;
        noiseMap = noise.GenerateNoiseMap(chunckArea, chunckArea, seed, scale, octaves, persistance, lacunarity, addOffSet);

        for (int x = 1; x <= chunckArea; x++) {
            noiseMap[x, 1] = 1;
            noiseMap[chunckArea, x] = 1;
        }

        for(int x = 1; x <= chunckArea; x++)
            for(int z = 1; z <= chunckArea; z++) {

                if (noiseMap[x, z] >= frequency)
                    continue;

                for (int d = 0; d < 8; d++)
                    if (noiseMap[x + dx[d], z + dy[d]] < frequency)
                        noiseMap[x + dx[d], z + dy[d]] = 1;

                for (int y = 1; y <= chunckHeight; y++) {
                    if (chunckData.blockType[x, y - 1, z] == 1 || chunckData.blockType[x, y - 1, z] == 5)
                        break;
                    if (chunckData.blockType[x, y, z] == 0 || chunckData.blockType[x, y, z] == 7) {
                        placeTree(x, y, z, chunckData);
                        break;
                    }
                }

            }

        return noiseMap;
    }

    private void placeTree(int x, int y, int z, ChunckData chunckData) {
        for(int k = 2; k <= 3; k++)
            for(int i = -2; i <= 2; i++)
                for(int j = -2; j <= 2; j++) {
                    if (x + i < 0 || z + j < 0 || x + i > chunckArea + 1 || z + j > chunckArea + 1)
                        continue;
                    if (chunckData.blockType[x + i, y + k, z + j] != 0)
                        continue;
                    chunckData.blockType[x + i, y + k, z + j] = 7;
                }
        for (int k = 4; k <= 5; k++)
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++) {
                    if (x + i < 0 || z + j < 0 || x + i > chunckArea + 1 || z + j > chunckArea + 1)
                        continue;
                    chunckData.blockType[x + i, y + k, z + j] = 7;
                }
        for (int d = 0; d < 8; d += 2) {
            if (x + dx[d] < 0 || z + dy[d] < 0 || x + dx[d] > chunckArea + 1 || z + dy[d] > chunckArea + 1)
                continue;
            chunckData.blockType[x + dx[d], y + 5, z + dy[d]] = 0;
        }
        if (x < 0 || z < 0 || x > chunckArea + 1 || z > chunckArea + 1)
            return;
        for (int i = 0; i <= 4; i++) {
            chunckData.blockType[x, y + i, z] = 6;
        }
    }

    public void generateMarginsTrees(ChunckData chunckData, Vector2Int coordinates, Dictionary<Vector2Int, ChunckData> terrainDictionary) {
        float[,] tempNoiseMap;
        
        for(int d = 0; d < 8; d++) {
            Vector2Int nextCoordinates = coordinates + new Vector2Int(dx[d], dy[d]);

            if (!terrainDictionary.ContainsKey(nextCoordinates))
                continue;

            tempNoiseMap = terrainDictionary[nextCoordinates].treesNoiseMap;

            for(int x = 1; x <= chunckArea; x++)
                for(int y = 1; y <= chunckArea; y++) {
                    if(tempNoiseMap[x, y] < frequency) {
                        bool ok = false;

                        for (int k = 0; k < 8; k += 2) {
                            ok |= checkForInChunck(chunckArea * dx[d] + x + 2 * dx[k], chunckArea * dy[d] + y + 2 * dy[k]);
                            if (ok) {
                                //Debug.Log(new Vector2(chunckArea * dx[d] + x + 2 * dx[k], chunckArea * dx[d] + y + 2 * dy[k]));
                                continue;
                            }
                        }

                        if (!ok)
                            continue;

                        int h = chunckData.getHeightTerrain(terrainDictionary[nextCoordinates].terrainNoiseMap[chunckArea - x + 1, chunckArea - y + 1]) + 1;
                        int typeBelow = terrainDictionary[nextCoordinates].blockType[x, h - 1, y];

                        if (typeBelow == 1 || typeBelow == 5)
                            continue;

                        placeTree(chunckArea * dx[d] + x, h, chunckArea * dy[d] + y, chunckData);
                    }
                }
        }
    }

    private bool checkForInChunck(int x, int y) {
        if (x >= 0 && y >= 0 && x <= chunckArea + 1 && y <= chunckArea + 1)
            return true;
        return false;
    }
}
