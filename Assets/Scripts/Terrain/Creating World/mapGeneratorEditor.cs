using UnityEngine;
using UnityEditor;
using System;

[CustomEditor (typeof(mapGenerator))]

public class mapGeneratorEditor : Editor {

    private Vector2 lastChunck = Vector2.zero;

    private float chunckArea = 160;

    public override void OnInspectorGUI() {
        GameObject obj = GameObject.Find("chunckForEditor");

        mapGenerator mapGen = (mapGenerator)target;

        if (checkLastAndCurrent()) {
            mapGen.updateMapTerrain(obj, Vector3.zero, false);
        }

        if (DrawDefaultInspector()) {
            mapGen.updateMapTerrain(obj, Vector3.zero, false);
            //mapGen.generateBlockTest();
        }

        if (GUILayout.Button("Generate")) {
            mapGen.updateMapTerrain(obj, Vector3.zero, false);
        }
        if (GUILayout.Button("Reset Position")) {
            GameObject terrainGen = GameObject.Find("Terrain Generator");
            terrainGen.transform.position = Vector3.zero;
        }
    }

    private bool checkLastAndCurrent() {
        GameObject terrainGen = GameObject.Find("Terrain Generator");
        bool ok = false;

        Vector2 currentChunck = new Vector2(Mathf.Round(terrainGen.transform.position.x / chunckArea), Mathf.Round(terrainGen.transform.position.z / chunckArea));
        if(currentChunck != lastChunck) {
            lastChunck = currentChunck;
            ok = true;
        }

        return ok;
    }

}
