// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/10/26

#if !UNITY_FLASH || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! @class lwEnumFlagBaseClass
//!
//! @brief base class only used to display properly in Editor
public abstract class lwEnumFlagBaseClass
{
}

//! @class lwEnumFlag
//!
//! @brief abstract base class for arrays that will be maintain consistency with an enumeration given as generic parameter
[System.Serializable]
public class lwEnumFlag<t_Enum> : lwEnumFlagBaseClass, IEnumerable<KeyValuePair<t_Enum, bool>>, ISerializationCallbackReceiver where t_Enum : struct, IConvertible
{
	public static readonly t_Enum[] ENUM_VALUES = System.Enum.GetValues(typeof(t_Enum)) as t_Enum[];
	public static readonly int COUNT = ENUM_VALUES.Length;
	public static readonly int ARRAY_LENGTH = COUNT/( sizeof( byte ) * 8 ) + ( ( COUNT%( sizeof( byte ) * 8 ) )>0? 1 : 0 );

	[SerializeField]
	private byte[] m_nValues;

	// FIX : Prevent from different serialization in Editor & in Build
//#if UNITY_EDITOR
	[SerializeField]
	private string[] m_enumNames;
//#endif // UNITY_EDITOR

	//! @class Enumerator
	//!
	//!	@brief	Enumerator structure of this collection
	public class Enumerator : IEnumerator<KeyValuePair<t_Enum, bool>>
	{
		//! Accessor to the current item of the collection
		public KeyValuePair<t_Enum, bool> Current
		{
			get{ return m_currentItem; }
		}

		object IEnumerator.Current
		{
			get{ return Current; }
		}

		//! Construct an enumerator based on the given collection
		//!
		//!	@param	collection	collection we enumerates
		public Enumerator( lwEnumFlag<t_Enum> collection )
		{
			lwTools.Assert( collection!=null );
			m_collection = collection;
			Reset();
		}

		//! Move to the next item of the collection
		//!
		//!	@return true if the next item exists, false otherwise
		public bool MoveNext()
		{
			++m_nCurrentIndex;
			if( m_nCurrentIndex>=COUNT )
			{
				return false;
			}
			else
			{
				m_currentItem = new KeyValuePair<t_Enum, bool>( ENUM_VALUES[m_nCurrentIndex], m_collection[m_nCurrentIndex] );
				return true;
			}
		}

		//!	Reset the enumerator to its initial position
		public void Reset()
		{
			m_nCurrentIndex = -1;
			m_currentItem = default( KeyValuePair<t_Enum, bool> );
		}

		void IDisposable.Dispose() {}

#region Private
	#region Attributes
		private lwEnumFlag<t_Enum> m_collection;
		private int m_nCurrentIndex;
		private KeyValuePair<t_Enum, bool> m_currentItem;
	#endregion
#endregion
	}

	//! access the value of the flag by its index
	//!
	//! @param	index	real index in the enumerations (not the value)
	public bool this[int nIndex]
	{
		get
		{
			return GetFlagValue( m_nValues, nIndex );
		}
		set
		{
			SetFlagValue( ref m_nValues, nIndex, value );
		}
	}

	//! access the value of the by its enumeration value
	//!
	//! @param enumValue	value of the enumeration
	public bool this[t_Enum enumValue]
	{
		get
		{
			int nIndex = System.Array.FindIndex( ENUM_VALUES, ( t_Enum o ) => { return o.Equals( enumValue ); } );
			return this[nIndex];
		}
		set
		{
			int nIndex = System.Array.FindIndex( ENUM_VALUES, ( t_Enum o ) => { return o.Equals( enumValue ); } );
			this[nIndex] = value;
		}
	}

	//! Default constructor
	public lwEnumFlag()
	{
		m_nValues = new byte[ARRAY_LENGTH];

#if UNITY_EDITOR
		m_enumNames = System.Enum.GetNames( typeof( t_Enum ) );
#endif // UNITY_EDITOR
	}

	//! Copy constructor
	//!
	//!	@param	source	copy source
	public lwEnumFlag( lwEnumFlag<t_Enum> source ) : this()
	{
		System.Array.Copy( source.m_nValues, m_nValues, m_nValues.Length );
	}

	//! Constructor with default value
	//!
	//!	@param	bValue	default value
	public lwEnumFlag( bool bValue ) : this()
	{
		SetAll(bValue);
	}

	//! Constructor with specific flags
	//!
	//!	@param	flags	flags that need to be set
	public lwEnumFlag( params t_Enum[] flags ) : this()
	{
		for( int nFlagIndex = 0; nFlagIndex<flags.Length; ++nFlagIndex )
		{
			this[flags[nFlagIndex]] = true;
		}
	}

