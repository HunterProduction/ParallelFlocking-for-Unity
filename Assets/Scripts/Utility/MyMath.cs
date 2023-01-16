using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utility
{
    public static class MyMath
    {
        public static Vector3[] RandomPointsInSphere(int numPoints, Vector3 sphCenter, float sphRadius)
        {
            Vector3[] points = new Vector3[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                var theta = Random.Range(0f, 2f*(float)Math.PI);
                var z0 = Random.Range(-1f, 1f);
                var temp = Mathf.Sqrt(1 - z0 * z0);
                var x0 = temp * Mathf.Cos(theta);
                var y0 = temp * Mathf.Sin(theta);
                var r = Random.Range(0f, sphRadius);
                points[i] = sphCenter + new Vector3(x0, y0, z0) * r;
            }
            return points;
        }
    }
}