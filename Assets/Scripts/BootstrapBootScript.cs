using UnityEngine;

public class BootstrapBootScript : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
    }
}
