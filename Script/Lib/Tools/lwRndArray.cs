public sealed class lwRndArray
{
	private byte[] m_buffer = null;
	private uint m_nByteCount = 0;
	private uint m_nNbrValues = 0;
	private uint m_nNbrLeft = 0;
	private uint m_nFirstFreeByte = 0;
	private bool m_bEnsureNoRepeat = false;
	private int m_nLastValue = -1;
	
	public lwRndArray( uint nNbrValues )
	{
		InitRndArray( nNbrValues, false );
	}
	
	public lwRndArray( uint nNbrValues, bool bEnsureNoRepeat )
	{
		InitRndArray( nNbrValues, bEnsureNoRepeat );
	}

    public void Delete()
	{
		Destroy();
	}
	
	public void InitRndArray( uint nNbrValues, bool bEnsureNoRepeat )
	{
		if( nNbrValues>0 )
		{
			m_bEnsureNoRepeat = bEnsureNoRepeat;
			Init( nNbrValues );
		}
	}
	
	public void Init( uint nNbrValues )
	{
		m_nByteCount = nNbrValues>>3;
		if( (m_nByteCount<<3) < nNbrValues )
		{
			m_nByteCount++;
		}
		m_buffer = new byte[m_nByteCount];
		m_nNbrValues = nNbrValues;
		Reset();
	}

	public void Destroy()
	{
		m_buffer = null;
	}
	
	public void Reset()
	{
		for( int i=0; i<m_nByteCount; i++ )
		{
			m_buffer[i] = 0;
		}
		m_nNbrLeft = m_nNbrValues;
		m_nFirstFreeByte = 0;
	}

	public uint ChooseValue( bool bUseSeed=false )
	{
		if( m_nNbrLeft==0 )
		{
			Reset();
		}
		
		uint nNum = 1;
		if( m_nNbrLeft>1 )
		{	
 
            if(bUseSeed )
            {
                nNum = RrRndHandler.RndRange(1, m_nNbrLeft + 1);
            }
            else
            {
                nNum = (uint)UnityEngine.Random.Range(0, (int)m_nNbrLeft - 1) + 1;
            }
            //nNum = lwRndHandler.RndRange( 1, m_nNbrLeft+1 ); // (uint)UnityEngine.Random.Range( 1, m_nNbrLeft+1 );
            if ( m_bEnsureNoRepeat && m_nNbrLeft==m_nNbrValues && m_nNbrValues>1 )
			{
				while( nNum==m_nLastValue+1 )
				{
					
                    if (bUseSeed)
                    {
                        nNum = (uint)(RrRndHandler.RndRange(0, (int)m_nNbrLeft - 1) + 1);
                    }
                    else
                    {
                        nNum = (uint)UnityEngine.Random.Range(0, (int)m_nNbrLeft - 1) + 1;
                    }
                    //nNum = lwRndHandler.RndRange( 1, m_nNbrLeft+1 ); // (uint)UnityEngine.Random.Range( 1, m_nNbrLeft+1 );
                }
			}
		}
		
		uint nIndex = m_nFirstFreeByte;
		uint xBits = m_buffer[(int)nIndex];
		uint nValue = m_nFirstFreeByte << 3;
		byte xMask = 1;
		
		while( nNum>0 )
		{
			if( (xBits & xMask)==0 )
			{
				nNum--;
				if( nNum==0 )
				{
					break;
				}
			}
			if( xMask==0x80 )
			{
				while( true )
				{
					xBits = m_buffer[(int)(++nIndex)];
					if( xBits==0 && nNum>8 )
					{
						nValue += 8;
						nNum -= 8;
						continue;
					}
					if( xBits==0xFF )
					{
						nValue += 8;
						continue;
					}
					break;
				}
				xMask = 1;
			}
			else
			{
				xMask <<= 1;
			}
			nValue++;
		}
		
		m_buffer[(int)nIndex] |= xMask;
		
		DecreaseNbrLeft( nIndex );
		
		m_nLastValue = (int)nValue;
		
		return nValue;
	}

	public bool SetValueAsChoosen( uint nValue )
	{
		uint nIndex;
		byte xMask;
		if( !SplitValue( nValue, out nIndex, out xMask ) )
		{
			m_buffer[(int)nIndex] |= xMask;
			DecreaseNbrLeft( nIndex );
			return true;
		}
		return false;
	}

	public bool SetValueAsAvailable( uint nValue )
	{
		uint nIndex;
		byte xMask;
		if( SplitValue( nValue, out nIndex, out xMask ) )
		{
			m_buffer[(int)nIndex] &= (byte)~(int)(xMask);
			IncreaseNbrLeft( nIndex );
			return true;
		}
		return false;
	}

	public bool HasValueBeenChoosen( uint nValue )
	{
		uint nIndex;
		byte xMask;
		return SplitValue( nValue, out nIndex, out xMask );
	}

	public uint GetNbrValues()
	{
		return m_nNbrValues;
	}

	public uint GetNbrLeft()
	{
		return m_nNbrLeft;
	}
	
	private void DecreaseNbrLeft( uint nIndex )
	{
		m_nNbrLeft--;
		if( m_nNbrLeft>0 && nIndex==m_nFirstFreeByte )
		{
			uint nData = nIndex;
			while( nData<m_nByteCount && m_buffer[(int)nData]==0xFF )
			{
				m_nFirstFreeByte++;
				nData++;
			}
		}
	}

	private void IncreaseNbrLeft( uint nIndex )
	{
		m_nNbrLeft++;
		uint nByteIndex = nIndex>>3;
		if( nByteIndex<m_nFirstFreeByte )
		{
			m_nFirstFreeByte = nByteIndex;
		}
	}
	
	private bool SplitValue( uint nValue, out uint nIndex, out byte xMask )
	{
		nIndex = nValue >> 3;
		xMask = (byte)( 1 << ((int)nValue & 7) );
		return ( (m_buffer[(int)nIndex] & xMask)!=0 );
	}
}
