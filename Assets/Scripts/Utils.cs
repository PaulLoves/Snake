using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static int CeilPower2(int x)
    {
        if (x < 2)
        {
            return 1;
        }
        return (int)Mathf.Pow(2, (int)Mathf.Log(x - 1, 2) + 1);
    }

    public static Vector3 ClampComponents(Vector3 v, float min, float max)
    {
        float x = Mathf.Clamp(v.x, min, max);
        float y = Mathf.Clamp(v.y, min, max);
        float z = Mathf.Clamp(v.z, min, max);
        return new Vector3(x, y, z);
    }

    public static bool OpposingVectors(Vector2Int v1, Vector2Int v2)
    {
        return Vector2.Dot(v1, v2) == -1;
    }
}
