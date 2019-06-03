using UnityEngine;
using UnityEditor;

public static class CreateScriptableObject
{
    public static Object Create(System.Type scriptableObjectClassType, string assetsFolder, string name)
    {
        var asset = ScriptableObject.CreateInstance(scriptableObjectClassType);

        AssetDatabase.CreateAsset(asset, "Assets/"+assetsFolder+"/"+name+".asset");
        AssetDatabase.SaveAssets();

        //pops to the newly created object
        //EditorUtility.FocusProjectWindow();

        //selects the object
        //Selection.activeObject = asset;

        return asset;
    }

    public static Object Create(ref ScriptableObject asset, string assetsFolder)
    {
        AssetDatabase.CreateAsset(asset, "Assets/" + assetsFolder + "/" + asset.name + ".asset");
        AssetDatabase.SaveAssets();

        return asset;
    }
}