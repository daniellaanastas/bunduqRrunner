using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    public float RotationPerSecond = 2;
    private int frameCounter = 0;
    private const int FRAMES_TO_SKIP = 3;
    protected void Update()
    {
        frameCounter++;
        if (frameCounter % FRAMES_TO_SKIP == 0)
        {
            RenderSettings.skybox.SetFloat("_Rotation", Time.time * RotationPerSecond);
        }
    }
}