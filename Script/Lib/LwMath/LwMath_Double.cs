// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2017/03/14

//! @class lwMath
//!
//! @brief	holds the methods for mathematics instruction
public static partial class lwMath
{
	/// <summary>
	/// Get the minimal value inside an array
	/// </summary>
	/// <param name="values">array of values</param>
	public static double Min( params double[] values )
	{
		lwTools.Assert( values!=null );
		lwTools.Assert( values.Length>0 );

		double result = values[0];
		for( int nIndex = 1; nIndex<values.Length; ++nIndex )
		{
			if( values[nIndex]<result )
			{
				result = values[nIndex];
			}
		}

		return result;
	}

	/// <summary>
	/// Get the maximal value inside an array
	/// </summary>
	/// <param name="values">array of values</param>
	public static double Max( params double[] values )
	{
		lwTools.Assert( values!=null );
		lwTools.Assert( values.Length>0 );

		double result = values[0];
		for( int nIndex = 1; nIndex<values.Length; ++nIndex )
		{
			if( values[nIndex]>result )
			{
				result = values[nIndex];
			}
		}

		return result;
	}

	/// <summary>
	/// Get the absolute value of the given one
	/// </summary>
	/// <param name="value">value</param>
	public static double Abs( double value )
	{
		return value>0 ? value : -value;
	}

	/// <summary>
	/// Cosinus of the specified angle (in degrees)
	/// </summary>
	/// <param name="angleInDegree">Angle in degree</param>
	public static double Cos( double angleInDegree )
	{
		return System.Math.Cos( angleInDegree*System.Math.PI/180.0 );
	}

	/// <summary>
	/// Sinus of the specified angle (in degrees)
	/// </summary>
	/// <param name="angleInDegree">Angle in degree</param>
	public static double Sin( double angleInDegree )
	{
		return System.Math.Sin( angleInDegree*System.Math.PI/180.0 );
	}

	/// <summary>
	/// Tangent the specified angle (in degrees)
	/// </summary>
	/// <param name="angleInDegree">Angle in degree</param>
	public static double Tan( double angleInDegree )
	{
		return System.Math.Tan( angleInDegree*System.Math.PI/180.0 );
	}

	/// <summary>
	/// Get the power of a value
	/// </summary>
	/// <param name="value">value</param>
	/// <param name="power">power</param>
	public static double Pow( double value, double power )
	{
		return System.Math.Pow( value, power );
	}

	/// <summary>
	/// Get the squarred root of a value
	/// </summary>
	/// <param name="value">value</param>
	public static double Sqrt( double value )
	{
		return System.Math.Sqrt( value );
	}

	/// <summary>
	/// Integer division is like the division but returns the integer amount of times the divisor is in the dividend
	/// </summary>
	/// <param name="dividend">dividend</param>
	/// <param name="divisor">divisor</param>
	public static double IntegerDivision( double dividend, double divisor )
	{
		return Floor( dividend/divisor );
	}

	/// <summary>
	/// Get the integer value just below or equal to the value given
	/// </summary>
	/// <param name="value">Value.</param>
	public static double Floor( double value )
	{
		double integerValue = ( double )( ( int )value );
		if( integerValue<=value )
		{
			return integerValue;
		}
		else
		{
			--integerValue;
			lwTools.Assert( integerValue<=value );
			return integerValue;
		}
	}

	/// <summary>
	/// Get the integer value just above or equal to the value given
	/// </summary>
	/// <param name="value">Value.</param>
	public static double Ceil( double value )
	{
		double integerValue = ( double )( ( int )value );
		if( integerValue>=value )
		{
			return integerValue;
		}
		else
		{
			++integerValue;
			lwTools.Assert( integerValue>=value );
			return integerValue;
		}
	}
}