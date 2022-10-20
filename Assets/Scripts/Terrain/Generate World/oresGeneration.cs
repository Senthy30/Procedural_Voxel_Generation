using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oresGeneration : MonoBehaviour {

    private float[,] noiseMap;

    public int octaves;
    public float persistance, lacunarity, scale;
    public float frequency;

    private int chunckArea, chunckHeight;

    private int[] dx = new int[4] { 1, -1, 0, 0 };
    private int[] dy = new int[4] { 0, 0, 1, -1 };

    private int oresArea;
    private bool[,] seen;
    private Queue<Vector2Int> Q;

    public float[,] generateNoiseMapForOres(int A, int H, int seed, Vector2 addOffSet, ChunckData chunckData) {
        chunckArea = A;
        chunckHeight = H;

        noiseMap = noise.GenerateNoiseMap(chunckArea, chunckArea, seed, scale, octaves, persistance, lacunarity, addOffSet);

        redefineNoiseMap();
        Vector2Int coordinates = findMaximumAreaOfOre();

        seen = new bool[chunckArea + 1, chunckArea + 1];
        findOresInNoiseMap(chunckData, coordinates.x, coordinates.y, true);

        return noiseMap;
    }

    private Vector2Int findMaximumAreaOfOre() {
        int currentMaxArea = 0;
        Vector2Int coordinates = Vector2Int.zero;

        seen = new bool[chunckArea + 1, chunckArea + 1];

        for (int x = 1; x <= chunckArea; x++)
            for (int y = 1; y <= chunckArea; y++) {
                if (noiseMap[x, y] < frequency)
                    continue;
                oresArea = 0;

                findOresInNoiseMap(null, x, y, false);

                if(oresArea > currentMaxArea) {
                    currentMaxArea = oresArea;
                    coordinates = new Vector2Int(x, y);
                }
            }

        return coordinates;
    }

    private void redefineNoiseMap() {
        float[] vMin = new float[2] { 0.5f, 0.7f};

        for(int k = 0; k <= 1; k++) {
            for(int i = 1 + k; i <= chunckArea - k; i++) {
                noiseMap[1 + k, i] = Mathf.Min(vMin[k], noiseMap[1 + k, i]);
                noiseMap[chunckArea - k, i] = Mathf.Min(vMin[k], noiseMap[chunckArea - k, i]);
                noiseMap[i, 1 + k] = Mathf.Min(vMin[k], noiseMap[i, 1 + k]);
                noiseMap[i, chunckArea - k] = Mathf.Min(vMin[k], noiseMap[i, chunckArea - k]);
            }
        }
        
        for (int k = 0; k <= 1; k++) {
            for (int i = 1 + k; i <= chunckArea - k; i++) {
                noiseMap[1 + k, i] -= 0.1f;
                noiseMap[chunckArea - k, i] -= 0.1f;
                noiseMap[i, 1 + k] -= 0.1f;
                noiseMap[i, chunckArea - k] -= 0.1f;
            }
        }
    }

    private void findOresInNoiseMap(ChunckData chunckData, int x, int y, bool typeOfOperation) {
        Q = new Queue<Vector2Int>();

        Q.Enqueue(new Vector2Int(x, y));
        seen[x, y] = true;

        while (Q.Count != 0) {
            Vector2Int currentPos = Q.Peek();
            Q.Dequeue();

            if (typeOfOperation)
                generateOres(chunckData, currentPos.x, currentPos.y, getDiameter(noiseMap[currentPos.x, currentPos.y]));
            else oresArea++;

            for(int d = 0; d < 4; d++) {
                Vector2Int nextPos = new Vector2Int(dx[d], dy[d]) + currentPos;
                if(nextPos.x >= 1 && nextPos.y >= 1 && nextPos.x <= chunckArea && nextPos.y <= chunckArea) 
                    if (noiseMap[nextPos.x, nextPos.y] >= frequency && !seen[nextPos.x, nextPos.y]) {
                        Q.Enqueue(nextPos);
                        seen[nextPos.x, nextPos.y] = true;
                    }
            }
        }
    }

    private int getDiameter(float x) {
        if (x <= 0.5f)
            return 0;
        else if (x <= 0.7f)
            return 1;
        else if (x <= 0.9f)
            return 2;
        return 3;
    }

    private void generateOres(ChunckData chunckData, int x, int z, int D) {
        int y = 60;
        for(int i = y - D; i <= y + D; i++) {
            chunckData.blockType[x, i, z] = 2;
        }
    }

}
