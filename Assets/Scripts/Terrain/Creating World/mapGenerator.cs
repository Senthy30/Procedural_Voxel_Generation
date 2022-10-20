using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class mapGenerator : MonoBehaviour {
    public enum DrawMode { NoiseMap, ColourMap, TerrainMap };
    public DrawMode drawMode;

    public int distanceNumberChuncks;
    public int chunckArea, chunckHeight;
    public float noiseScale;

    public int octaves;
    public float persistance, lacunarity;

    public int seed;
    public Vector2 offSet;

    public bool autoUpdate;

    public TerrainType[] regions;

    public Material textureMaterialSource;
    public GameObject planeColourMap, centerObject;

    private int cnt = 0;
    private Color[] colourMap;

    private GameObject mapTerrain;

    public GameObject chunckEmpty;
    public GameObject testObject;
    public Material[] matTest;
    public PhysicMaterial physicMaterial;

    private Dictionary<Vector2Int, bool> wasCreatedChunck = new Dictionary<Vector2Int, bool>();
    private Dictionary <Vector2Int, ChunckData> terrainDictionary = new Dictionary<Vector2Int, ChunckData>();

    private float[,] noiseMap;

    private int[] dx = new int[6] { 1, -1, 0, 0, 0, 0 };
    private int[] dy = new int[6] { 0, 0, 1, -1, 0, 0 };
    private int[] dz = new int[6] { 0, 0, 0, 0, 1, -1 };

    private TerrainGeneration terrainGeneration;
    private TreesGeneration treesGeneration;

    private Heap Q = new Heap();

    public float lvl;
    public bool typeGen;

    public bool updateMapTerrain(GameObject mapTerrain, Vector3 playerPosition, bool typeOfOperation) {
        terrainGeneration = GetComponent<TerrainGeneration>();
        treesGeneration = GetComponent<TreesGeneration>();

        Vector2Int currentChunck = new Vector2Int((int)Mathf.Round(playerPosition.x / 16f), (int)Mathf.Round(playerPosition.z / 16f));

        int posX = distanceNumberChuncks + 3;
        int posZ = distanceNumberChuncks + 3;

        for(int x = 1; x <= (distanceNumberChuncks + 3) * 2 - 1; x++) {
            for (int z = 1; z <= (distanceNumberChuncks + 3) * 2 - 1; z++) {
                int distX = posX - x;
                int distZ = posZ - z;

                if (Mathf.Abs(distX) + Mathf.Abs(distZ) > distanceNumberChuncks + 2)
                    continue;

                Vector2Int coordinates = new Vector2Int(distX, distZ) + currentChunck;

                if (terrainDictionary.ContainsKey(coordinates))
                    continue;

                Vector2 addOffSet = new Vector2(-chunckArea * (currentChunck.x + distX), chunckArea * (currentChunck.y + distZ));

                createNoiseMap(addOffSet);

                ChunckData chunckData = new ChunckData(chunckArea, chunckHeight, noiseMap);
                chunckData.treesNoiseMap = treesGeneration.generateTrees(chunckArea, chunckHeight, seed, addOffSet, chunckData);
                
                terrainDictionary.Add(coordinates, chunckData);
            }
        }

        for (int x = 1; x <= (distanceNumberChuncks + 3) * 2 - 1; x++) {
            for (int z = 1; z <= (distanceNumberChuncks + 3) * 2 - 1; z++) {
                int distX = posX - x;
                int distZ = posZ - z;

                if (Mathf.Abs(distX) + Mathf.Abs(distZ) > distanceNumberChuncks)
                    continue;

                Vector2Int coordinates = new Vector2Int(distX, distZ) + currentChunck;

                if (wasCreatedChunck.ContainsKey(coordinates))
                    continue;

                ChunckData chunckData = terrainDictionary[coordinates];
                wasCreatedChunck.Add(coordinates, true);

                generateMarginsChunck(chunckData, coordinates);
                treesGeneration.generateMarginsTrees(chunckData, coordinates, terrainDictionary);

                GameObject chunckObj = terrainGeneration.generateTerrain(chunckArea, chunckHeight, new Vector3(currentChunck.x + distX, 0, currentChunck.y + distZ), chunckData);
                chunckObj.transform.parent = mapTerrain.transform;

                if (typeOfOperation)
                    return true;
            }
        }

        return false;
    }

    private void generateMarginsChunck(ChunckData chunckData, Vector2Int coordinates) {
        for(int d = 0; d < 4; d++) {
            
            Vector2Int nextCoordinates = new Vector2Int(coordinates.x + dx[d], coordinates.y + dy[d]);

            if (!terrainDictionary.ContainsKey(nextCoordinates))
                continue;

            ChunckData nextChunck = terrainDictionary[nextCoordinates];

            if(d < 2) {
                int currentX = (d == 1) ? 0 : (chunckArea + 1);
                int nextX = (d == 1) ? chunckArea : 1;

                for (int y = 1; y <= chunckHeight; y++)
                    for (int z = 1; z <= chunckArea; z++)
                        chunckData.blockType[currentX, y, z] = nextChunck.blockType[nextX, y, z];
            } else {
                int currentZ = (d == 3) ? 0 : (chunckArea + 1);
                int nextZ = (d == 3) ? chunckArea : 1;

                for (int y = 1; y <= chunckHeight; y++)
                    for (int x = 1; x <= chunckArea; x++)
                        chunckData.blockType[x, y, currentZ] = nextChunck.blockType[x, y, nextZ];
            }
        }
    }

    /*public void GenerateMap() {
        if (cnt == 0)
            mapTerrain = Instantiate(centerObject, Vector3.zero, Quaternion.identity);
        else {
            DestroyImmediate(mapTerrain);
            terrainDictionary.Clear();
            mapTerrain = Instantiate(centerObject, Vector3.zero, Quaternion.identity);
        }
        if (cnt == 0)
            cnt++;
        mapTerrain.name = "Terrain";
        mapTerrain.SetActive(true);

        terrGen = GetComponent<terrainGeneration>();
        mapDisplay display = FindObjectOfType<mapDisplay>();
        Vector2Int currentChunck = new Vector2Int((int)Mathf.Round(transform.position.x / 160f), (int)Mathf.Round(transform.position.z / 160f));

        int posX, posZ;
        posX = posZ = distanceNumberChuncks + 3;

        for (int z = 1; z <= (distanceNumberChuncks + 3) * 2 - 1; z++)
            for (int x = 1; x <= (distanceNumberChuncks + 3) * 2 - 1; x++) {
                int distX = posX - x;
                int distZ = posZ - z;

                if (Mathf.Abs(distX) + Mathf.Abs(distZ) > distanceNumberChuncks + 2)
                    continue;

                Vector2Int coordinates = new Vector2Int(distX, distZ) + currentChunck;

                Vector2 addOffSet = new Vector2(-chunckArea * (currentChunck.x + distX), chunckArea * (currentChunck.y + distZ));

                createNoiseMap(addOffSet);
            }

        for (int z = 1; z <= (distanceNumberChuncks + 3) * 2 - 1; z++)
            for (int x = 1; x <= (distanceNumberChuncks + 3) * 2 - 1; x++) {
                int distX = posX - x;
                int distZ = posZ - z;

                if (Mathf.Abs(distX) + Mathf.Abs(distZ) > distanceNumberChuncks)
                    continue;

                Vector2Int coordinates = new Vector2Int(distX, distZ) + currentChunck;

                createMarginsForNoiseMap(coordinates);
            }

        for (int z = 1; z <= (distanceNumberChuncks + 3) * 2 - 1; z++) {
            for (int x = 1; x <= (distanceNumberChuncks + 3) * 2 - 1; x++) {
                int distX = posX - x;
                int distZ = posZ - z;

                if (Mathf.Abs(distX) + Mathf.Abs(distZ) > distanceNumberChuncks + 2)
                    continue;

                Vector2Int coordinates = new Vector2Int(distX, distZ) + currentChunck;

                if (terrainDictionary.ContainsKey(coordinates))
                    continue;

                Vector2 addOffSet = new Vector2(-chunckArea * (currentChunck.x + distX), chunckArea * (currentChunck.y + distZ));

                if (drawMode == DrawMode.TerrainMap) {
                    //ChunckData chunckData = new ChunckData(chunckArea, chunckHeight, noiseMapDictionary[coordinates].noiseMap);

                    treesGeneration trees = GetComponent<treesGeneration>();
                    chunckData.treesNoiseMap = trees.generateTrees(chunckArea, chunckHeight, seed, addOffSet, chunckData);

                    terrainDictionary.Add(coordinates, chunckData);

                    continue;
                }

                generateColour(coordinates);

                GameObject plane = Instantiate(planeColourMap, new Vector3(chunckArea * 10 * (distX + currentChunck.x), 0, chunckArea * 10 * (distZ + currentChunck.y)), Quaternion.identity);
                plane.transform.parent = mapTerrain.transform;

                Renderer planeRenderer = plane.transform.GetComponent<Renderer>();
                Material newMaterialref = new Material(textureMaterialSource);
                planeRenderer.material = newMaterialref;

                if (drawMode == DrawMode.NoiseMap) {
                    display.DrawTexture(textureGenerator.TextureFromHeightMap(chunckArea, noiseMapDictionary[coordinates].noiseMap, typeGen, lvl), planeRenderer);
                } else if (drawMode == DrawMode.ColourMap)
                    display.DrawTexture(textureGenerator.TextureFromColourMap(colourMap, chunckArea, chunckArea), planeRenderer);
            }
        }

        if (drawMode != DrawMode.TerrainMap)
            return;

        for (int z = 1; z <= (distanceNumberChuncks + 3) * 2 - 1; z++) 
            for (int x = 1; x <= (distanceNumberChuncks + 3) * 2 - 1; x++) {
                int distX = posX - x;
                int distZ = posZ - z;

                if (Mathf.Abs(distX) + Mathf.Abs(distZ) > distanceNumberChuncks)
                    continue;

                Vector2Int coordinates = new Vector2Int(distX, distZ) + currentChunck;
                Vector2 addOffSet = new Vector2(-chunckArea * (currentChunck.x + distX), chunckArea * (currentChunck.y + distZ));

                ChunckData chunckData = terrainDictionary[coordinates];

                treesGeneration trees = GetComponent<treesGeneration>();
                trees.generateMarginsTrees(chunckData, coordinates, terrainDictionary);

                GameObject objGen = terrGen.generateTerrain(chunckArea, chunckHeight, new Vector3(currentChunck.x + distX, 0, currentChunck.y + distZ), chunckData);
                objGen.transform.parent = mapTerrain.transform;
            }
    }*/

    /*private void createMarginsForNoiseMap(Vector2Int coordinates) {
        float[,] tempNoiseMap;
        chunckNoiseMap.noiseMap = noiseMapDictionary[coordinates].noiseMap;

        tempNoiseMap = noiseMapDictionary[coordinates + new Vector2Int(dx[0], dy[0])].noiseMap;
        for (int y = 1; y <= chunckArea; y++)
            chunckNoiseMap.noiseMap[0, y] = tempNoiseMap[chunckArea, y];

        tempNoiseMap = noiseMapDictionary[coordinates + new Vector2Int(dx[1], dy[1])].noiseMap;
        for (int y = 1; y <= chunckArea; y++)
            chunckNoiseMap.noiseMap[chunckArea + 1, y] = tempNoiseMap[1, y];

        tempNoiseMap = noiseMapDictionary[coordinates + new Vector2Int(dx[2], dy[2])].noiseMap;
        for (int x = 1; x <= chunckArea; x++)
            chunckNoiseMap.noiseMap[x, 0] = tempNoiseMap[x, chunckArea];

        tempNoiseMap = noiseMapDictionary[coordinates + new Vector2Int(dx[3], dy[3])].noiseMap;
        for (int x = 1; x <= chunckArea; x++)
            chunckNoiseMap.noiseMap[x, chunckArea + 1] = tempNoiseMap[x, 1];

        noiseMapDictionary.Remove(coordinates);
        noiseMapDictionary.Add(coordinates, chunckNoiseMap);
    }*/

    private void createNoiseMap(Vector2 addOffSet) {
        noiseMap = noise.GenerateNoiseMap(chunckArea, chunckArea, seed, noiseScale, octaves, persistance, lacunarity, offSet + addOffSet);
    }

    private void generateColour(Vector2Int coordinates) {
        colourMap =  new Color[chunckArea * chunckArea];

        for (int y = 0; y < chunckArea; y++) {
            for (int x = 0; x < chunckArea; x++) {
                float currentHeight = noiseMap[x + 1, y + 1];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].height) {
                        colourMap[y * chunckArea + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }
    }

    public void placeBlock(byte typeBlock, int indexTriangle, GameObject chunckObj) {
        Vector2Int chunckCoordinates = new Vector2Int((int)chunckObj.transform.position.x / 16, (int)chunckObj.transform.position.z / 16);
        ChunckData chunckData = terrainDictionary[new Vector2Int(chunckCoordinates.x, chunckCoordinates.y)];

        if (3 * indexTriangle >= chunckData.triangles[0].Count)
            return;

        byte dir = chunckData.faceDirection[0][3 * indexTriangle];

        if (dir < 2)
            dir += 2;
        else if (dir < 4)
            dir += 2;
        else dir -= 4;

        int x = chunckData.blockTriangles[0][3 * indexTriangle].x + dx[dir];
        int y = chunckData.blockTriangles[0][3 * indexTriangle].y + dy[dir];
        int z = chunckData.blockTriangles[0][3 * indexTriangle].z + dz[dir];

        if (x < 1 || x > 16 || z < 1 || z > 16) {
            createBlockInNextChunck(chunckCoordinates, x, y, z, dir, true);
        } else {
            createBlock(chunckCoordinates, new Vector3Int(x, y, z), typeBlock);
        }
    }

    public void destroyBlock(int indexTriangle, GameObject chunckObj) {
        Vector2Int chunckCoordinates = new Vector2Int((int)chunckObj.transform.position.x / 16, (int)chunckObj.transform.position.z / 16);
        ChunckData chunckData = terrainDictionary[new Vector2Int(chunckCoordinates.x, chunckCoordinates.y)];

        if(3 * indexTriangle >= chunckData.triangles[0].Count)
            return;

        Vector3Int blockCoordinates = chunckData.blockTriangles[0][3 * indexTriangle];
        chunckData.blockType[blockCoordinates.x, blockCoordinates.y, blockCoordinates.z] = 0;

        Mesh mesh = chunckObj.GetComponent<MeshFilter>().mesh;

        int length = chunckData.triangles[0].Count;
        byte[] oldFaceDirection = chunckData.faceDirection[0].ToArray();
        int[] oldTriangle = chunckData.triangles[0].ToArray();
        Vector3Int[] newBlockTriangle = chunckData.blockTriangles[0].ToArray();

        chunckData.triangles[0].Clear();
        chunckData.faceDirection[0].Clear();
        chunckData.blockTriangles[0].Clear();

        for (int d = 0; d < 6; d++)
            chunckData.existFace[blockCoordinates.x, blockCoordinates.y, blockCoordinates.z, d] = false;

        for (int i = 0; i < length; i += 3) {
            if(blockCoordinates != newBlockTriangle[i]) {
                chunckData.triangles[0].Add(oldTriangle[i]);
                chunckData.triangles[0].Add(oldTriangle[i + 1]);
                chunckData.triangles[0].Add(oldTriangle[i + 2]);

                chunckData.faceDirection[0].Add(oldFaceDirection[i]);
                chunckData.faceDirection[0].Add(oldFaceDirection[i + 1]);
                chunckData.faceDirection[0].Add(oldFaceDirection[i + 2]);

                chunckData.blockTriangles[0].Add(newBlockTriangle[i]);
                chunckData.blockTriangles[0].Add(newBlockTriangle[i + 1]);
                chunckData.blockTriangles[0].Add(newBlockTriangle[i + 2]);
            }
        }

        for(byte d = 0; d < 6; d++) {
            int x = blockCoordinates.x + dx[d];
            int y = blockCoordinates.y + dy[d];
            int z = blockCoordinates.z + dz[d];

            if (chunckData.blockType[x, y, z] == 0)
                continue;

            if(x < 1 || z < 1 || x > 16 || z > 16) {
                createBlockInNextChunck(chunckCoordinates, x, y, z, d, false);
            } else {
                terrainGeneration.generateFaces(chunckData, x, y, z);
            }
        }

        mesh.vertices = chunckData.vertices.ToArray();
        mesh.uv = chunckData.uvs.ToArray();
        mesh.SetTriangles(chunckData.triangles[0].ToArray(), 0);
        mesh.SetTriangles(chunckData.triangles[1].ToArray(), 1);
        mesh.RecalculateNormals();

        chunckObj.GetComponent<MeshCollider>().sharedMesh = mesh;
        chunckObj.GetComponent<MeshCollider>().sharedMaterial = physicMaterial;
    }

    private void createBlockInNextChunck(Vector2Int chunckCoordinates, int x, int y, int z, byte d, bool typeOfOperation) {
        //typeOfOperation : 1 - destroy   //   2 - place

        int xChunck = chunckCoordinates.x + dx[d];
        int yChunck = chunckCoordinates.y + dz[d];

        if (x < 1 || x > 16) {
            int newX = chunckArea + 1;
            if (dx[d] > 0)
                newX = 0;

            ChunckData nextChunck = terrainDictionary[new Vector2Int(xChunck, yChunck)];
            if(!typeOfOperation)
                nextChunck.blockType[newX, y, z] = 0;

            newX = chunckArea;
            if (dx[d] > 0)
                newX = 1;

            if(!typeOfOperation)
                createBlock(new Vector2Int(xChunck, yChunck), new Vector3Int(newX, y, z), nextChunck.blockType[newX, y, z]);
            else createBlock(new Vector2Int(xChunck, yChunck), new Vector3Int(newX, y, z), 2);
        }

        if (z < 1 || z > 16) {
            int newZ = chunckArea + 1;
            if (dz[d] > 0)
                newZ = 0;

            ChunckData nextChunck = terrainDictionary[new Vector2Int(xChunck, yChunck)];
            if (!typeOfOperation) 
                nextChunck.blockType[x, y, newZ] = 0;

            newZ = chunckArea;
            if (dz[d] > 0)
                newZ = 1;
            if (!typeOfOperation)
                createBlock(new Vector2Int(xChunck, yChunck), new Vector3Int(x, y, newZ), nextChunck.blockType[x, y, newZ]);
            else createBlock(new Vector2Int(xChunck, yChunck), new Vector3Int(x, y, newZ), 2);
        }
    }

    public void createBlock(Vector2Int chunckCoordinates, Vector3Int blockCoordinates, byte typeBlock) {
        ChunckData chunckData = terrainDictionary[chunckCoordinates];
        GameObject chunckObj = GameObject.Find("chunckID(" + chunckCoordinates.x + "," + chunckCoordinates.y + ")");

        chunckData.blockType[blockCoordinates.x, blockCoordinates.y, blockCoordinates.z] = typeBlock;
        terrainGeneration.generateFaces(chunckData, blockCoordinates.x, blockCoordinates.y, blockCoordinates.z);

        Mesh mesh = chunckObj.GetComponent<MeshFilter>().mesh;

        mesh.vertices = chunckData.vertices.ToArray();
        mesh.uv = chunckData.uvs.ToArray();
        mesh.SetTriangles(chunckData.triangles[0].ToArray(), 0);
        mesh.SetTriangles(chunckData.triangles[1].ToArray(), 1);
        mesh.RecalculateNormals();

        chunckObj.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}

[System.Serializable]
public struct TerrainType{
    public string name;
    public float height;
    public Color colour;
}
 