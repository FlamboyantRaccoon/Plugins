using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

public sealed class RRGeometry
{

    public static bool ComputeSegmentIntersection(Vector2 v1a, Vector2 v1b, Vector2 v2a, Vector2 v2b, ref Vector2 vInter)
    {
        Vector2 v1 = v1b - v1a;
        Vector2 v2 = v2b - v2a;

        float fDiv = v1.x * v2.y - v2.x * v1.y;

        if( fDiv==0 )
        {
            return false;
        }

        float m = -(-v1.x * v1a.y + v1.x * v2a.y + v1.y * v1a.x - v1.y * v2a.x) / fDiv;
        float k = -(v2.y * v1a.x - v2.y * v2a.x - v2.x * v1a.y + v2.x * v2a.y) / fDiv;

        if( m>0f && m<=1f && k>0f && k <=1f )
        {
            vInter = v1a + k * v1;
            return true;
        }
        return false;
    }

    public static void ComputeLineEquation(Vector2 va, Vector2 vb, out float a, out float b)
    {
        a = vb.x != va.x ? (vb.y - va.y) / (vb.x - va.x) : 0f;
        b = va.y - a * va.x;
    }

    public static void ComputeSegmentIntersectionWithVerticalLine(float a, float b, float fX, ref Vector2 vInter)
    {
        float fY = a * fX + b;
        vInter = new Vector2(fX, fY);
    }
}
