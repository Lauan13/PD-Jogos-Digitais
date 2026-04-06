using UnityEngine;

[CreateAssetMenu(fileName = "BootPlaySettings", menuName = "Boot/BootPlaySettings")]
public class BootPlaySettings : ScriptableObject
{
    // Path to the scene that should be loaded after _Boot runs (e.g. "Assets/Scenes/MyScene.unity")
    public string targetScenePath;
}

