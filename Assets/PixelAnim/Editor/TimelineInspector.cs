using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PixelEditor.Timeline))]
[CanEditMultipleObjects]
public class TimelineInspector : Editor
{
    private void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        //base.DrawDefaultInspector();
        GUILayout.Space(10);
        GUILayout.Label("Edit Timelines with PixelAnim:\nWindow > PixelAnim > Scissors Icon (✂)");
        GUILayout.Space(10);
        
    }
}


