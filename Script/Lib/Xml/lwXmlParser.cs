using UnityEngine;
using System;
using System.Collections;
using System.Text;
//using System.Globalization;

public class lwXmlParser
{
	internal class DefaultHandler : lwXmlParser.IContentHandler
	{
		public void OnStartParsing( lwXmlParser parser )
		{
			//Debug.Log("### OnStartParsing " );
		}

		public void OnEndParsing( lwXmlParser parser )
		{
			//Debug.Log("### OnEndParsing " );
		}

		public void OnStartElement( string name, lwXmlParser.IAttrList attrs )
		{
			/*Debug.Log("### OnStartElement " + name + " ## " + attrs.Length );
			for( int i=0; i<attrs.Length; i++ )
			{
				Debug.Log("\t### elem : " + i + " : " + attrs.GetName( i ) + " ## " + attrs.GetValue( i ) );
			}*/
		}

		public void OnEndElement( string name )
		{
			//Debug.Log("### OnEndElement " + name );
		}

		public void OnChars( string s )
		{
			//Debug.Log("### OnChars " + s );
		}

		public void OnIgnorableWhitespace( string s )
		{
			//Debug.Log("### OnIgnorableWhitespace " + s );
		}

		public void OnProcessingInstruction( string name, string text )
		{
			//Debug.Log("### OnProcessingInstruction " + name + " ## " + text );
		}
	}
	
	public interface IContentHandler
	{
		void OnStartParsing( lwXmlParser parser );
		void OnEndParsing( lwXmlParser parser );
		void OnStartElement( string name, IAttrList attrs );
		void OnEndElement( string name );
		void OnProcessingInstruction( string name, string text );
		void OnChars( string text );
		void OnIgnorableWhitespace( string text );
	}
	
	public interface IAttrList
	{
		int Length { get; }
		bool IsEmpty { get; }
		string GetName( int i );
		string GetValue( int i );
		string GetValue( string name );
		string[] Names { get; }
		string[] Values { get; }
	}

	class AttrListImpl : IAttrList
	{
		public int Length
		{
			get { return attrNames.Count; }
		}
		public bool IsEmpty
		{
			get { return attrNames.Count==0; }
		}
		public string GetName( int i )
		{
			return (string)attrNames[i];
		}
		public string GetValue( int i )
		{
			return (string)attrValues[i];
		}
		public string GetValue( string name )
		{
			for( int i = 0; i < attrNames.Count; i++ )
				if( (string)attrNames[i] == name )
					return (string)attrValues[i];
			return null;
		}
		public string[] Names
		{
			get { return (string[])attrNames.ToArray( typeof(string) ); }
		}
		public string[] Values
		{
			get { return (string[])attrValues.ToArray( typeof(string) ); }
		}

		ArrayList attrNames = new ArrayList();
		ArrayList attrValues = new ArrayList();

		internal void Clear()
		{
			attrNames.Clear();
			attrValues.Clear();
		}

		internal void Add( string name, string value )
		{
			attrNames.Add( name );
			attrValues.Add( value );
		}
	}
	
	IContentHandler handler;
	lwStringReader reader;
	Stack elementNames = new Stack();
	Stack xmlSpaces = new Stack();
	string xmlSpace;
	StringBuilder buffer = new StringBuilder(200);
	char[] nameBuffer = new char[30];
	string m_sName;
	string m_sValue;
	bool m_bHasEnd;
		
	bool isWhitespace;
	lwXmlReader.lwXmlNodeType m_NodeType = lwXmlReader.lwXmlNodeType.None;

	AttrListImpl attributes = new AttrListImpl();
	int line = 1, column;
	bool resetColumn;

	public lwXmlParser()
	{
	}

	private Exception Error( string msg )
	{
		return new lwXmlParserException( msg, line, column );
	}

	private Exception UnexpectedEndError()
	{
		string[] arr = new string[elementNames.Count];
		// COMPACT FRAMEWORK NOTE: CopyTo is not visible through the Stack class
		(elementNames as ICollection).CopyTo( arr, 0 );
		return Error( String.Format( "Unexpected end of stream. Element stack content is {0}", String.Join( ",", arr ) ) );
	}

