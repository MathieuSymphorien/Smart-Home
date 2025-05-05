using UnityEngine;

public class BuildModeEvents : MonoBehaviour
{
    public static System.Action<bool> OnBuildMode;  // true = Build, false = Play

    public static bool IsInBuildMode { get; private set; }

    public void SwitchToBuildMode() { IsInBuildMode = true;  OnBuildMode?.Invoke(true); }
    public void SwitchToPlayMode () { IsInBuildMode = false; OnBuildMode?.Invoke(false); }

}
