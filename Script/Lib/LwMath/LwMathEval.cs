using UnityEngine;
using System;
using System.Collections.Generic;

//! @class	lwMathEval
//!
//! @brief	Expression evaluator class
public class lwMathEval
{
	//! @class	EvalException
	//!
	//! @brief	Custom exception for evaluation errors
	public class EvalException : Exception
	{
		//! @brief	Zero-base position in expression where exception occurred
		public int Column;

		//! @brief	Constructor
		//!
		//! @param	sMessage	Message that describes this exception
		//! @param	nPosition	Position within expression where exception occurred
		public EvalException( string sMessage, int nPosition ) : base( sMessage )
		{
			Column = nPosition;
		}

		//! @brief	Gets the message associated with this exception
		public override string Message
		{
			get
			{
				return String.Format( "{0} (column {1})", base.Message, Column+1 );
			}
		}
	}

	//! @brief	Enumeration for symbol status
	public enum SymbolStatus
	{
		OK,
		UndefinedSymbol
	}

	//! @class	SymbolEventArgs
	//!
	//! @brief	Holds arguments for ProcessSymbol
	public class SymbolEventArgs : EventArgs
	{
		public string sName;
		public double fResult;
		public SymbolStatus eStatus;
	}

	//! @brief	Enumeration for function status
	public enum FunctionStatus
	{
		OK,
		UndefinedFunction,
		WrongParameterCount
	}

	//! @class	FunctionEventArgs
	//!
	//! @brief	Holds arguments for ProcessFunction
	public class FunctionEventArgs : EventArgs
	{
		public string sName;
		public List<double> fParameters;
		public double fResult;
		public FunctionStatus eStatus;
	}

	//! @brief	Event handlers
	public delegate void ProcessSymbolHandler( SymbolEventArgs sea );
	public delegate void ProcessFunctionHandler( FunctionEventArgs fea );
	public void AddSymbolHandler( ProcessSymbolHandler handler ) { m_processSymbol += handler; }
	public void RemoveSymbolHandler( ProcessSymbolHandler handler ) { m_processSymbol -= handler; }
	public void AddFunctionHandler( ProcessFunctionHandler handler ) { m_processFunction += handler; }
	public void RemoveFunctionHandler( ProcessFunctionHandler handler ) { m_processFunction -= handler; }

	// Token state enums
	protected enum State
	{
		None,
		Operand,
		Operator,
		UnaryOperator
	}

	// Error messages
	private const string ERROR_INVALID_OPERAND = "Invalid operand";
	private const string ERROR_OPERAND_EXPECTED = "Operand expected";
	private const string ERROR_OPERATOR_EXPECTED = "Operator expected";
	private const string ERROR_UNMATCHED_CLOSING_PAR = "Closing parenthesis without matching open parenthesis";
	private const string ERROR_MULTIPLE_DECIMAL_POINTS = "Operand contains multiple decimal points";
	private const string ERROR_UNEXPECTED_CHARACTER = "Unexpected character encountered \"{0}\"";
	private const string ERROR_UNDEFINED_SYMBOL = "Undefined symbol \"{0}\"";
	private const string ERROR_UNDEFINED_FUNCTION = "Undefined function \"{0}\"";
	private const string ERROR_CLOSING_PAR_EXPECTED = "Closing parenthesis expected";
	private const string ERROR_WRONG_PARAM_COUNT = "Wrong number of function parameters";

	// To distinguish it from a minus operator,
	// we'll use a character unlikely to appear
	// in expressions to signify a unary negative
	private const string UNARY_MINUS = "\x80";

	private ProcessSymbolHandler m_processSymbol;
	private ProcessFunctionHandler m_processFunction;
	
	//! @brief	Constructor
	public lwMathEval()
	{
		// Add default handlers for common symbols/functions
		AddSymbolHandler( DefaultSymbolHandler );
		AddFunctionHandler( DefaultFunctionHandler );
	}

	//! @brief	Evaluates the given expression and returns the result
	//!
	//! @param	sExpression	Expression to evaluate
	//!
	//! @return	Result of the expression, as a double
	public double Execute( string sExpression )
	{
		return ExecuteTokens( TokenizeExpression( sExpression ) );
	}