	//! Operator & bit ot bit
	//!
	//!	@param	operand1	first operand
	//!	@param	operand2	second operand
	//!
	//!	@return the result of the operation
	public static lwEnumFlag<t_Enum> operator&( lwEnumFlag<t_Enum> operand1, lwEnumFlag<t_Enum> operand2 )
	{
		lwEnumFlag<t_Enum> result = new lwEnumFlag<t_Enum>();
		for( int nArrayIndex = 0; nArrayIndex<result.m_nValues.Length; ++nArrayIndex )
		{
			result.m_nValues[nArrayIndex] = ( byte )( operand1.m_nValues[nArrayIndex] & operand2.m_nValues[nArrayIndex] );
		}
		return result;
	}

	//! Operator | bit ot bit
	//!
	//!	@param	operand1	first operand
	//!	@param	operand2	second operand
	//!
	//!	@return the result of the operation
	public static lwEnumFlag<t_Enum> operator|( lwEnumFlag<t_Enum> operand1, lwEnumFlag<t_Enum> operand2 )
	{
		lwEnumFlag<t_Enum> result = new lwEnumFlag<t_Enum>();
		for( int nArrayIndex = 0; nArrayIndex<result.m_nValues.Length; ++nArrayIndex )
		{
			result.m_nValues[nArrayIndex] = ( byte )( operand1.m_nValues[nArrayIndex] | operand2.m_nValues[nArrayIndex] );
		}
		return result;
	}

	//! Operator ~
	//!
	//!	@param	operand	operand
	//!
	//!	@return the result of the operation
	public static lwEnumFlag<t_Enum> operator~( lwEnumFlag<t_Enum> operand )
	{
		lwEnumFlag<t_Enum> result = new lwEnumFlag<t_Enum>();
		for( int nArrayIndex = 0; nArrayIndex<result.m_nValues.Length-1; ++nArrayIndex )
		{
			result.m_nValues[nArrayIndex] = ( byte )( ~operand.m_nValues[nArrayIndex] );
		}

		// the last element may not be fully filled
		int nBitCount = COUNT % BIT_PER_ELEMENT;
		if( nBitCount>0 )
		{
			int xMask = ( 1 << nBitCount )-1;
			result.m_nValues[result.m_nValues.Length-1] = ( byte )( ( ~operand.m_nValues[operand.m_nValues.Length-1] ) & xMask );
		}

		return result;
	}

	//! Operator ==
	//!
	//!	@param	operand1	first operand
	//!	@param	operand2	second operand
	//!
	//!	@return the result of the operation
	public static bool operator==( lwEnumFlag<t_Enum> operand1, lwEnumFlag<t_Enum> operand2 )
	{
		if( object.Equals( operand1, null ) && object.Equals( operand2, null ) )
		{
			return true;
		}
		else if( object.Equals( operand1, null ) || object.Equals( operand2, null ) )
		{
			return false;
		}
		else
		{
			int nArrayIndex = 0;
			while( nArrayIndex<ARRAY_LENGTH && operand1.m_nValues[nArrayIndex]==operand2.m_nValues[nArrayIndex] )
			{
				++nArrayIndex;
			}
			return nArrayIndex==ARRAY_LENGTH;
		}
	}

	//! Operator !=
	//!
	//!	@param	operand1	first operand
	//!	@param	operand2	second operand
	//!
	//!	@return the result of the operation
	public static bool operator!=( lwEnumFlag<t_Enum> operand1, lwEnumFlag<t_Enum> operand2 )
	{
		if( object.Equals( operand1, null ) && object.Equals( operand2, null ) )
		{
			return false;
		}
		else if( object.Equals( operand1, null ) || object.Equals( operand2, null ) )
		{
			return true;
		}
		else
		{
			int nArrayIndex = 0;
			while( nArrayIndex<ARRAY_LENGTH && operand1.m_nValues[nArrayIndex]==operand2.m_nValues[nArrayIndex] )
			{
				++nArrayIndex;
			}
			return nArrayIndex<ARRAY_LENGTH;
		}
	}

	//! Set all flags to false
	public void SetAll( bool bValue )
	{
		for( int nArrayIndex=0; nArrayIndex<m_nValues.Length-1; nArrayIndex++ )
		{
			m_nValues[nArrayIndex] = bValue ? byte.MaxValue : byte.MinValue;
		}

		// the last value may not be fully filled
		if( bValue )
		{
			int nBitCount = COUNT % BIT_PER_ELEMENT;
			if( nBitCount==0 )
			{
				m_nValues[m_nValues.Length-1] = byte.MaxValue;
			}
			else
			{
				int xMask = ( 1 << nBitCount ) - 1;
				m_nValues[m_nValues.Length-1] = (byte)( byte.MaxValue & xMask );
			}
		}
		else
		{
			m_nValues[m_nValues.Length-1] = byte.MinValue;
		}
	}

