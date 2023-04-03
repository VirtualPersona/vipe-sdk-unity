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
        FileUtil.ReplaceFile("Assets/SimplestarGame/SimpleURPToonLitOutlineExample/Scripts/MaterialFactory.cs", "Assets/VRMShaders/GLTF/IO/Runtime/Material/Importer/MaterialFactory.cs");
        AssetDatabase.Refresh();

        // Create a backup of the original MaterialFactory script if it doesn't exist
        string backupPath = "Backup/MaterialFactoryBackup.cs";
        if (!File.Exists(backupPath))
        {
            FileUtil.CopyFileOrDirectory("Assets/VRMShaders/GLTF/IO/Runtime/Material/Importer/MaterialFactory.cs", backupPath);
        }

        GraphicsSettings.renderPipelineAsset = null;
        Debug.Log("Switched to Built In Pipeline");
    }

    private void SwitchToURPPipeline()
    {
        // Replace MaterialFactory script when switching to URP Pipeline
        FileUtil.ReplaceFile("Assets/VRMShaders/GLTF/IO/Runtime/Material/Importer/MaterialFactory.cs", "Assets/SimplestarGame/SimpleURPToonLitOutlineExample/Scripts/MaterialFactory.cs");
        AssetDatabase.Refresh();

        // Restore the original MaterialFactory script from the backup
        string backupPath = "Backup/MaterialFactoryBackup.cs";
        if (File.Exists(backupPath))
        {
            FileUtil.ReplaceFile(backupPath, "Assets/VRMShaders/GLTF/IO/Runtime/Material/Importer/MaterialFactory.cs");
        }

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