	//! @brief	Converts a standard infix expression to list of tokens in postfix order
	//!
	//! @param	sExpression	Expression to evaluate
	//!
	//! @return	A list of tokens
	protected List<string> TokenizeExpression( string sExpression )
	{
		List<string> sTokens = new List<string>();
		Stack<string> stack = new Stack<string>();
		State eState = State.None;
		int nParenCount = 0;
		string sTemp;

		lwStringReader reader = new lwStringReader( sExpression );

		while( !reader.IsEOF() )
		{
			char c = (char)reader.Peek();
			if( Char.IsWhiteSpace( c ) )
			{
				// Ignore spaces, tabs, etc.
			}
			else if( c=='(' )
			{
				// Cannot follow operand
				if( eState==State.Operand ) throw new EvalException( ERROR_OPERATOR_EXPECTED, reader.Position );
				// Allow additional unary operators after "("
				if( eState==State.UnaryOperator ) eState = State.Operator;
				// Push opening parenthesis onto stack
				stack.Push( c.ToString() );
				// Track number of parentheses
				nParenCount++;
			}
			else if( c==')' )
			{
				// Must follow operand
				if( eState!=State.Operand ) throw new EvalException( ERROR_OPERAND_EXPECTED, reader.Position );
				// Must have matching open parenthesis
				if( nParenCount==0 ) throw new EvalException( ERROR_UNMATCHED_CLOSING_PAR, reader.Position );
				// Pop all operators until matching "(" found
				sTemp = stack.Pop();
				while( sTemp!="(" )
				{
					sTokens.Add( sTemp );
					sTemp = stack.Pop();
				}
				// Track number of parentheses
				nParenCount--;
			}
			else if( IsOperator( c ) )
			{
				// Need a bit of extra code to support unary operators
				if( eState==State.Operand )
				{
					// Pop operators with precedence >= current operator
					int nCurrentPrecedence = GetPrecedence( c.ToString() );
					while( stack.Count>0 && GetPrecedence( stack.Peek() )>=nCurrentPrecedence )
					{
						sTokens.Add( stack.Pop() );
					}
					stack.Push( c.ToString() );
					eState = State.Operator;
				}
				else if( eState==State.UnaryOperator )
				{
					// Don't allow two unary operators together
					throw new EvalException( ERROR_OPERAND_EXPECTED, reader.Position );
				}
				else
				{
					// Test for unary operator
					if( c=='-' )
					{
						// Push unary minus
						stack.Push( UNARY_MINUS );
						eState = State.UnaryOperator;
					}
					else if( c=='+' )
					{
						// Just ignore unary plus
						eState = State.UnaryOperator;
					}
					else
					{
						throw new EvalException( ERROR_OPERAND_EXPECTED, reader.Position );
					}
				}
			}
			else if( Char.IsDigit( c ) || c=='.' )
			{
				// Cannot follow other operand
				if( eState==State.Operand ) throw new EvalException( ERROR_OPERATOR_EXPECTED, reader.Position );
				// Parse number
				sTemp = ParseNumberToken( reader );
				sTokens.Add( sTemp );
				eState = State.Operand;
				continue;
			}
			else
			{
				double fResult;

				// Parse symbols and functions
				// Symbol or function cannot follow other operand
				if( eState==State.Operand ) throw new EvalException( ERROR_OPERATOR_EXPECTED, reader.Position );
				// Invalid character
				if( !( Char.IsLetter( c ) || c=='_' ) ) throw new EvalException( String.Format( ERROR_UNEXPECTED_CHARACTER, c ), reader.Position );
				// Save start of symbol for error reporting
				int nSymbolPos = reader.Position;
				// Parse this symbol
				sTemp = ParseSymbolToken( reader );
				// Skip whitespace
				SkipWhiteSpaces( reader );
				// Check for parameter list
				if( (char)reader.Peek()=='(' )
				{
					// Found parameter list, evaluate function
					fResult = EvaluateFunction( reader, sTemp, nSymbolPos );
				}
				else
				{
					// No parameter list, evaluate symbol (variable)
					fResult = EvaluateSymbol( sTemp, nSymbolPos );
				}
				// Handle negative result
				if( fResult<0 )
				{
					stack.Push( UNARY_MINUS );
					fResult = -fResult;
				}
				sTokens.Add( ((decimal)fResult).ToString() );
				eState = State.Operand;
				continue;
			}
			reader.Skip( 1 );
		}
		// Expression cannot end with operator
		if( eState==State.Operator || eState==State.UnaryOperator ) throw new EvalException( ERROR_OPERAND_EXPECTED, reader.Position );
		// Check for balanced parentheses
		if( nParenCount>0 ) throw new EvalException( ERROR_CLOSING_PAR_EXPECTED, reader.Position );
		// Retrieve remaining operators from stack
		while( stack.Count>0 )
		{
			sTokens.Add( stack.Pop() );
		}
		return sTokens;
	}