	private bool IsNameChar( char c, bool start )
	{
		switch( c )
		{
		case ':':
		case '_':
			return true;
		case '-':
		case '.':
			return !start;
		}
		if( c > 0x100 )
		{ // optional condition for optimization
			switch( c )
			{
			case '\u0559':
			case '\u06E5':
			case '\u06E6':
				return true;
			}
			if( '\u02BB' <= c && c <= '\u02C1' )
				return true;
		}
		
		// TODO complete with char
		//Debug.Log("##### character : " + c + " ## " + Char.GetUnicodeCategory( c ) + " ## " + uniReturn + "## " + start );*/
		switch( c )
		{
		case ' ':
		case '\n':
		case '=':
		case '"':
		case '>':
		case '<':	
		case '?':
		case '/':
			return false;
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			return !start;
		default:
			return true;
		}
		
		/*switch( Char.GetUnicodeCategory( c ) )
		{
		case UnicodeCategory.LowercaseLetter:
		case UnicodeCategory.UppercaseLetter:
		case UnicodeCategory.OtherLetter:
		case UnicodeCategory.TitlecaseLetter:
		case UnicodeCategory.LetterNumber:
			return true;
		case UnicodeCategory.SpacingCombiningMark:
		case UnicodeCategory.EnclosingMark:
		case UnicodeCategory.NonSpacingMark:
		case UnicodeCategory.ModifierLetter:
		case UnicodeCategory.DecimalDigitNumber:
			return !start;
		default:
			return false;
		}*/

	}

	private bool IsWhitespace( int c )
	{
		switch( c )
		{
		case ' ':
		case '\r':
		case '\t':
		case '\n':
			return true;
		default:
			return false;
		}
	}

	private void SkipWhitespaces()
	{
		SkipWhitespaces( false );
	}

	private void HandleWhitespaces()
	{
		while( IsWhitespace( Peek() ) )
			buffer.Append( (char)ReadChar() );
		if( Peek() != '<' && Peek() >= 0 )
			isWhitespace = false;
	}

	private void SkipWhitespaces( bool expected )
	{
		while( true )
		{
			if( IsWhitespace( Peek() ) )
			{
				ReadChar();
				if( expected )
					expected = false;
				continue;
			}
			if( expected )
				throw Error( "Whitespace is expected." );
			return;
		}
	}

	private int Peek()
	{
		return reader.Peek();
	}

	private int ReadChar()
	{
		int i = reader.Read();
		if( i == '\n' )
			resetColumn = true;
		if( resetColumn )
		{
			line++;
			resetColumn = false;
			column = 1;
		}
		else
			column++;
		return i;
	}

	private void Expect( int c )
	{
		int p = ReadChar();
		if( p < 0 )
			throw UnexpectedEndError();
		else if( p != c )
			throw Error( String.Format( "Expected '{0}' but got {1}", (char)c, (char)p ) );
	}

	private string ReadUntil( char until, bool handleReferences )
	{
		while( true )
		{
			if( Peek() < 0 )
				throw UnexpectedEndError();
			char c = (char)ReadChar();
			if( c == until )
				break;
			else if( handleReferences && c == '&' )
				ReadReference();
			else
				buffer.Append( c );
		}
		string ret = buffer.ToString();
		buffer.Length = 0;
		return ret;
	}

	private string ReadName()
	{
		int idx = 0;
		if( Peek() < 0 || !IsNameChar( (char) Peek(), true ) )
			throw Error( "XML name start character is expected." );
		for( int i = Peek(); i >= 0; i = Peek() )
		{
			char c = (char)i;
			if( !IsNameChar( c, false ) )
				break;
			if( idx == nameBuffer.Length )
			{
				char[] tmp = new char[idx * 2];
				// COMPACT FRAMEWORK NOTE: Array.Copy(sourceArray, destinationArray, count) is not available.
				Array.Copy( nameBuffer, 0, tmp, 0, idx );
				nameBuffer = tmp;
			}
			nameBuffer[idx++] = c;
			ReadChar();
		}
		
		int k=1;
		k = k*k;
		
		if( idx == 0 )
			throw Error( "Valid XML name is expected." );
		
		string sReturn = "";
		for( int i=0; i<idx; i++ )
		{
			sReturn += nameBuffer[i].ToString();
		}
		return sReturn;
	}


	public void Parse( lwStringReader input, IContentHandler handler )
	{
		this.reader = input;
		this.handler = handler;
		
		if( handler != null )
			handler.OnStartParsing( this );

		while( Peek() >= 0 )
			ReadContent();
		HandleBufferedContent();
		if( elementNames.Count > 0 )
			throw Error( String.Format( "Insufficient close tag: {0}", elementNames.Peek() ) );

		if( handler != null )
			handler.OnEndParsing( this );

		Cleanup();
	}
	
	private void Cleanup()
	{
		line = 1;
		column = 0;
		handler = null;
		Close();
#if CF_1_0
		elementNames = new Stack();
		xmlSpaces = new Stack();
#else
		elementNames.Clear();
		xmlSpaces.Clear();
#endif
		attributes.Clear();
		buffer.Length = 0;
		xmlSpace = null;
		isWhitespace = false;
		m_NodeType = lwXmlReader.lwXmlNodeType.None;
	}

