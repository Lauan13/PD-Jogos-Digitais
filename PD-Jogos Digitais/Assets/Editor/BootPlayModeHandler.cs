using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

[InitializeOnLoad]
public static class BootPlayModeHandler
{
    private const string bootSceneName = "_Boot";
    private const string resourcesAssetName = "BootPlaySettings"; // Resources/BootPlaySettings.asset

    static BootPlayModeHandler()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // About to enter Play
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == bootSceneName)
                return; // already boot

            // Find the _Boot scene in the project (Assets)
            string[] guids = AssetDatabase.FindAssets("t:Scene " + bootSceneName);
            string bootScenePath = null;

            // The FindAssets query above might not find by name alone, so search all scenes and match name.
            if (guids == null || guids.Length == 0)
            {
                var sceneGuids = AssetDatabase.FindAssets("t:Scene");
                foreach (var g in sceneGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    if (System.IO.Path.GetFileNameWithoutExtension(path) == bootSceneName)
                    {
                        bootScenePath = path;
                        break;
                    }
                }
            }
            else
            {
                // Try to find a matching name among results
                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    if (System.IO.Path.GetFileNameWithoutExtension(path) == bootSceneName)
                    {
                        bootScenePath = path;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(bootScenePath))
            {
                Debug.LogWarning("BootPlayModeHandler: _Boot scene not found in project. Play will proceed normally.");
                return;
            }

            // Ensure boot scene is in Build Settings
            var buildScenes = EditorBuildSettings.scenes.ToList();
            if (!buildScenes.Any(s => s.path == bootScenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(bootScenePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
                Debug.Log($"BootPlayModeHandler: Added '{bootScenePath}' to Build Settings.");
            }

            // Ensure active scene is in Build Settings so it can be loaded by name
            var activePath = activeScene.path;
            if (!buildScenes.Any(s => s.path == activePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(activePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
                Debug.Log($"BootPlayModeHandler: Added '{activePath}' to Build Settings.");
            }

            // Set PlayMode start scene to boot
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootScenePath);

            // Write target scene path into BootPlaySettings asset in Resources
            var settings = Resources.Load<BootPlaySettings>(resourcesAssetName);
            if (settings == null)
            {
                // Try to create the asset at Assets/Resources
                string resourcesFolder = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(resourcesFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                var newSettings = ScriptableObject.CreateInstance<BootPlaySettings>();
                newSettings.targetScenePath = activePath;
                AssetDatabase.CreateAsset(newSettings, resourcesFolder + "/" + resourcesAssetName + ".asset");
                AssetDatabase.SaveAssets();
                Debug.Log($"BootPlayModeHandler: Created BootPlaySettings in {resourcesFolder} with target '{activePath}'.");
            }
            else
            {
                settings.targetScenePath = activePath;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                Debug.Log($"BootPlayModeHandler: Set BootPlaySettings.targetScenePath = '{activePath}'.");
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            // Clear the play start scene and the settings asset
            if (EditorSceneManager.playModeStartScene != null)
            {
                EditorSceneManager.playModeStartScene = null;
            }

            var settings = Resources.Load<BootPlaySettings>(resourcesAssetName);
            if (settings != null)
            {
                settings.targetScenePath = string.Empty;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