	//! @brief	Parses and extracts a numeric value at the current position
	//!
	//! @param	reader	lwStringReader object
	//!
	//! @return	A string containing the number token
	protected string ParseNumberToken( lwStringReader reader )
	{
		bool bHasDecimal = false;
		int nStart = reader.Position;
		do
		{
			char c = (char)reader.Peek();
			if( c=='.' )
			{
				if( bHasDecimal ) throw new EvalException( ERROR_MULTIPLE_DECIMAL_POINTS, reader.Position );
				bHasDecimal = true;
				reader.Skip( 1 );
			}
			else if( Char.IsDigit( c ) )
			{
				reader.Skip( 1 );
			}
			else break;
		}
		while( true );
		// Extract token
		string sToken = reader.Extract( nStart, reader.Position );
		if( sToken=="." ) throw new EvalException( ERROR_INVALID_OPERAND, reader.Position-1 );
		return sToken;
	}

	//! @brief	Parses and extracts a symbol at the current position
	//!
	//! @param	reader	lwStringReader object
	//!
	//! @return	A string containing the symbol token
	protected string ParseSymbolToken( lwStringReader reader )
	{
		int nStart = reader.Position;
		do
		{
			char c = (char)reader.Peek();
			if( !Char.IsLetterOrDigit( c ) && c!='_' ) break;
			reader.Skip( 1 );
		}
		while( true );
		return reader.Extract( nStart, reader.Position );
	}

	//! @brief	Evaluates a function and returns its value
	//!			It is assumed the current position is at the opening parenthesis of the argument list
	//!
	//! @param	reader	lwStringReader object
	//! @param	sName	Name of function
	//! @param	nPos	Position at start of function
	//!
	//! @return	Result of the function
	protected double EvaluateFunction( lwStringReader reader, string sName, int nPos )
	{
		double fResult = default(double);
		List<double> fParameters = ParseParameters( reader );
		FunctionStatus eStatus = FunctionStatus.UndefinedFunction;
		if( m_processFunction!=null )
		{
			FunctionEventArgs args = new FunctionEventArgs();
			args.sName = sName;
			args.fParameters = fParameters;
			args.fResult = fResult;
			args.eStatus = eStatus;
			m_processFunction( args );
			fResult = args.fResult;
			eStatus = args.eStatus;
		}
		if( eStatus==FunctionStatus.UndefinedFunction ) throw new EvalException( String.Format( ERROR_UNDEFINED_FUNCTION, sName ), nPos );
		if( eStatus==FunctionStatus.WrongParameterCount ) throw new EvalException( ERROR_WRONG_PARAM_COUNT, nPos );
		return fResult;
	}

	//! @brief	Evaluates each parameter of a function's parameter list
	//!			It is assumed the current position is at the opening parenthesis of the argument list
	//!
	//! @param	reader	lwStringReader object
	//!
	//! @return	A list of values
	//!			An empty list is returned if no parameters were found
	protected List<double> ParseParameters( lwStringReader reader )
	{
		// Move past open parenthesis
		reader.Skip( 1 );

		// Look for function parameters
		List<double> fParameters = new List<double>();
		SkipWhiteSpaces( reader );
		if( (char)reader.Peek()!= ')' )
		{
			// Parse function parameter list
			int nParamStart = reader.Position;
			int nParenCount = 1;
			while( !reader.IsEOF() )
			{
				if( (char)reader.Peek()==',' )
				{
					// Note: Ignore commas inside parentheses. They could be
					// from a parameter list for a function inside the parameters
					if( nParenCount==1 )
					{
						fParameters.Add( EvaluateParameter( reader, nParamStart ) );
						nParamStart = reader.Position+1;
					}
				}
				if( (char)reader.Peek()==')' )
				{
					nParenCount--;
					if( nParenCount==0 )
					{
						fParameters.Add( EvaluateParameter( reader, nParamStart ) );
						break;
					}
				}
				else if( (char)reader.Peek()=='(' )
				{
					nParenCount++;
				}
				reader.Skip( 1 );
			}
		}
		// Make sure we found a closing parenthesis
		if( (char)reader.Peek()!=')' ) throw new EvalException( ERROR_CLOSING_PAR_EXPECTED, reader.Position );
		// Move past closing parenthesis
		reader.Skip( 1 );
		// Return parameter list
		return fParameters;
	}

