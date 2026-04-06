using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private void Start()
    {
        // Only run in Editor play mode path - but runtime in builds could also work if desired
        var settings = Resources.Load<BootPlaySettings>("BootPlaySettings");
        if (settings == null)
        {
            Debug.LogWarning("BootLoader: BootPlaySettings not found in Resources. Nothing to load.");
            return;
        }

        if (string.IsNullOrEmpty(settings.targetScenePath))
        {
            // Nothing to load
            return;
        }

        // Convert path to scene name (strip folders and extension)
        string scenePath = settings.targetScenePath;
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"BootLoader: Invalid scene path stored in BootPlaySettings: '{scenePath}'");
            settings.targetScenePath = string.Empty;
            return;
        }

        Debug.Log($"BootLoader: Loading target scene '{sceneName}' from path '{scenePath}'");

        // Clear the setting so if _Boot is used normally it won't re-load
        settings.targetScenePath = string.Empty;

        // Load the target scene and replace this one
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}

