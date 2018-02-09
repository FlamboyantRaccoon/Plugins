// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/05/27

#if !UNITY_FLASH || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! @class lwEnumArrayBaseClass
//!
//! @brief base class only used to display properly in Editor
public abstract class lwEnumArrayBaseClass
{
	
}

//! @class lwEnumArray
//!
//! @brief abstract base class for arrays that will be maintain consistency with an enumeration given as generic parameter
[System.Serializable]
public class lwEnumArray<t_Enum, t_Content> : lwEnumArrayBaseClass, IEnumerable<t_Content>, ISerializationCallbackReceiver where t_Enum : struct, IConvertible
{
	[SerializeField]
	private t_Content[] m_internalArray;

	// FIX : Prevent from different serialization in Editor & in Build
//#if UNITY_EDITOR
	[SerializeField]
	private string[] m_enumNames;
//#endif // UNITY_EDITOR

	//! access the length of the array
	public int nLength
	{
		get{ return m_internalArray.Length; }
	}

	//! access an element by its index in the enumeration
	//!
	//! @param	index	real index in the enumerations (not the value)
	public t_Content this[int nIndex]
	{
		get{ return m_internalArray[nIndex]; }
		set{ m_internalArray[nIndex] = value; }
	}

	//! access an element by its enumeration value
	//!
	//! @param enumValue	value of the enumeration
	public t_Content this[t_Enum enumValue]
	{
		get
		{
			int nIndex = System.Array.FindIndex( System.Enum.GetValues( typeof( t_Enum ) ) as t_Enum[], ( t_Enum o ) => { return o.Equals( enumValue ); } );
			return this[nIndex];
		}
		set
		{
			int nIndex = System.Array.FindIndex( System.Enum.GetValues( typeof( t_Enum ) ) as t_Enum[], ( t_Enum o ) => { return o.Equals( enumValue ); } );
			this[nIndex] = value;
		}
	}

	//! Constructor
	public lwEnumArray()
	{
		int nValueCount = System.Enum.GetValues( typeof( t_Enum ) ).Length;
		m_internalArray = new t_Content[nValueCount];

#if UNITY_EDITOR
		m_enumNames = System.Enum.GetNames( typeof( t_Enum ) );
#endif // UNITY_EDITOR
	}

	//! Copy constructor
	//!
	//!	@param	source	source to copy
	public lwEnumArray(lwEnumArray<t_Enum, t_Content> source) : this()
	{
		for( int nIndex = 0; nIndex<source.m_internalArray.Length; ++nIndex )
		{
			m_internalArray[nIndex] = source.m_internalArray[nIndex];
		}
	}

	//! Convert the enumeration array into an array
	//!
	//! @return a copy of the internal array in order to avoid modifications of the internal structure.
	public t_Content[] ToArray()
	{
		t_Content[] destinationArray = new t_Content[m_internalArray.Length];
		System.Array.Copy(m_internalArray, destinationArray, destinationArray.Length);
		return destinationArray;
	}

#region IEnumerable<t_Content>
	IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return m_internalArray.GetEnumerator();
	}

	IEnumerator<t_Content> System.Collections.Generic.IEnumerable<t_Content>.GetEnumerator()
	{
		return ( m_internalArray as IEnumerable<t_Content> ).GetEnumerator();
	}
#endregion

#region ISerializationCallbackReceiver
	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
