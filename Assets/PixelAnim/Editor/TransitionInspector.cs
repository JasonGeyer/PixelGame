using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PixelEditor.Transition))]
[CanEditMultipleObjects]
public class TransitionInspector : Editor
{
    SerializedProperty targetParameter;
    SerializedProperty numberOfConditions;
    SerializedProperty ComparisonTypes;

    private void OnEnable()
    {
        targetParameter = serializedObject.FindProperty("targetParameter");

        numberOfConditions = serializedObject.FindProperty("numberOfConditions");

        ComparisonTypes = serializedObject.FindProperty("ComparisonTypes");

    }

    public override void OnInspectorGUI()
    {
        //base.DrawDefaultInspector();
        GUILayout.Space(10);
        GUILayout.Label("Edit Transitions with PixelAnim:\nWindow > PixelAnim > Scissors Icon (✂) > Transitions > Edit");
        GUILayout.Space(10);
        
    }
}


