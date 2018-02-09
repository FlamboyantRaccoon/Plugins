#if UNITY_EDITOR
using UnityEngine;
#endif
using System.Globalization;

public sealed class lwParseTools
{
	// Parse a float from a string
	// Returns a default value if it failed
	public static float ParseFloatSafe( string sValue, float fDefault=0f )
	{
		float fValueOut = fDefault;
#if UNITY_FLASH
		if( !float.TryParse( sValue, out fValueOut ) )
#else
		if( !float.TryParse( sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out fValueOut ) )
#endif
		{
			fValueOut = fDefault;
#if UNITY_EDITOR
			Debug.LogError( "Can't parse " + sValue + " to float" );
#endif
		}
		return fValueOut;
	}

	// Parse a int from a string
	// Returns a default value if it failed
	public static int ParseIntSafe( string sValue, int nDefault=0 )
	{
		int nValueOut = nDefault;
#if UNITY_FLASH
		if( !int.TryParse( sValue, out nValueOut ) )
#else
		if( !int.TryParse( sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out nValueOut ) )
#endif
		{
			nValueOut = nDefault;
#if UNITY_EDITOR
			Debug.LogError( "Can't parse " + sValue + " to int" );
#endif
		}
		return nValueOut;
	}

	// Parse a uint from a string
	// Returns a default value if it failed
	public static uint ParseUIntSafe( string sValue, uint nDefault=0 )
	{
		uint nValueOut = nDefault;
#if UNITY_FLASH
		if( !uint.TryParse( sValue, out nValueOut ) )
#else
		if( !uint.TryParse( sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out nValueOut ) )
#endif
		{
			nValueOut = nDefault;
#if UNITY_EDITOR
			Debug.LogError( "Can't parse " + sValue + " to uint" );
#endif
		}
		return nValueOut;
	}

	// Parse a long from a string
	// Returns a default value if it failed
	public static long ParseLongSafe( string sValue, long nDefault=0 )
	{
		long nValueOut = nDefault;
#if UNITY_FLASH
		if( !long.TryParse( sValue, out nValueOut ) )
#else
		if( !long.TryParse( sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out nValueOut ) )
#endif
		{
			nValueOut = nDefault;
#if UNITY_EDITOR
			Debug.LogError( "Can't parse " + sValue + " to long" );
#endif
		}
		return nValueOut;
	}

	// Parse a double from a string
	// Returns a default value if it failed
	public static double ParseDoubleSafe( string sValue, double fDefault=0 )
	{
		double fValueOut = fDefault;
#if UNITY_FLASH
		if( !double.TryParse( sValue, out fValueOut ) )
#else
		if( !double.TryParse( sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out fValueOut ) )
#endif
		{
			fValueOut = fDefault;
#if UNITY_EDITOR
			Debug.LogError( "Can't parse " + sValue + " to double" );
#endif
		}
		return fValueOut;
	}

	// Parse a bool from a string
	// Returns a default value if it failed
	public static bool ParseBoolSafe( string sValue, bool bDefault=false )
	{
		bool bValueOut = bDefault;
		if( !bool.TryParse( sValue, out bValueOut ) )
		{
			bValueOut = bDefault;
#if UNITY_EDITOR
			Debug.LogError( "Can't parse " + sValue + " to bool" );
#endif
		}
		return bValueOut;
	}

	// Parse an enum from a string
	// Returns a default value if it failed
	public static T ParseEnumSafe<T>( string sEnum, T eDefault )
	{
		try
		{
			object oEnum = System.Enum.Parse( typeof( T ), sEnum );
			return (T)oEnum;
		}
		catch
		{
			// ignored
		}
#if UNITY_EDITOR
		Debug.LogError( "Can't parse " + sEnum + " to enum " + typeof( T ) );
#endif
		return eDefault;
	}

	//! @brief Try to parse an enumeration from a string value
	//!
	//! @param sEnum	value of the enumeration to parse as a string
	//! @param eResult	result value of the enumeration if the parsing succeeds, first value of the enumeration if the parsing fails
	//!
	//! @return true if the parsing succeeds, false otherwise
	public static bool TryParseEnum<T>( string sEnum, out T eResult ) where T : struct, System.IConvertible
	{
		try
		{
			object oEnum = System.Enum.Parse( typeof( T ), sEnum );
			eResult = ( T )oEnum;
			return true;
		}
		catch
		{
			// ignored
		}

		eResult = default( T );
		return false;
	}
}
