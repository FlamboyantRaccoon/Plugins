// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/07/06

#if !UNITY_FLASH
using System;

//!	@class Curry
//!
//!	@brief Static class that holds methods to create functions with binded arguments
public static class lwCurry
{	
	// Actions
	
	//!	Bind an argument to an action with 1 argument
	//!
	//!	@param	action	action that takes 1 argument
	//!	@param	arg0	argument to bind
	//!
	//!	@return	an action that takes no argument
	public static Action Bind<t_Arg0>( Action<t_Arg0> action, t_Arg0 arg0 )
	{
		return () => action( arg0 );
	}

	//!	Bind an argument to an action with 2 arguments
	//!
	//!	@param	action	action that takes 2 arguments
	//!	@param	arg0	first argument to bind
	//!	@param	arg1	second argument to bind
	//!
	//!	@return	an action that takes no argument
	public static Action Bind<t_Arg0, t_Arg1>( Action<t_Arg0, t_Arg1> action, t_Arg0 arg0, t_Arg1 arg1 )
	{
		return () => action( arg0, arg1 );
	}

	//!	Bind an argument to an action with 3 arguments
	//!
	//!	@param	action	action that takes 3 arguments
	//!	@param	arg0	first argument to bind
	//!	@param	arg1	second argument to bind
	//!	@param	arg2	third argument to bind
	//!
	//!	@return	an action that takes no argument
	public static Action Bind<t_Arg0, t_Arg1, t_Arg2>( Action<t_Arg0, t_Arg1, t_Arg2> action, t_Arg0 arg0, t_Arg1 arg1, t_Arg2 arg2 )
	{
		return () => action( arg0, arg1, arg2 );
	}

