using UnityEngine;

public class FixPoint
{
    public static float Round(float num)
    {
        int f = (int)(num * 1000);
        return (float)f / 1000;
    }

    public static Vector3 Round(Vector3 vec)
    {
        int a = (int)(vec.x * 1000);
        int b = (int)(vec.y * 1000);
        int c = (int)(vec.z * 1000);
        return new Vector3((float)a / 1000, (float)b / 1000, (float)c / 1000);
    }
}
