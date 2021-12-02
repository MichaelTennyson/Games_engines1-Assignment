using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NewBehaviourScript 
{
    public static Texture2D TextureFromColourMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
   
}
