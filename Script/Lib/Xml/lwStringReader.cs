public class lwStringReader
{
	private string m_sContent = null;
	private int m_nPos = 0;
	private int m_nLength = 0;
	
	public lwStringReader( string sInput )
	{
		if( sInput==null ) UnityEngine.Debug.LogWarning( "lwStringReader created with null string" ) ;
		m_sContent = sInput;
		m_nLength = ( sInput==null ) ? 0 : sInput.Length;
		m_nPos = 0;
	}
	
	public int Position
	{
		get { return m_nPos; }
		set { m_nPos = value; }
	}
	
	public int Peek()
	{
		if( m_nPos==m_nLength ) return -1;
		return (int)m_sContent[m_nPos];
	}
	
	public int Read() 
	{
		if( m_nPos==m_nLength ) return -1;
		return (int)m_sContent[m_nPos++];
	}

	public string Read( int nChars )
	{
		string sContent = m_sContent.Substring( m_nPos, nChars );
		Skip( nChars );
		return sContent;
	}
	
	public void Skip( int nChars )
	{
		m_nPos += nChars;
		if( m_nPos>m_nLength ) m_nPos = m_nLength;
	}
	
	public void Dispose()
	{
		m_sContent = null;
		m_nPos = 0;
		m_nLength = 0;
	}
}
