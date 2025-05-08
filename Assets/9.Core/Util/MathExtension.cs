using UnityEngine;

public static class MathExtension
{
    public static bool IsAlmostEqaul(this float a, float b)
    {
        float big = a > b ? a : b;
        float small = a > b ? b : a;

        if (big - small < 0.0001f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static float Abs(this float a)
    {
        return Mathf.Abs(a);
    }
}
