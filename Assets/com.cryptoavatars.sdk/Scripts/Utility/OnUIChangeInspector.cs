using UnityEngine;
using UnityEditor;
using static System.Collections.Specialized.BitVector32;
using static UnityEditor.Progress;

[CustomEditor(typeof(UIModeSelector))]

public class OnUIChangeInspector : Editor
{
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        UIModeSelector myScript = (UIModeSelector)target;
        myScript.LoginUIMode = (LoginUIModes)EditorGUILayout.EnumPopup("UI Modes:", (LoginUIModes)myScript.LoginUIMode);
        //serializedObject.ApplyModifiedProperties();
    }

    
}

