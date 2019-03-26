public sealed class lwStringReader
{
    private string m_sContent = null;
    private int m_nPos = 0;
    private int m_nLength = 0;

    public lwStringReader(string sInput)
    {
        if (sInput == null) UnityEngine.Debug.LogWarning("lwStringReader created with null string");
        m_sContent = sInput;
        m_nLength = (sInput == null) ? 0 : sInput.Length;
        m_nPos = 0;
    }

    public int Position
    {
        get { return m_nPos; }
        set { m_nPos = value; }
    }

    public int PeekAt(int nPos)
    {
        if (nPos >= m_nLength) return -1;
        return (int)m_sContent[nPos];
    }

    public int Peek()
    {
        return PeekAt(m_nPos);
    }

    public int Read()
    {
        if (IsEOF()) return -1;
        return (int)m_sContent[m_nPos++];
    }

    public string Read(int nChars)
    {
        string sContent = m_sContent.Substring(m_nPos, nChars);
        Skip(nChars);
        return sContent;
    }

    public string ReadLine()
    {
        if (Peek() == -1) return null;
        int nPos = m_nPos;
        int nChar = -1;
        do
        {
            nChar = PeekAt(nPos++);
        }
        while (nChar != -1 && nChar != 0x0D && nChar != 0x0A);
        string sLine = Read(nPos - m_nPos - 1);
        if (nChar != -1)
        {
            int nSkip = nPos;
            do
            {
                nChar = PeekAt(nSkip++);
            }
            while (nChar != -1 && (nChar == 0x0D || nChar == 0x0A));
            Skip(nSkip - nPos);
        }
        return sLine;
    }

    public string Extract(int nBegin, int nEnd)
    {
        return m_sContent.Substring(nBegin, nEnd - nBegin);
    }

    public void Skip(int nChars)
    {
        m_nPos += nChars;
        if (m_nPos > m_nLength) m_nPos = m_nLength;
    }

    public bool IsEOF()
    {
        return (m_nPos >= m_nLength);
    }

    public void Dispose()
    {
        m_sContent = null;
        m_nPos = 0;
        m_nLength = 0;
    }
}
