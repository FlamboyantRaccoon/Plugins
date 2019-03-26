// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/05/18

//! @class lwMath
//!
//! @brief	holds the methods for mathematics instruction
public static partial class lwMath
{
	//! @brief Checks if an integer is a power of 2
	//!
	//! @param	n	integer value
	//!
	//! @return true if the value given is a power of 2, false otherwise
	public static bool IsPowerOfTwo( ushort n )
	{
		return ( n!=0 ) && ( ( n&( n-1 ) )==0 );
	}

	//! @brief Checks if an integer is a power of 2
	//!
	//! @param	n	integer value
	//!
	//! @return true if the value given is a power of 2, false otherwise
	public static bool IsPowerOfTwo( uint n )
	{
		return ( n!=0 ) && ( ( n&( n-1 ) )==0 );
	}

	//! @brief Checks if an integer is a power of 2
	//!
	//! @param	n	integer value
	//!
	//! @return true if the value given is a power of 2, false otherwise
	public static bool IsPowerOfTwo( ulong n )
	{
		return ( n!=0 ) && ( ( n&( n-1 ) )==0 );
	}
}