	private bool ReadContent()
	{
		if( IsWhitespace( Peek() ) )
		{
			if( buffer.Length == 0 )
				isWhitespace = true;
			HandleWhitespaces();
		}
		if( Peek() != '<' )
		{
			buffer.Length = 0;
			ReadCharacters();
			m_sValue = buffer.ToString();
			buffer.Length = 0;
			return false;
		}
		ReadChar();
		switch( Peek() )
		{
			case '!': // declarations
				m_NodeType = lwXmlReader.lwXmlNodeType.CDATA;
				ReadChar();
				if( Peek() == '[' )
				{
					ReadChar();
					if( ReadName() != "CDATA" )
						throw Error( "Invalid declaration markup" );
					Expect( '[' );
					ReadCDATASection();
					break;
				}
				else if( Peek() == '-' )
				{
					ReadComment();
					break;
				}
				else if( ReadName() != "DOCTYPE" )
					throw Error( "Invalid declaration markup." );
				else
					throw Error( "This parser does not support document type." );
			
			case '?': // PIs
				m_NodeType = lwXmlReader.lwXmlNodeType.ProcessingInstruction;
				HandleBufferedContent();
				ReadChar();
				m_sName = ReadName();
				SkipWhitespaces();
				string text = String.Empty;
				if( Peek() != '?' )
				{
					while( true )
					{
						text += ReadUntil( '?', false );
						if( Peek() == '>' )
							break;
						text += "?";
					}
				}
				m_bHasEnd = true;
				if( handler != null )
					handler.OnProcessingInstruction( m_sName, text );
				Expect( '>' );
				break;
			
			case '/': // end tags
				m_NodeType = lwXmlReader.lwXmlNodeType.EndElement;
				HandleBufferedContent();
				if( elementNames.Count == 0 )
					throw UnexpectedEndError();
				ReadChar();
				m_sName = ReadName();
				SkipWhitespaces();
				string expected = (string)elementNames.Pop();
				xmlSpaces.Pop();
				if( xmlSpaces.Count > 0 )
					xmlSpace = (string)xmlSpaces.Peek();
				else
					xmlSpace = null;
				if( m_sName != expected )
					throw Error( String.Format( "End tag mismatch: expected {0} but found {1}", expected, m_sName ) );
				m_bHasEnd = true;
				if( handler != null )
					handler.OnEndElement( m_sName );
				Expect( '>' );
				break;
			
			default: // start tags (including empty tags)
				m_NodeType = lwXmlReader.lwXmlNodeType.Element;
				HandleBufferedContent();
				m_sName = ReadName();
				m_bHasEnd = false;
				attributes.Clear();
				while( Peek() != '>' && Peek() != '/' )
					ReadAttribute( attributes );
				if( handler != null )
					handler.OnStartElement( m_sName, attributes );
				SkipWhitespaces();
				if( Peek() == '/' )
				{
					ReadChar();
					m_bHasEnd = true;
					if( handler != null )
						handler.OnEndElement( m_sName );
				}
				else
				{
					elementNames.Push( m_sName );
					xmlSpaces.Push( xmlSpace );
				}
				Expect( '>' );
				break;
		}
		return true;
	}

	private void HandleBufferedContent()
	{
		if( buffer.Length == 0 )
			return;
		if( isWhitespace )
			if( handler != null )
				handler.OnIgnorableWhitespace( buffer.ToString() );
		else
			if( handler != null )
				handler.OnChars( buffer.ToString() );
		buffer.Length = 0;
		isWhitespace = false;
	}

	private void ReadCharacters()
	{
		isWhitespace = false;
		while( true )
		{
			int i = Peek();
			switch( i )
			{
			case -1:
				return;
			case '<':
				return;
			case '&':
				ReadChar();
				ReadReference();
				continue;
			default:
				buffer.Append( (char)ReadChar() );
				continue;
			}
		}
	}

	private void ReadReference()
	{
		if( Peek() == '#' )
		{
			// character reference
			ReadChar();
			ReadCharacterReference();
		}
		else
		{
			string name = ReadName();
			Expect( ';' );
			switch( name )
			{
			case "amp":
				buffer.Append( '&' );
				break;
			case "quot":
				buffer.Append( '"' );
				break;
			case "apos":
				buffer.Append( '\'' );
				break;
			case "lt":
				buffer.Append( '<' );
				break;
			case "gt":
				buffer.Append( '>' );
				break;
			default:
				throw Error( "General non-predefined entity reference is not supported in this parser." );
			}
		}
	}