	//! Equality test
	//!
	//!	@param	operand	operand
	//!
	//!	@return true if the object are equal, false otherwise
	public override bool Equals( object operand )
	{
		if( operand is lwEnumFlag<t_Enum> )
		{
			return this == ( operand as lwEnumFlag<t_Enum> );
		}
		else
		{
			return false;
		}
	}

	//! Hash code getter
	//!
	//!	@return the has code of the object
	public override int GetHashCode()
	{
		int nHashCode = 17;
		for( int nElementIndex = 0; nElementIndex<m_nValues.Length; ++nElementIndex)
		{
			nHashCode += 13 * nElementIndex + 11 * m_nValues[nElementIndex];
		}

		return nHashCode;
	}

	//! Convert the collection into an array
	//!
	//! @return an array of boolean values for each flag
	public bool[] ToArray()
	{
		bool[] destinationArray = new bool[COUNT];
		for(int bitIndex = 0; bitIndex < COUNT; ++bitIndex)
		{
			destinationArray[bitIndex] = this[bitIndex];
		}
		return destinationArray;
	}

	//! Convert the collection into an array
	//!
	//! @return an array of t_Enum values for each flag set
	public t_Enum[] ToFlags()
	{
		List<t_Enum> flags = new List<t_Enum>();
		for( int nBitIndex = 0; nBitIndex<COUNT; ++nBitIndex )
		{
			if( this[nBitIndex] )
			{
				flags.Add( ENUM_VALUES[nBitIndex] );
			}
		}
		return flags.ToArray();
	}

#region IEnumerable<t_Content>
	IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return new Enumerator( this );
	}

	IEnumerator<KeyValuePair<t_Enum, bool>> System.Collections.Generic.IEnumerable<KeyValuePair<t_Enum, bool>>.GetEnumerator()
	{
		return new Enumerator( this );
	}
#endregion

#region ISerializationCallbackReceiver
	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
#if UNITY_EDITOR
		string[] sEnumNames = System.Enum.GetNames( typeof( t_Enum ) );

		// copy old values
		byte[] oldValues = new byte[m_nValues.Length];
		System.Array.Copy( m_nValues, oldValues, m_nValues.Length );

		// resize array
		System.Array.Resize( ref m_nValues, ARRAY_LENGTH );
		System.Array.Clear( m_nValues, 0, m_nValues.Length );

		for( int nNewNameIndex = 0; nNewNameIndex<sEnumNames.Length; ++nNewNameIndex )
		{
			int nOldNameIndex = System.Array.FindIndex(m_enumNames, ( string s ) => { return s == sEnumNames[nNewNameIndex]; } );
			if( nOldNameIndex>=0 )
			{
				SetFlagValue( ref m_nValues, nNewNameIndex, GetFlagValue( oldValues, nOldNameIndex ) );
			}
			else
			{
				SetFlagValue( ref m_nValues, nNewNameIndex, default( bool ) );
			}
		}

		m_enumNames = sEnumNames;
#endif // UNITY_EDITOR
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}
#endregion

#region Private
	#region Declarations
	private static readonly int BIT_PER_ELEMENT = sizeof( byte ) * 8;
	#endregion

	#region Methods
	private static bool GetFlagValue( byte[] byteArray, int nBitIndex )
	{
		lwTools.Assert( nBitIndex>=0 && nBitIndex<COUNT );

		int nArrayIndex = nBitIndex/BIT_PER_ELEMENT;
		int nIndexInElement = nBitIndex%BIT_PER_ELEMENT;
		return ( byteArray[nArrayIndex] & ( 1<<nIndexInElement ) )!=0;
	}

	private static void SetFlagValue( ref byte[] byteArray, int nBitIndex, bool bValue)
	{
		lwTools.Assert( nBitIndex>=0 && nBitIndex<COUNT );
		
		int nArrayIndex = nBitIndex/BIT_PER_ELEMENT;
		int nIndexInElement = nBitIndex%BIT_PER_ELEMENT;
		if( bValue )
		{
			byteArray[nArrayIndex] = ( byte )( byteArray[nArrayIndex] | 1<<nIndexInElement );
		}
		else
		{
			byteArray[nArrayIndex] = ( byte )( byteArray[nArrayIndex] & ~( 1<<nIndexInElement ) );
		}
	}
	#endregion
#endregion
}
#endif //!UNITY_FLASH || UNITY_EDITOR