#if UNITY_EDITOR
		string[] sEnumNames = System.Enum.GetNames( typeof( t_Enum ) );

		List<t_Content> contents = new List<t_Content>( m_internalArray );
		List<string> sNames = new List<string>( m_enumNames );
		for( int nIndex = 0; nIndex<sEnumNames.Length; ++nIndex )
		{
			if( nIndex<sNames.Count )
			{
				int nOccurenceIndex = sNames.FindIndex( nIndex, ( string s ) => { return s == sEnumNames[nIndex]; } );
				if( nOccurenceIndex>=0 )
				{
					if( nOccurenceIndex!=nIndex )
					{
						t_Content tempContent = contents[nIndex];
						string sTempName = sNames[nIndex];

						contents[nIndex] = contents[nOccurenceIndex];
						sNames[nIndex] = sNames[nOccurenceIndex];

						contents[nOccurenceIndex] = tempContent;
						sNames[nOccurenceIndex] = sTempName;
					}
				}
				else
				{
					contents.Insert( nIndex, default( t_Content ) );
					sNames.Insert( nIndex, sEnumNames[nIndex] );
				}
			}
			else
			{
				contents.Insert( nIndex, default( t_Content ) );
				sNames.Insert( nIndex, sEnumNames[nIndex] );
			}
		}

		lwTools.Assert( contents.Count==sNames.Count );

		if( sNames.Count>sEnumNames.Length )
		{
			contents.RemoveRange( sEnumNames.Length, sNames.Count-sEnumNames.Length );
			sNames.RemoveRange( sEnumNames.Length, sNames.Count-sEnumNames.Length );
		}

		lwTools.Assert( contents.Count==sNames.Count );
		lwTools.Assert( contents.Count==sEnumNames.Length );

		m_internalArray = contents.ToArray();
		m_enumNames = sNames.ToArray();
#else
		lwTools.Assert( m_internalArray.Length==System.Enum.GetValues( typeof( t_Enum ) ).Length, "The project needs to be recompiled and saved before building." );
#endif // UNITY_EDITOR
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}
#endregion
}

//! @class lwEnumArrayUtility
//!
//! @brief	Static class that adds utility methods for lwEnumArray
public static class lwEnumArrayUtility
{
	//! @brief Convert an array of booleans into an integer
	//!
	//! @return the array of booleans as an integer
	public static int ToInt<t_Enum>( this lwEnumArray<t_Enum, bool> sourceArray ) where t_Enum : struct, IConvertible
	{
		int nEnumerationLength = System.Enum.GetValues( typeof( t_Enum ) ).Length;
		lwTools.Assert( nEnumerationLength<=sizeof( int )*8 );

		int nResultValue = 0;
		for( int nEnumIndex = 0; nEnumIndex<nEnumerationLength; ++nEnumIndex )
		{
			if( sourceArray[nEnumIndex] )
			{
				nResultValue |= 1<<nEnumIndex;
			}
		}

		return nResultValue;
	}

	//! @brief Convert an array of booleans into an integer
	//!
	//! @param	source	array of booleans as an integer
	public static void FromInt<t_Enum>( this lwEnumArray<t_Enum, bool> array, int source ) where t_Enum : struct, IConvertible
	{
		int nEnumerationLength = System.Enum.GetValues( typeof( t_Enum ) ).Length;
		lwTools.Assert( nEnumerationLength<=sizeof(int)*8 );

		for( int nEnumIndex = 0; nEnumIndex<nEnumerationLength; ++nEnumIndex )
		{
			array[nEnumIndex] = ( source&( 1<<nEnumIndex ) )>0;
		}
	}

	//! @brief Convert an array of booleans into an unsigned integer
	//!
	//! @return the array of booleans as an unsigned integer
	public static uint ToUInt<t_Enum>( this lwEnumArray<t_Enum, bool> sourceArray ) where t_Enum : struct, IConvertible
	{
		int nEnumerationLength = System.Enum.GetValues( typeof( t_Enum ) ).Length;
		lwTools.Assert( nEnumerationLength<=sizeof( int )*8 );

		uint nResultValue = 0;
		for( int nEnumIndex = 0; nEnumIndex<nEnumerationLength; ++nEnumIndex )
		{
			if( sourceArray[nEnumIndex] )
			{
				nResultValue |= ( uint )( 1<<nEnumIndex );
			}
		}

		return nResultValue;
	}

	//! @brief Convert an array of booleans into an integer
	//!
	//! @param	source	array of booleans as an integer
	public static void FromUInt<t_Enum>( this lwEnumArray<t_Enum, bool> array, uint source ) where t_Enum : struct, IConvertible
	{
		int nEnumerationLength = System.Enum.GetValues( typeof( t_Enum ) ).Length;
		lwTools.Assert( nEnumerationLength<=sizeof( int )*8 );

		for( int nEnumIndex = 0; nEnumIndex<nEnumerationLength; ++nEnumIndex )
		{
			array[nEnumIndex] = ( source&( 1<<nEnumIndex ) )>0;
		}
	}
}
#endif