	//! @brief	Extracts and evaluates a function parameter and returns its value
	//!			If an exception occurs, it is caught and the column is adjusted to reflect
	//!			the position in original string, and the exception is rethrown
	//!
	//! @param	reader		lwStringReader object
	//! @param	nParamStart	Column where this parameter started
	//!
	//! @return	Value of the function parameter
	protected double EvaluateParameter( lwStringReader reader, int nParamStart )
	{
		try
		{
			// Extract expression and evaluate it
			string sExpression = reader.Extract( nParamStart, reader.Position );
			return Execute( sExpression );
		}
		catch( EvalException ee )
		{
			// Adjust column and rethrow exception
			ee.Column += nParamStart;
			throw ee;
		}
	}

	//! @brief	Evaluates a symbol name and returns its value
	//!
	//! @param	sName	Name of symbol
	//! @param	nPos	Position at start of symbol
	//!
	//! @return	Value of the symbol
	protected double EvaluateSymbol( string sName, int nPos )
	{
		double fResult = default(double);
		SymbolStatus eStatus = SymbolStatus.UndefinedSymbol;
		if( m_processSymbol!=null )
		{
			SymbolEventArgs args = new SymbolEventArgs();
			args.sName = sName;
			args.fResult = fResult;
			args.eStatus = eStatus;
			m_processSymbol( args );
			fResult = args.fResult;
			eStatus = args.eStatus;
		}
		if( eStatus==SymbolStatus.UndefinedSymbol ) throw new EvalException( String.Format(ERROR_UNDEFINED_SYMBOL, sName ), nPos );
		return fResult;
	}

	//! @brief	Evaluates the given list of tokens and returns the result
	//!			Tokens must appear in postfix order
	//!
	//! @param	sTokens	List of tokens to evaluate
	//!
	//! @return	Result of the evaluation
	protected double ExecuteTokens( List<string> sTokens )
	{
		Stack<double> stack = new Stack<double>();
		double fTmp1;
		double fTmp2;

		for( int i=0; i<sTokens.Count; i++ )
		{
			string sToken = sTokens[i];
			// Is this a value token?
			if( CountDigits( sToken )==sToken.Length )
			{
				stack.Push( double.Parse( sToken, System.Globalization.CultureInfo.InvariantCulture) );
			}
			else if( sToken=="+" )
			{
				fTmp1 = stack.Pop();
				fTmp2 = stack.Pop();
				stack.Push( fTmp2+fTmp1 );
			}
			else if( sToken=="-" )
			{
				fTmp1 = stack.Pop();
				fTmp2 = stack.Pop();
				stack.Push( fTmp2-fTmp1 );
			}
			else if( sToken=="*" )
			{
				fTmp1 = stack.Pop();
				fTmp2 = stack.Pop();
				stack.Push( fTmp2*fTmp1 );
			}
			else if( sToken=="/" )
			{
				fTmp1 = stack.Pop();
				fTmp2 = stack.Pop();
                double result = fTmp2 / fTmp1;
                //Debug.Log("division " + fTmp2 + " / " + fTmp1 + " = " + result);
				stack.Push( result );
			}
			else if( sToken=="^" )
			{
				fTmp1 = stack.Pop();
				fTmp2 = stack.Pop();
				stack.Push( Mathf.Pow( (float)fTmp2, (float)fTmp1 ) );
			}
			else if( sToken==UNARY_MINUS )
			{
				stack.Push( -stack.Pop() );
			}
            else
            {
                double d = 0;
                if( double.TryParse(sToken, out d) )
                {
                    stack.Push(d);
                }
            }
		}
		// Remaining item on stack contains result
		return( stack.Count>0 ) ? stack.Pop() : 0.0;
	}
	
	//! @brief	Handles common math symbols
	//!
	//! @param	e	SymbolEventArgs object
	protected void DefaultSymbolHandler( SymbolEventArgs e )
	{
		if( String.Compare( e.sName, "pi", true )==0 )
		{
			e.fResult = Mathf.PI;
			e.eStatus = SymbolStatus.OK;
		}
	}

