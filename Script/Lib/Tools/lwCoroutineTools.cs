// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/05/17

using System.Collections;

//! @class lwCoroutineTools
//!
//! @brief	Tools for coroutine
public static class lwCoroutineTools
{
	//! Continue the coroutine where it paused
	//!
	//!	@param	oEnumerator	the routine enumerator
	//!
	//!	@return true if the routine needs to continue, false it the routine reaches its end
	public static bool ContinueRoutine( IEnumerator oEnumerator )
	{
		if( oEnumerator==null )
		{
			return false;
		}

		if( oEnumerator.Current!=null && oEnumerator.Current is IEnumerator )
		{
			IEnumerator nextEnumerator = oEnumerator.Current as IEnumerator;
			if( ContinueRoutine( nextEnumerator ) )
			{
				return true;
			}
		}
		return oEnumerator.MoveNext();
	}
}
