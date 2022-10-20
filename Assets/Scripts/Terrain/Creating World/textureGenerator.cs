using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class textureGenerator {
    
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(int chunckArea, float [,] heightMap, bool typeGen, float lvl) {

        Color[] colourMap = new Color[chunckArea * chunckArea];
        for (int y = 0; y < chunckArea; y++) {
            for (int x = 0; x < chunckArea; x++) {
                if (typeGen) {
                    if (heightMap[x + 1, y + 1] >= lvl)
                        colourMap[y * chunckArea + x] = Color.white;
                    else colourMap[y * chunckArea + x] = Color.black;
                } else {
                    colourMap[y * chunckArea + x] = Color.Lerp(Color.black, Color.white, heightMap[x + 1, y + 1]);
                }
            }
        }

        return TextureFromColourMap(colourMap, chunckArea, chunckArea);
    }

}
