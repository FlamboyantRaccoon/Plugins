#if !UNITY_FLASH
#define USE_TEXTENCODING
#endif

public sealed class lwRC4
{
	private byte[] m_sRC4;
	private byte[] m_pKey;
	private uint m_nKeyLength;
	private uint m_iRC4;
	private uint m_jRC4;

	// RC4 default key
	static public string RC4_KEY = "Little-Worlds Studio - 69";	
	
	public lwRC4()
	{
		m_sRC4 = new byte[256];
		m_iRC4 = 0;
		m_jRC4 = 0;
		SetKey( RC4_KEY );
	}
	
	public void SetKey( string sKey )
	{
		byte[] pKey = StringToByteArray( sKey );
		m_pKey = pKey;
		m_nKeyLength = (uint)pKey.Length;
	}
	
	public string GetKey()
	{
		return ByteArrayToString( m_pKey );
	}
	
	public string EncryptToString( byte[] pBytes )
	{
		return ByteArrayToString( Encrypt( pBytes ) );
	}

	public string EncryptToString( string sText )
	{
		return EncryptToString( StringToByteArray( sText ) );
	}
	
	public byte[] Encrypt( string sText )
	{
		return Encrypt( StringToByteArray( sText ) );
	}
	
	public byte[] Encrypt( byte[] pSrc )
	{
		if( pSrc==null || m_pKey==null ) return null;
		
		uint nSize = (uint)pSrc.Length;
		byte[] pDst = new byte[nSize];
		
		// Create the keystream
		InputKey( m_pKey, m_nKeyLength );
		
		// Encrypt
		for( uint x=0; x<nSize; x++ )
		{
			pDst[(int)x] = (byte)( pSrc[(int)x] ^ OutputKey() );
		}
		
		return pDst;
	}
	
	private byte[] StringToByteArray( string sData )
	{
		if( sData==null ) return null;
#if USE_TEXTENCODING
		return new System.Text.UTF8Encoding().GetBytes( sData );
#else
		byte[] pBytes = new byte[sData.Length*sizeof(char)];
#if UNITY_FLASH && !UNITY_EDITOR
		// Flash build doesn't like System.Buffer.BlockCopy...
		int j = 0;
		for( int i=0; i<sData.Length; i++ )
		{
			int nData = sData[i];
			pBytes[j] = (byte)( nData & 0xFF );
			pBytes[j+1] = (byte)( ( nData>>8 ) & 0xFF );
			j += 2;
		}
#else
		System.Buffer.BlockCopy( sData.ToCharArray(), 0, pBytes, 0, pBytes.Length );
#endif
		return pBytes;
#endif
	}
	
	private string ByteArrayToString( byte[] pBytes )
	{
		if( pBytes==null ) return null;
#if USE_TEXTENCODING
		// *** Use Default of Encoding.Default (Ansi CodePage)
		System.Text.Encoding enc = System.Text.Encoding.Default;
		if( pBytes.Length>=5 )
		{
			byte c = pBytes[0];
			if( c==0xef && pBytes[1]==0xbb && pBytes[2]==0xbf )
				enc = System.Text.Encoding.UTF8;
			else if( c==0xfe && pBytes[1]==0xff )
				enc = System.Text.Encoding.Unicode;
			else if( c==0 && pBytes[1]==0 && pBytes[2]==0xfe && pBytes[3]==0xff )
				enc = System.Text.Encoding.UTF32;
			else if( c==0x2b && pBytes[1]==0x2f && pBytes[2]==0x76 )
				enc = System.Text.Encoding.UTF7;
			else if( c==0xFE && pBytes[1]==0xFF )
				enc = System.Text.Encoding.GetEncoding( 1201 ); // 1201 unicodeFFFE Unicode (Big-Endian)
			else if( c==0xFF && pBytes[1]==0xFE )
				enc = System.Text.Encoding.GetEncoding( 1200 ); // 1200 utf-16 Unicode
		}
		return enc.GetString( pBytes );
#else
		char[] pChars = new char[pBytes.Length/sizeof(char)];
#if UNITY_FLASH && !UNITY_EDITOR
		// Flash build doesn't like System.Buffer.BlockCopy...
		int j = 0;
		for( int i=0; i<pBytes.Length>>1; i++ )
		{
			pChars[i] = (char)( pBytes[j] | ( pBytes[j+1]<<8 ) );
			j += 2;
		}
#else
		System.Buffer.BlockCopy( pBytes, 0, pChars, 0, pBytes.Length );
#endif
		return new string( pChars );
#endif
	}
	
	private void InputKey( byte[] sKey, uint nKeyLen )
	{
		for( m_iRC4=0; m_iRC4<256; m_iRC4++ )
		{
			m_sRC4[(int)m_iRC4] = (byte)m_iRC4;
		}
		for( m_iRC4=0, m_jRC4=0; m_iRC4<256; m_iRC4++ )
		{
			m_jRC4 = ( m_jRC4 + sKey[(int)(m_iRC4 % nKeyLen)] + m_sRC4[(int)m_iRC4] ) % 256;
			Swap( m_sRC4, m_iRC4, m_jRC4 );
		}
	}
	
	private byte OutputKey()
	{
		m_iRC4 = ( m_iRC4 + 1 ) % 256;
		m_jRC4 = ( m_jRC4 + m_sRC4[(int)m_iRC4] ) % 256;
		
		Swap( m_sRC4, m_iRC4, m_jRC4 );
		
		return m_sRC4[( m_sRC4[(int)m_iRC4] + m_sRC4[(int)m_jRC4] ) % 256];
	}
	
	private void Swap( byte[] s, uint i, uint j )
	{
		s[(int)i] ^= s[(int)j];
		s[(int)j] ^= s[(int)i];
		s[(int)i] ^= s[(int)j];
	}
}
