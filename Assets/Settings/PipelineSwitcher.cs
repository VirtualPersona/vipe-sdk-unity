using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.IO;

public class PipelineSwitcher : EditorWindow
{
    [MenuItem("Window/Pipeline Switcher")]
    public static void ShowWindow()
    {
        GetWindow<PipelineSwitcher>("Pipeline Switcher");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a Pipeline:", EditorStyles.boldLabel);

        if (GUILayout.Button("Built In Pipeline"))
        {
            SwitchToBuiltInPipeline();
        }

        if (GUILayout.Button("URP Pipeline"))
        {
            SwitchToURPPipeline();
        }
    }

    private void SwitchToBuiltInPipeline()
    {
        // Replace MaterialFactory script when switching to Built-In Pipeline
        string sourcePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "PipelineSwitcherFiles/BuiltInPipeline/MaterialFactory.cs");
        string destinationPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library/PackageCache/com.vrmc.vrmshaders@6ea7819cec/GLTF/IO/Runtime/Material/Importer/MaterialFactory.cs");
        File.Copy(sourcePath, destinationPath, true);
        AssetDatabase.Refresh();

        GraphicsSettings.renderPipelineAsset = null;
        Debug.Log("Switched to Built In Pipeline");
    }

    private void SwitchToURPPipeline()
    {
        // Replace MaterialFactory script when switching to URP Pipeline
        string sourcePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "PipelineSwitcherFiles/URPPipeline/MaterialFactory.cs");
        string destinationPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library/PackageCache/com.vrmc.vrmshaders@6ea7819cec/GLTF/IO/Runtime/Material/Importer/MaterialFactory.cs");
        File.Copy(sourcePath, destinationPath, true);
        AssetDatabase.Refresh();

        var urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/MyURPAsset.asset");
        if (urpAsset == null)
        {
            // Create a new URP asset if one doesn't exist
            urpAsset = UniversalRenderPipelineAsset.Create();
            AssetDatabase.CreateAsset(urpAsset, "Assets/MyURPAsset.asset");
            AssetDatabase.SaveAssets();
        }
        GraphicsSettings.renderPipelineAsset = urpAsset;
        Debug.Log("Switched to URP Pipeline");
    }
}
