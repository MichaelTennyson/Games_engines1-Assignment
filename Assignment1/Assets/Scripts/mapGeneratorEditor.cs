using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (mapGenerator))]
public class mapGeneratorEditor : Editor
{
   public override void OnInspectorGUI()
    {
        mapGenerator mapGen = (mapGenerator)target;

        DrawDefaultInspector();

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.DrawMapEditor();
            }
        }

        //if the generate button is clicked
        if (GUILayout.Button("Generate"))
        {
            mapGen.DrawMapEditor();
        }
    }
}