	private int ReadCharacterReference()
	{
		int n = 0;
		if( Peek() == 'x' )
		{ // hex
			ReadChar();
			for( int i = Peek(); i >= 0; i = Peek() )
			{
				if( '0' <= i && i <= '9' )
					n = n << 4 + i - '0';
				else if( 'A' <= i && i <='F' )
					n = n << 4 + i - 'A' + 10;
				else if( 'a' <= i && i <='f' )
					n = n << 4 + i - 'a' + 10;
				else
					break;
				ReadChar();
			}
		}
		else
		{
			for( int i = Peek(); i >= 0; i = Peek() )
			{
				if( '0' <= i && i <= '9' )
					n = n << 4 + i - '0';
				else
					break;
				ReadChar();
			}
		}
		return n;
	}

	private void ReadAttribute( AttrListImpl a )
	{
		SkipWhitespaces( true );
		if( Peek() == '/' || Peek() == '>' )
		// came here just to spend trailing whitespaces
			return;

		string name = ReadName();
		string value;
		SkipWhitespaces();
		Expect( '=' );
		SkipWhitespaces();
		switch( ReadChar() )
		{
		case '\'':
			value = ReadUntil( '\'', true );
			break;
		case '"':
			value = ReadUntil( '"', true );
			break;
		default:
			throw Error( "Invalid attribute value markup." );
		}
		if( name == "xml:space" )
			xmlSpace = value;
		a.Add( name, value );
	}

	private void ReadCDATASection()
	{
		int nBracket = 0;
		while( true )
		{
			if( Peek() < 0 )
				throw UnexpectedEndError();
			char c = (char)ReadChar();
			if( c == ']' )
				nBracket++;
			else if( c == '>' && nBracket > 1 )
			{
				for( int i = nBracket; i > 2; i-- )
					buffer.Append( ']' );
				break;
			}
			else
			{
				for( int i = 0; i < nBracket; i++ )
					buffer.Append( ']' );
				nBracket = 0;
				buffer.Append( c );
			}
		}
	}

	private void ReadComment()
	{
		Expect( '-' );
		Expect( '-' );
		while( true )
		{
			if( ReadChar() != '-' )
				continue;
			if( ReadChar() != '-' )
				continue;
			if( ReadChar() != '>' )
				throw Error( "'--' is not allowed inside comment markup." );
			break;
		}
	}
	
	public void Create( string sXml )
	{
		this.reader = new lwStringReader( sXml );
		this.handler = null;
	}
	
	public void Close()
	{
		this.reader = null;
	}

	public bool Read()
	{
		if( reader != null )
		{
			if( Peek() >= 0 )
			{
				ReadContent();
				return true;
			}
			else
			{
				HandleBufferedContent();
				if( elementNames.Count > 0 )
					throw Error( String.Format( "Insufficient close tag: {0}", elementNames.Peek() ) );
				//Cleanup();
				return false;
			}
		}
		return false;
	}

	public string ReadInnerXml()
	{
		if( HasEnd ) return string.Empty;
		
		int nPos = reader.Position;
		int nLastPos = nPos;
		int nRead = 0;
		int nDepth = 1;
		string sName = Name.ToUpper();
		
		while( Read() )
		{
			nRead = reader.Position - nLastPos;
			nLastPos = reader.Position;
			if( NodeType==lwXmlReader.lwXmlNodeType.Element && !HasEnd )
			{
				nDepth++;
			}
			else if( NodeType==lwXmlReader.lwXmlNodeType.EndElement )
			{
				nDepth--;
				if( nDepth==0 && Name.ToUpper()==sName )
				{
					break;
				}
			}
		}
		
		int nSize = nLastPos - nPos - nRead;
		reader.Position = nPos;
		
		string sContent = reader.Read( nSize );
		reader.Skip( nRead );
		return sContent;
	}
	
	public lwXmlReader.lwXmlNodeType NodeType
	{
		get { return m_NodeType; }
	}
	
	public string Name
	{
		get { return m_sName; }
	}

	public string Value
	{
		get { return m_sValue; }
	}

	public bool HasEnd
	{
		get { return m_bHasEnd; }
	}
	
	public string GetAttribute( string sAttr )
	{
		if( attributes != null )
		{
			return attributes.GetValue( sAttr );
		}
		return null;
	}
	
	public string[] GetAttributeNames()
	{
		if( attributes!=null )
			return attributes.Names;
		else
			return null;
	}
	
	public string[] GetAttributeValues()
	{
		if( attributes!=null )
			return attributes.Values;
		else
			return null;
	}
}

internal class lwXmlParserException : SystemException
{
	int m_nLine;
	int m_nColumn;

	public lwXmlParserException( string msg, int line, int column )
	: base( String.Format( "{0}. At ({1},{2})", msg, line, column ) )
	{
		m_nLine = line;
		m_nColumn = column;
	}

	public int Line
	{
		get { return m_nLine; }
	}

	public int Column
	{
		get { return m_nColumn; }
	}
}