	//! @brief	Handles common math functions
	//!
	//! @param	e	FunctionEventArgs object
	protected void DefaultFunctionHandler( FunctionEventArgs e )
	{
		if( String.Compare( e.sName, "abs", true )==0 )
		{
			if( e.fParameters.Count==1 )
			{
				e.fResult = Mathf.Abs( (float)e.fParameters[0] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
		else if( String.Compare( e.sName, "round", true )==0 )
		{
			if( e.fParameters.Count==1 )
			{
				e.fResult = Mathf.Round( (float)e.fParameters[0] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
		else if( String.Compare( e.sName, "ceil", true )==0 )
		{
			if( e.fParameters.Count==1 )
			{
				e.fResult = Mathf.Ceil( (float)e.fParameters[0] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
		else if( String.Compare( e.sName, "floor", true )==0 )
		{
			if( e.fParameters.Count==1 )
			{
				e.fResult = Mathf.Floor( (float)e.fParameters[0] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
		else if( String.Compare( e.sName, "sqrt", true )==0 )
		{
			if( e.fParameters.Count==1 )
			{
				e.fResult = Mathf.Sqrt( (float)e.fParameters[0] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
		else if( String.Compare( e.sName, "cos", true )==0 )
		{
			if( e.fParameters.Count==1 )
			{
				e.fResult = Mathf.Cos( (float)e.fParameters[0] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
		else if( String.Compare( e.sName, "sin", true )==0 )
		{
			if( e.fParameters.Count==1 )
			{
				e.fResult = Mathf.Sin( (float)e.fParameters[0] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
		else if( String.Compare( e.sName, "tan", true )==0 )
		{
			if( e.fParameters.Count==1 )
			{
				e.fResult = Mathf.Tan( (float)e.fParameters[0] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
		else if( String.Compare( e.sName, "min", true )==0 )
		{
			if( e.fParameters.Count==2 )
			{
				e.fResult = Mathf.Min( (float)e.fParameters[0], (float)e.fParameters[1] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
		else if( String.Compare( e.sName, "max", true )==0 )
		{
			if( e.fParameters.Count==2 )
			{
				e.fResult = Mathf.Max( (float)e.fParameters[0], (float)e.fParameters[1] );
				e.eStatus = FunctionStatus.OK;
			}
			else e.eStatus = FunctionStatus.WrongParameterCount;
		}
        else if (String.Compare(e.sName, "pow", true) == 0)
        {
            if (e.fParameters.Count == 2)
            {
                e.fResult = Mathf.Pow((float)e.fParameters[0], (float)e.fParameters[1]);
                e.eStatus = FunctionStatus.OK;
            }
            else e.eStatus = FunctionStatus.WrongParameterCount;
        }
    }
	
	//! @brief	Returns a value that indicates the relative precedence of the specified operator
	//!
	//! @param	sOperator	Operator to be tested
	//!
	//! @return	Precedence indicator
	private int GetPrecedence( string sOperator )
	{
		switch( sOperator )
		{
			case "+":
			case "-":
				return 1;
			case "*":
			case "/":
				return 2;
			case "^":
				return 3;
			case UNARY_MINUS:
				return 10;
		}
		return 0;
	}
	
	//! @brief	Check if a character is a numeric operator
	//!
	//! @param	cOperator	Operator to be tested
	//!
	//! @return	True if check succeeded
	private bool IsOperator( char cOperator )
	{
		return
		(
			cOperator=='+' ||
			cOperator=='-' ||
			cOperator=='*' ||
			cOperator=='/' ||
			cOperator=='^'
		);
	}
	
	//! @brief	Counts the number of digits in a string
	//!
	//! @param	sText	String to be parsed
	//!
	//! @return	Number of digits
	private int CountDigits( string sText )
	{
		int nDigits = 0;
		for( int i=0; i<sText.Length; i++ )
		{
			char c = sText[i];
			if( Char.IsDigit( c ) || c=='.' )
			{
				nDigits++;
			}
		}
		return nDigits;
	}
	
	//! @brief	Skip all white spaces in a lwStringReader object
	//!
	//! @param	reader	lwStringReader object
	private void SkipWhiteSpaces( lwStringReader reader )
	{
		while( Char.IsWhiteSpace( (char)reader.Peek() ) )
		{
			reader.Skip( 1 );
		}
	}
}
