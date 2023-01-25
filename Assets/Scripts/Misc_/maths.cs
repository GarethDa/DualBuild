using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class maths 
{
    public static float nfmod(float a, float b)
    {
        return (Mathf.Abs(a * b) + a) % b;
    }

    public static int nfmod(int a, int b)
    {
        return (Mathf.Abs(a * b) + a) % b;
    }

    public static float lerp(float p0, float p1, float t)
{
	return (1.0f - t) * p0 + t* p1;
}

    public static Vector3 Catmull(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;

        return 0.5f * (2.0f * p1 + t * (-p0 + p2)
            + t * t * (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3)
            + t * t * t * (-p0 + 3.0f * p1 - 3.0f * p2 + p3));
    }


    public static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 l10 = Vector3.Lerp(p1, p0, t);
        Vector3 l32 = Vector3.Lerp(p3, p2, t);
        Vector3 l03 = Vector3.Lerp(p0, p3, t);

        Vector3 l10_l03 = Vector3.Lerp(l10, l03, t);
        Vector3 l10_l32 = Vector3.Lerp(l10, l32, t);

        Vector3 finalL = Vector3.Lerp(l10_l03, l10_l32, t);

        return finalL;

    }

}
