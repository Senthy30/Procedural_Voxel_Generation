using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;

public class TerrainGeneration : MonoBehaviour{

    private GameObject terrain;
    public GameObject centerObject, cube;

    public float textureSize;

    private sbyte[] dx = new sbyte[6] { 0, 0, 0, 0, 1, -1 };
    private sbyte[] dy = new sbyte[6] { 1, -1, 0, 0, 0, 0 };
    private sbyte[] dz = new sbyte[6] { 0, 0, 1, -1, 0, 0 };

    private Vector2[] correctOffSide = new Vector2[6] { new Vector2(0.001f, 0.001f), new Vector2(0.001f, 0.099f), new Vector2(0.099f, 0.001f), new Vector2(0.099f, 0.001f), new Vector2(0.001f, 0.099f), new Vector2(0.099f, 0.099f) };

    public List<Vector3> posFT;
    public List<TextureBlock> textureBlock;

    public PhysicMaterial physicMaterial;
    private MeshCollider meshCollider;

    public GameObject generateTerrain(int chunckArea, int chunckHeight, Vector3 posChunck, ChunckData chunckData) {

        chunckData.existFace = new bool[chunckArea + 2, chunckHeight + 2, chunckArea + 2, 6];

        chunckData.blockTriangles[0] = new List<Vector3Int>();
        chunckData.blockTriangles[1] = new List<Vector3Int>();

        chunckData.faceDirection[0] = new List<byte>();
        chunckData.faceDirection[1] = new List<byte>();

        chunckData.uvs = new List<Vector2>();
        chunckData.vertices = new List<Vector3>();

        chunckData.triangles[0] = new List<int>();
        chunckData.triangles[1] = new List<int>();

        terrain = Instantiate(centerObject, posChunck * chunckArea, Quaternion.identity);
        terrain.SetActive(true);
        terrain.name = "chunckID(" + posChunck.x + "," + posChunck.z + ")";

        chunckData.cntTriangles = 0;
        for (int x = 1; x <= chunckArea; x++)
            for (int z = 1; z <= chunckArea; z++)
                for (int y = 1; y < chunckHeight; y++) {
                    if (chunckData.blockType[x, y, z] == 0)
                        continue;

                    generateFaces(chunckData, x, y, z);
                }

        addToTerrainTriangles(terrain, chunckData);

        return terrain;
    }

    public void generateFaces(ChunckData chunckData, int x, int y, int z) {
        byte currentBlockType = chunckData.blockType[x, y, z];
        byte isTransparent = 0;

        Vector3Int coordinates = new Vector3Int(x, y, z);

        if (chunckData.blockType[x, y, z] == 1) {
            isTransparent = 1;
        }

        for (byte d = 0; d < 6; d++) {
            if (chunckData.existFace[x, y, z, d])
                continue;

            byte nextBlockType = chunckData.blockType[x + dx[d], y + dy[d], z + dz[d]];

            if ((textureBlock[nextBlockType].canSeeThrough && !textureBlock[currentBlockType].canSeeThrough) || (textureBlock[currentBlockType].canSeeThrough && nextBlockType == 0) || (currentBlockType == 7)) {

                chunckData.existFace[x, y, z, d] = true;
                for (int i = 0; i < 6; i++) {
                    chunckData.vertices.Add(coordinates + posFT[d * 6 + i]);
                    chunckData.triangles[isTransparent].Add(chunckData.cntTriangles + i);
                    chunckData.blockTriangles[isTransparent].Add(coordinates);
                    chunckData.faceDirection[isTransparent].Add(d);
                }
                chunckData.cntTriangles += 6;

                if (d == 0) {

                    for (byte i = 0; i < 6; i++)
                        chunckData.uvs.Add((Vector2.right * textureBlock[currentBlockType].up.x + Vector2.up * textureBlock[currentBlockType].up.y) * textureSize + correctOffSide[i]);

                } else if (d == 1) {

                    for (byte i = 0; i < 6; i++)
                        chunckData.uvs.Add((Vector2.right * textureBlock[currentBlockType].down.x + Vector2.up * textureBlock[currentBlockType].down.y) * textureSize + correctOffSide[i]);

                } else {

                    for (byte i = 0; i < 6; i++)
                        chunckData.uvs.Add((Vector2.right * textureBlock[currentBlockType].side.x + Vector2.up * textureBlock[currentBlockType].side.y) * textureSize + correctOffSide[i]);

                }
            }
        }
    }

    private void addToTerrainTriangles(GameObject terrain, ChunckData chunckData) {

        Mesh mesh = new Mesh();
        terrain.GetComponent<MeshFilter>().mesh = mesh;
        mesh.subMeshCount = 2;

        mesh.vertices = chunckData.vertices.ToArray();
        mesh.uv = chunckData.uvs.ToArray();
        mesh.SetTriangles(chunckData.triangles[0].ToArray(), 0);
        mesh.SetTriangles(chunckData.triangles[1].ToArray(), 1);

        meshCollider = terrain.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.sharedMaterial = physicMaterial;

        mesh.RecalculateNormals();
    }

}

[System.Serializable]
public struct TextureBlock {
    public string name;
    public int ID;
    public bool canSeeThrough;

    public Vector2Int up, down, side;
}
