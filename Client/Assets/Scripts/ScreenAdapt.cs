using UnityEngine;
using UnityEngine.UI;

public class ScreenAdapt : MonoBehaviour
{
    private void Awake()
    {
        FixResolution();
    }

    private void FixResolution()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();

        float sWToH = scaler.referenceResolution.x / scaler.referenceResolution.y;
        float vWToH = Screen.width * 1.0f / Screen.height;

        if (sWToH > vWToH)
        {
            scaler.matchWidthOrHeight = 0;
        }
        else
        {
            scaler.matchWidthOrHeight = 1;
        }
    }
}
