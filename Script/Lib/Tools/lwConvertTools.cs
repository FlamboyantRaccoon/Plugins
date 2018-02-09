/// <remarks>
/// Static helper functions to convert advanced types (such as objcts) to primary types
/// </remarks>

using System;

public sealed class lwConvertTools
{
	static public int ToInt32( object oValue, int nDefault=0 )
	{
		if( oValue!=null )
#if UNITY_FLASH
			return Convert.ToInt32( oValue.ToString() );
#else
			return Convert.ToInt32( oValue );
#endif
		else
			return nDefault;
	}
	
	static public uint ToUInt32( object oValue, uint nDefault=0 )
	{
		if( oValue!=null )
#if UNITY_FLASH
			return Convert.ToUInt32( oValue.ToString() );
#else
			return Convert.ToUInt32( oValue );
#endif
		else
			return nDefault;
	}

	static public float ToFloat( object oValue, float fDefault=0.0f )
	{
		if( oValue!=null )
#if UNITY_FLASH
			return Convert.ToSingle( oValue.ToString() );
#else
			return Convert.ToSingle( oValue );
#endif
		else
			return fDefault;
	}

	static public double ToDouble( object oValue, double fDefault=0.0 )
	{
		if( oValue!=null )
#if UNITY_FLASH
			return Convert.ToDouble( oValue.ToString() );
#else
			return Convert.ToDouble( oValue );
#endif
		else
			return fDefault;
	}

	static public bool ToBool( object oValue, bool bDefault=false )
	{
		if( oValue!=null )
		{
			try
			{
#if UNITY_FLASH
				return Convert.ToBoolean( oValue.ToString() );
#else
				return Convert.ToBoolean( oValue );
#endif
			}
			catch{}
			try
			{
#if UNITY_FLASH
				return Convert.ToInt32( oValue.ToString() ) != 0;
#else
				return Convert.ToInt32( oValue ) != 0;
#endif
			}
			catch{}
		}
		return bDefault;
	}

	static public char ToChar( object oValue, char cDefault = default( char ) )
	{
		if( oValue!=null )
		{
			try
			{
#if UNITY_FLASH
				return Convert.ToChar( oValue.ToString() );
#else
				return Convert.ToChar( oValue );
#endif
			}
			catch{}
		}
		return cDefault;
	}

	static public string ToString( object oValue, string sDefault=null )
	{
		if( oValue!=null )
			return oValue.ToString();
		else
			return sDefault;
	}
}
