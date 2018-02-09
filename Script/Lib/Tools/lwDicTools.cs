/// <remarks>
/// Static helper functions to retrieve typed values from Dictionary<string,object> variables
/// Useful when working with data serialized with JSON
/// </remarks>

using System.Collections.Generic;

public sealed class lwDicTools
{
	static public object GetValue( Dictionary<string,object> dic, string sKey )
	{
		object oValue = null;
		if( dic!=null ) dic.TryGetValue( sKey, out oValue );
		return oValue;
	}
	
	static public int GetValueInt32( Dictionary<string,object> dic, string sKey, int nDefault=0 )
	{
		return lwConvertTools.ToInt32( GetValue( dic, sKey ), nDefault );
	}
	
	static public uint GetValueUInt32( Dictionary<string,object> dic, string sKey, uint nDefault=0 )
	{
		return lwConvertTools.ToUInt32( GetValue( dic, sKey ), nDefault );
	}

	static public float GetValueFloat( Dictionary<string,object> dic, string sKey, float fDefault=0.0f )
	{
		return lwConvertTools.ToFloat( GetValue( dic, sKey ), fDefault );
	}

	static public double GetValueDouble( Dictionary<string,object> dic, string sKey, double fDefault=0.0 )
	{
		return lwConvertTools.ToDouble( GetValue( dic, sKey ), fDefault );
	}

	static public bool GetValueBool( Dictionary<string,object> dic, string sKey, bool bDefault=false )
	{
		return lwConvertTools.ToBool( GetValue( dic, sKey ), bDefault );
	}

	static public char GetValueChar( Dictionary<string,object> dic, string sKey, char cDefault = default( char ) )
	{
		return lwConvertTools.ToChar( GetValue( dic, sKey ), cDefault );
	}

	static public string GetValueString( Dictionary<string,object> dic, string sKey, string sDefault=null )
	{
		return lwConvertTools.ToString( GetValue( dic, sKey ), sDefault );
	}
	
	static public List<object> GetValueList( Dictionary<string,object> dic, string sKey )
	{
		return GetValue( dic, sKey ) as List<object>;
	}
	
	static public Dictionary<string,object> GetValueDictionary( Dictionary<string,object> dic, string sKey )
	{
		return GetValue( dic, sKey ) as Dictionary<string,object>;
	}

	static public int[] GetValueListAsIntArray( Dictionary<string,object> dic, string sKey )
	{
		return GetValueListAsIntArray( GetValueList( dic, sKey ) ) ;
	}
	
	static public int[] GetValueListAsIntArray( List<object> list )
	{
		if( list==null ) return null;
		int nCount = list.Count;
		int[] nArray = new int[nCount];
		for( int i=0; i<nCount; i++ )
		{
			nArray[i] = lwConvertTools.ToInt32( list[i] );
		}
		return nArray;
	}

	static public float[] GetValueListAsFloatArray( Dictionary<string,object> dic, string sKey )
	{
		return GetValueListAsFloatArray( GetValueList( dic, sKey ) ) ;
	}

	static public float[] GetValueListAsFloatArray( List<object> list )
	{
		if( list==null ) return null;
		int nCount = list.Count;
		float[] fArray = new float[nCount];
		for( int i=0; i<nCount; i++ )
		{
			fArray[i] = lwConvertTools.ToFloat( list[i] );
		}
		return fArray;
	}
	
	static public string[] GetValueListAsStringArray( Dictionary<string,object> dic, string sKey )
	{
		return GetValueListAsStringArray( GetValueList( dic, sKey ) ) ;
	}

	static public string[] GetValueListAsStringArray( List<object> list )
	{
		if( list==null ) return null;
		int nCount = list.Count;
		string[] sArray = new string[nCount];
		for( int i=0; i<nCount; i++ )
		{
			sArray[i] = lwConvertTools.ToString( list[i] );
		}
		return sArray;
	}

	static public bool[] GetValueListAsBoolArray( Dictionary<string,object> dic, string sKey )
	{
		return GetValueListAsBoolArray( GetValueList( dic, sKey ) ) ;
	}

	static public bool[] GetValueListAsBoolArray( List<object> list )
	{
		if( list==null ) return null;
		int nCount = list.Count;
		bool[] bArray = new bool[nCount];
		for( int i=0; i<nCount; i++ )
		{
			bArray[i] = lwConvertTools.ToBool( list[i] );
		}
		return bArray;
	}

	static public T[] GetValueListAsEnumArray<T>( Dictionary<string, object> dic, string sKey ) where T : struct, System.IConvertible
	{
		return GetValueListAsEnumArray<T>( GetValueList( dic, sKey ) ) ;
	}

	static public T[] GetValueListAsEnumArray<T>( List<object> list ) where T : struct, System.IConvertible
	{
		if( list==null ) return null;
		int nCount = list.Count;
		T[] array = new T[nCount];
		for( int i=0; i<nCount; i++ )
		{
			T enumValue;
			if(lwParseTools.TryParseEnum<T>((string)list[i], out enumValue))
			{
				array[i] = enumValue;
			}
		}
		return array;
	}
}
