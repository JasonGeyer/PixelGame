using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PixelEditor.Parameter))]
public class ParameterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Edit Transition Parameters with PixelAnim:\nWindow > PixelAnim > Scissors Icon (✂) > Transitions ");
        GUILayout.Space(10);
    }
}
