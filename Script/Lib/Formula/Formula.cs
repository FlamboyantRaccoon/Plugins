// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/09

using UnityEngine;

//! @struct Equation
//!
//!	@brief	Equation
[System.Serializable]
public struct Formula
{
	public string stringValue
	{
		get{ return m_formulaString; }
	}
	
	[SerializeField]
	private string m_formulaString;
}
