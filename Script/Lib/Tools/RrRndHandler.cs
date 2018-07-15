using UnityEngine;

//-----------------------------------------------------------------------------
// Created: Pascal Dubois 17-01-2014
// from P:\Dev\Library\lwh\impl\Flash\src\lwSys\RndHandler.as
//-----------------------------------------------------------------------------
public sealed class RrRndHandler
{
    static private uint m_nSeed = 0;

    static public void Init()
    {
        m_nSeed = (uint)(Random.value * 0x7FFFFFFF);
    }

    static public void RndSeed(uint nSeed)
    {
        m_nSeed = nSeed;
    }

    static public double Rnd()
    {
        m_nSeed = (uint)(((double)m_nSeed * 16807) % int.MaxValue);
        return ((double)m_nSeed / (double)0x7FFFFFFF) + 0.000000000233;
    }

    static public uint RndRange(uint nMin, uint nMax)
    {
        return (uint)(nMin + (nMax - nMin) * Rnd());
    }
    static public int RndRange(int nMin, int nMax)
    {
        return (int)(nMin + (nMax - nMin) * Rnd());
    }
    static public float RndRange(float fMin, float fMax)
    {
        return (float)(fMin + (fMax - fMin) * Rnd());
    }
    //
}