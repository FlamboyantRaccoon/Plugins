using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class lwMinMaxVectorAttribute : PropertyAttribute 
{
	public float fMinValue { get { return m_fMinValue; } }
	public float fMaxValue { get { return m_fMaxValue; } }
	public bool bUseInteger { get { return m_bUseInteger; } }
	
	private readonly float m_fMinValue;
	private readonly float m_fMaxValue;
	private readonly bool m_bUseInteger;

	public lwMinMaxVectorAttribute( float fMinValue, float fMaxValue, bool bUseInteger=false )
	{
		lwTools.Assert( fMinValue<=fMaxValue );
		m_fMinValue = fMinValue;
		m_fMaxValue = fMaxValue;
		m_bUseInteger = bUseInteger;
	}
}