	//!	Bind an argument to an action with 4 arguments
	//!
	//!	@param	action	action that takes 4 arguments
	//!	@param	arg0	first argument to bind
	//!	@param	arg1	second argument to bind
	//!	@param	arg2	third argument to bind
	//!	@param	arg3	fourth argument to bind
	//!
	//!	@return	an action that takes no argument
	public static Action Bind<t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Action<t_Arg0, t_Arg1, t_Arg2, t_Arg3> action, t_Arg0 arg0, t_Arg1 arg1, t_Arg2 arg2, t_Arg3 arg3 )
	{
		return () => action( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the first argument to an action with 2 arguments
	//!
	//!	@param	action	action that takes 2 arguments
	//!	@param	arg		argument to bind
	//!
	//!	@return	an action that takes 1 argument
	public static Action<t_Arg1> BindFirst<t_Arg0, t_Arg1>( Action<t_Arg0, t_Arg1> action, t_Arg0 arg0 )
	{
		return ( t_Arg1 arg1 ) => action( arg0, arg1 );
	}

	//!	Bind the last argument to an action with 2 arguments
	//!
	//!	@param	action	action that takes 2 arguments
	//!	@param	arg		argument to bind
	//!
	//!	@return	an action that takes 1 argument
	public static Action<t_Arg0> BindLast<t_Arg0, t_Arg1>(Action<t_Arg0, t_Arg1> action, t_Arg1 arg1)
	{
		return ( t_Arg0 arg0 ) => action( arg0, arg1 );
	}

	//!	Bind the first argument to an action with 3 arguments
	//!
	//!	@param	action	action that takes 3 arguments
	//!	@param	arg0	argument to bind
	//!
	//!	@return	an action that takes 2 arguments
	public static Action<t_Arg1, t_Arg2> BindFirst<t_Arg0, t_Arg1, t_Arg2>( Action<t_Arg0, t_Arg1, t_Arg2> action, t_Arg0 arg0 )
	{
		return ( t_Arg1 arg1, t_Arg2 arg2 ) => action( arg0, arg1, arg2 );
	}

	//!	Bind the first arguments to an action with 3 arguments
	//!
	//!	@param	action	action that takes 3 arguments
	//!	@param	arg0	first argument to bind
	//!	@param	arg1	second argument to bind
	//!
	//!	@return	an action that takes 2 arguments
	public static Action<t_Arg2> BindFirst<t_Arg0, t_Arg1, t_Arg2>( Action<t_Arg0, t_Arg1, t_Arg2> action, t_Arg0 arg0, t_Arg1 arg1 )
	{
		return ( t_Arg2 arg2 ) => action( arg0, arg1, arg2 );
	}

	//!	Bind the second argument to an action with 3 arguments
	//!
	//!	@param	action	action that takes 3 arguments
	//!	@param	arg1	argument to bind
	//!
	//!	@return	an action that takes 1 argument
	public static Action<t_Arg0, t_Arg2> BindSecond<t_Arg0, t_Arg1, t_Arg2>( Action<t_Arg0, t_Arg1, t_Arg2> action, t_Arg1 arg1 )
	{
		return ( t_Arg0 arg0, t_Arg2 arg2 ) => action( arg0, arg1, arg2 );
	}

	//!	Bind the last arguments to an action with 3 arguments
	//!
	//!	@param	action	action that takes 3 arguments
	//!	@param	arg1	first argument to bind
	//!	@param	arg2	second argument to bind
	//!
	//!	@return	an action that takes 1 argument
	public static Action<t_Arg0> BindLast<t_Arg0, t_Arg1, t_Arg2>( Action<t_Arg0, t_Arg1, t_Arg2> action, t_Arg1 arg1, t_Arg2 arg2 )
	{
		return ( t_Arg0 arg0 ) => action( arg0, arg1, arg2 );
	}

	//!	Bind the last argument to an action with 3 arguments
	//!
	//!	@param	action	action that takes 3 arguments
	//!	@param	arg2	argument to bind
	//!
	//!	@return	an action that takes 2 arguments
	public static Action<t_Arg0, t_Arg1> BindLast<t_Arg0, t_Arg1, t_Arg2>( Action<t_Arg0, t_Arg1, t_Arg2> action, t_Arg2 arg2 )
	{
		return ( t_Arg0 arg0, t_Arg1 arg1 ) => action( arg0, arg1, arg2 );
	}

	//!	Bind the first argument to an action with 4 arguments
	//!
	//!	@param	action	action that takes 4 arguments
	//!	@param	arg0	argument to bind
	//!
	//!	@return	an action that takes 3 arguments
	public static Action<t_Arg1, t_Arg2, t_Arg3> BindFirst<t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Action<t_Arg0, t_Arg1, t_Arg2, t_Arg3> action, t_Arg0 arg0 )
	{
		return ( t_Arg1 arg1, t_Arg2 arg2, t_Arg3 arg3 ) => action( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the first arguments to an action with 4 arguments
	//!
	//!	@param	action	action that takes 4 arguments
	//!	@param	arg0	first argument to bind
	//!	@param	arg1	second argument to bind
	//!
	//!	@return	an action that takes 2 arguments
	public static Action<t_Arg2, t_Arg3> BindFirst<t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Action<t_Arg0, t_Arg1, t_Arg2, t_Arg3> action, t_Arg0 arg0, t_Arg1 arg1 )
	{
		return ( t_Arg2 arg2, t_Arg3 arg3 ) => action( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the first arguments to an action with 4 arguments
	//!
	//!	@param	action	action that takes 4 arguments
	//!	@param	arg0	first argument to bind
	//!	@param	arg1	second argument to bind
	//!	@param	arg2	third argument to bind
	//!
	//!	@return	an action that takes 1 argument
	public static Action<t_Arg3> BindFirst<t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Action<t_Arg0, t_Arg1, t_Arg2, t_Arg3> action, t_Arg0 arg0, t_Arg1 arg1, t_Arg2 arg2 )
	{
		return ( t_Arg3 arg3 ) => action( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the second argument to an action with 4 arguments
	//!
	//!	@param	action	action that takes 4 arguments
	//!	@param	arg1	argument to bind
	//!
	//!	@return	an action that takes 3 arguments
	public static Action<t_Arg0, t_Arg2, t_Arg3> BindSecond<t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Action<t_Arg0, t_Arg1, t_Arg2, t_Arg3> action, t_Arg1 arg1 )
	{
		return ( t_Arg0 arg0, t_Arg2 arg2, t_Arg3 arg3 ) => action( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the third argument to an action with 4 arguments
	//!
	//!	@param	action	action that takes 4 arguments
	//!	@param	arg2	argument to bind
	//!
	//!	@return	an action that takes 3 arguments
	public static Action<t_Arg0, t_Arg1, t_Arg3> BindThird<t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Action<t_Arg0, t_Arg1, t_Arg2, t_Arg3> action, t_Arg2 arg2 )
	{
		return ( t_Arg0 arg0, t_Arg1 arg1, t_Arg3 arg3 ) => action( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the last argument to an action with 4 arguments
	//!
	//!	@param	action	action that takes 4 arguments
	//!	@param	arg3	argument to bind
	//!
	//!	@return	an action that takes 3 arguments
	public static Action<t_Arg0, t_Arg1, t_Arg2> BindLast<t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Action<t_Arg0, t_Arg1, t_Arg2, t_Arg3> action, t_Arg3 arg3 )
	{
		return ( t_Arg0 arg0, t_Arg1 arg1, t_Arg2 arg2 ) => action( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the last arguments to an action with 4 arguments
	//!
	//!	@param	action	action that takes 4 arguments
	//!	@param	arg2	first argument to bind
	//!	@param	arg3	second argument to bind
	//!
	//!	@return	an action that takes 2 arguments
	public static Action<t_Arg0, t_Arg1> BindLast<t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Action<t_Arg0, t_Arg1, t_Arg2, t_Arg3> action, t_Arg2 arg2, t_Arg3 arg3 )
	{
		return ( t_Arg0 arg0, t_Arg1 arg1 ) => action( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the last arguments to an action with 4 arguments
	//!
	//!	@param	action	action that takes 4 arguments
	//!	@param	arg1	first argument to bind
	//!	@param	arg2	second argument to bind
	//!	@param	arg3	third argument to bind
	//!
	//!	@return	an action that takes 1 argument
	public static Action<t_Arg0> BindLast<t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Action<t_Arg0, t_Arg1, t_Arg2, t_Arg3> action, t_Arg1 arg1, t_Arg2 arg2, t_Arg3 arg3 )
	{
		return ( t_Arg0 arg0 ) => action( arg0, arg1, arg2, arg3 );
	}

	// Functions

	//!	Bind an argument to a function with 1 argument
	//!
	//!	@param	function	function that takes 1 argument
	//!	@param	arg0		argument to bind
	//!
	//!	@return	a function that takes no argument
	public static Func<t_Ret> Bind<t_Ret, t_Arg0>( Func<t_Arg0, t_Ret> function, t_Arg0 arg0 )
	{
		return () => function( arg0 );
	}

	//!	Bind an argument to a function with 2 arguments
	//!
	//!	@param	function	function that takes 2 arguments
	//!	@param	arg0		first argument to bind
	//!	@param	arg1		second argument to bind
	//!
	//!	@return	a function that takes no argument
	public static Func<t_Ret> Bind<t_Ret, t_Arg0, t_Arg1>( Func<t_Arg0, t_Arg1, t_Ret> function, t_Arg0 arg0, t_Arg1 arg1 )
	{
		return () => function( arg0, arg1 );
	}

	//!	Bind an argument to a function with 3 arguments
	//!
	//!	@param	function	function that takes 3 arguments
	//!	@param	arg0		first argument to bind
	//!	@param	arg1		second argument to bind
	//!	@param	arg2		third argument to bind
	//!
	//!	@return	a function that takes no argument
	public static Func<t_Ret> Bind<t_Ret, t_Arg0, t_Arg1, t_Arg2>( Func<t_Arg0, t_Arg1, t_Arg2, t_Ret> function, t_Arg0 arg0, t_Arg1 arg1, t_Arg2 arg2 )
	{
		return () => function( arg0, arg1, arg2 );
	}

	//!	Bind an argument to a function with 4 arguments
	//!
	//!	@param	function	function that takes 4 arguments
	//!	@param	arg0		first argument to bind
	//!	@param	arg1		second argument to bind
	//!	@param	arg2		third argument to bind
	//!	@param	arg3		fourth argument to bind
	//!
	//!	@return	a function that takes no argument
	public static Func<t_Ret> Bind<t_Ret, t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Func<t_Arg0, t_Arg1, t_Arg2, t_Arg3, t_Ret> function, t_Arg0 arg0, t_Arg1 arg1, t_Arg2 arg2, t_Arg3 arg3 )
	{
		return () => function( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the first argument to a function with 2 arguments
	//!
	//!	@param	function	function that takes 2 arguments
	//!	@param	arg0		argument to bind
	//!
	//!	@return	a function that takes 1 argument
	public static Func<t_Arg1, t_Ret> BindFirst<t_Ret, t_Arg0, t_Arg1>( Func<t_Arg0, t_Arg1, t_Ret> function, t_Arg0 arg0 )
	{
		return ( t_Arg1 arg1 ) => function( arg0, arg1 );
	}

	//!	Bind the last argument to a function with 2 arguments
	//!
	//!	@param	function	function that takes 2 arguments
	//!	@param	arg1		argument to bind
	//!
	//!	@return	a function that takes 1 argument
	public static Func<t_Arg0, t_Ret> BindLast<t_Ret, t_Arg0, t_Arg1>( Func<t_Arg0, t_Arg1, t_Ret> function, t_Arg1 arg1 )
	{
		return ( t_Arg0 arg0 ) => function( arg0, arg1 );
	}

	//!	Bind the first argument to a function with 3 arguments
	//!
	//!	@param	function	function that takes 3 arguments
	//!	@param	arg0		argument to bind
	//!
	//!	@return	a function that takes 2 arguments
	public static Func<t_Arg1, t_Arg2, t_Ret> BindFirst<t_Ret, t_Arg0, t_Arg1, t_Arg2>( Func<t_Arg0, t_Arg1, t_Arg2, t_Ret> function, t_Arg0 arg0 )
	{
		return ( t_Arg1 arg1, t_Arg2 arg2 ) => function( arg0, arg1, arg2 );
	}

	//!	Bind the first arguments to a function with 3 arguments
	//!
	//!	@param	function	function that takes 3 arguments
	//!	@param	arg0		first argument to bind
	//!	@param	arg1		second argument to bind
	//!
	//!	@return	a function that takes 1 argument
	public static Func<t_Arg2, t_Ret> BindFirst<t_Ret, t_Arg0, t_Arg1, t_Arg2>( Func<t_Arg0, t_Arg1, t_Arg2, t_Ret> function, t_Arg0 arg0, t_Arg1 arg1 )
	{
		return ( t_Arg2 arg2 ) => function( arg0, arg1, arg2 );
	}

	//!	Bind the second argument to a function with 3 arguments
	//!
	//!	@param	function	function that takes 3 arguments
	//!	@param	arg1		argument to bind
	//!
	//!	@return	a function that takes 2 arguments
	public static Func<t_Arg0, t_Arg2, t_Ret> BindSecond<t_Ret, t_Arg0, t_Arg1, t_Arg2>( Func<t_Arg0, t_Arg1, t_Arg2, t_Ret> function, t_Arg1 arg1 )
	{
		return ( t_Arg0 arg0, t_Arg2 arg2 ) => function( arg0, arg1, arg2 );
	}

	//!	Bind the last argument to a function with 3 arguments
	//!
	//!	@param	function	function that takes 3 arguments
	//!	@param	arg2		argument to bind
	//!
	//!	@return	a function that takes 2 arguments
	public static Func<t_Arg0, t_Arg1, t_Ret> BindLast<t_Ret, t_Arg0, t_Arg1, t_Arg2>( Func<t_Arg0, t_Arg1, t_Arg2, t_Ret> function, t_Arg2 arg2 )
	{
		return ( t_Arg0 arg0, t_Arg1 arg1 ) => function( arg0, arg1, arg2 );
	}

	//!	Bind the last arguments to a function with 3 arguments
	//!
	//!	@param	function	function that takes 3 arguments
	//!	@param	arg1		first argument to bind
	//!	@param	arg2		second argument to bind
	//!
	//!	@return	a function that takes 1 argument
	public static Func<t_Arg0, t_Ret> BindLast<t_Ret, t_Arg0, t_Arg1, t_Arg2>( Func<t_Arg0, t_Arg1, t_Arg2, t_Ret> function, t_Arg1 arg1, t_Arg2 arg2 )
	{
		return ( t_Arg0 arg0 ) => function( arg0, arg1, arg2 );
	}

	//!	Bind the first argument to a function with 4 arguments
	//!
	//!	@param	function	function that takes 4 arguments
	//!	@param	arg0		argument to bind
	//!
	//!	@return	a function that takes 3 arguments
	public static Func<t_Arg1, t_Arg2, t_Arg3, t_Ret> BindFirst<t_Ret, t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Func<t_Arg0, t_Arg1, t_Arg2, t_Arg3, t_Ret> function, t_Arg0 arg0 )
	{
		return ( t_Arg1 arg1, t_Arg2 arg2, t_Arg3 arg3 ) => function( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the first arguments to a function with 4 arguments
	//!
	//!	@param	function	function that takes 4 arguments
	//!	@param	arg0		first argument to bind
	//!	@param	arg1		second argument to bind
	//!
	//!	@return	a function that takes 2 arguments
	public static Func<t_Arg2, t_Arg3, t_Ret> BindFirst<t_Ret, t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Func<t_Arg0, t_Arg1, t_Arg2, t_Arg3, t_Ret> function, t_Arg0 arg0, t_Arg1 arg1 )
	{
		return ( t_Arg2 arg2, t_Arg3 arg3 ) => function( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the first arguments to a function with 4 arguments
	//!
	//!	@param	function	function that takes 4 arguments
	//!	@param	arg0		first argument to bind
	//!	@param	arg1		second argument to bind
	//!	@param	arg2		third argument to bind
	//!
	//!	@return	a function that takes 1 argument
	public static Func<t_Arg3, t_Ret> BindFirst<t_Ret, t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Func<t_Arg0, t_Arg1, t_Arg2, t_Arg3, t_Ret> function, t_Arg0 arg0, t_Arg1 arg1, t_Arg2 arg2 )
	{
		return ( t_Arg3 arg3 ) => function( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the second argument to a function with 4 arguments
	//!
	//!	@param	function	function that takes 4 arguments
	//!	@param	arg1			argument to bind
	//!
	//!	@return	a function that takes 3 arguments
	public static Func<t_Arg0, t_Arg2, t_Arg3, t_Ret> BindSecond<t_Ret, t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Func<t_Arg0, t_Arg1, t_Arg2, t_Arg3, t_Ret> function, t_Arg1 arg1 )
	{
		return ( t_Arg0 arg0, t_Arg2 arg2, t_Arg3 arg3 ) => function( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the third argument to a function with 4 arguments
	//!
	//!	@param	function	function that takes 4 arguments
	//!	@param	arg2			argument to bind
	//!
	//!	@return	a function that takes 3 arguments
	public static Func<t_Arg0, t_Arg1, t_Arg3, t_Ret> BindThird<t_Ret, t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Func<t_Arg0, t_Arg1, t_Arg2, t_Arg3, t_Ret> function, t_Arg2 arg2 )
	{
		return ( t_Arg0 arg0, t_Arg1 arg1, t_Arg3 arg3 ) => function( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the last argument to a function with 4 arguments
	//!
	//!	@param	function	function that takes 4 arguments
	//!	@param	arg3			argument to bind
	//!
	//!	@return	a function that takes 3 arguments
	public static Func<t_Arg0, t_Arg1, t_Arg2, t_Ret> BindLast<t_Ret, t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Func<t_Arg0, t_Arg1, t_Arg2, t_Arg3, t_Ret> function, t_Arg3 arg3 )
	{
		return ( t_Arg0 arg0, t_Arg1 arg1, t_Arg2 arg2 ) => function( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the last arguments to a function with 4 arguments
	//!
	//!	@param	function	function that takes 4 arguments
	//!	@param	arg2		first argument to bind
	//!	@param	arg3		second argument to bind
	//!
	//!	@return	a function that takes 2 arguments
	public static Func<t_Arg0, t_Arg1, t_Ret> BindLast<t_Ret, t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Func<t_Arg0, t_Arg1, t_Arg2, t_Arg3, t_Ret> function, t_Arg2 arg2, t_Arg3 arg3 )
	{
		return ( t_Arg0 arg0, t_Arg1 arg1 ) => function( arg0, arg1, arg2, arg3 );
	}

	//!	Bind the last arguments to a function with 4 arguments
	//!
	//!	@param	function	function that takes 4 arguments
	//!	@param	arg1		first argument to bind
	//!	@param	arg2		second argument to bind
	//!	@param	arg3		third argument to bind
	//!
	//!	@return	a function that takes 1 argument
	public static Func<t_Arg0, t_Ret> BindLast<t_Ret, t_Arg0, t_Arg1, t_Arg2, t_Arg3>( Func<t_Arg0, t_Arg1, t_Arg2, t_Arg3, t_Ret> function, t_Arg1 arg1, t_Arg2 arg2, t_Arg3 arg3 )
	{
		return ( t_Arg0 arg0 ) => function( arg0, arg1, arg2, arg3 );
	}
}
#endif
