// LWS XML parser is enabled by default, to avoid using System.Xml class
// which is not available on Unity Flash builds
#define USE_LWXMLPARSER
using System;
using UnityEngine;
using System.IO;
#if !USE_LWXMLPARSER
using System.Xml;
#endif

public class lwXmlReader
{
	public delegate void ItemParseDelegate( lwXmlReader reader );
	public enum lwXmlNodeType { None, Element, CDATA, ProcessingInstruction, EndElement, WhiteSpace, Comment, XmlDeclaration, Text };

    public const string INNER_SUBTREE_ROOT_NAME = "INNER_SUBTREE";

#if USE_LWXMLPARSER
	lwXmlParser m_xr;
#else
    XmlReader m_xr;
#endif
	
	public lwXmlReader()
	{
	}
	
	public bool Init( string sXml )
	{
		return Init( sXml, false );
	}
	
	public bool Init( string sXml, bool bFromResource )
	{
		if( string.IsNullOrEmpty( sXml ) ) return false;
#if USE_LWXMLPARSER
		m_xr = new lwXmlParser();
		m_xr.Create( sXml );
		return true;
#else
		StringReader sr = new StringReader( sXml );
	#if !UNITY_4_0
		if( bFromResource ) sr.Read();	// skip BOM
	#endif
		m_xr = XmlReader.Create( sr );
		return( m_xr!=null );
#endif
	}
	
	public void Close()
	{
		m_xr.Close();
	}
	
	public void Clean()
	{
		m_xr = null;
	}
	
	public bool Read()
	{
		return m_xr.Read();
	}

    public bool HasElementEnd()
    {
        return m_xr.HasEnd;
    }

    public string ReadInnerXml()
	{
        string s = "";
        try
        {
            s = m_xr.ReadInnerXml();
        }
        catch ( Exception e)
        {
            Debug.Log("WTF : " + e.Message );
        }
		return s;
	}

    public lwXmlReader ReadSubtree()
    {
        lwXmlReader xrInner = null;
        string sInnerXml = ReadInnerXml();
        if (!string.IsNullOrEmpty(sInnerXml))
        {
            xrInner = new lwXmlReader();
            xrInner.Init("<" + INNER_SUBTREE_ROOT_NAME + ">" + sInnerXml + "</" + INNER_SUBTREE_ROOT_NAME + ">");
        }
        return xrInner;
    }

    public lwXmlNodeType NodeType
	{
 		get { return GetNodeType(); }
	}
	
	public string Name
	{
 		get { return GetName(); }
	}
	
	public string Value
	{
 		get { return GetValue(); }
	}
	
	public bool IsNodeElement()
	{
#if USE_LWXMLPARSER
		return ( m_xr.NodeType==lwXmlNodeType.Element );
#else
		return ( m_xr.NodeType==XmlNodeType.Element );
#endif
	}

	public lwXmlNodeType GetNodeType()
	{
#if USE_LWXMLPARSER
		return m_xr.NodeType;
#else
		switch( m_xr.NodeType )
		{
			case XmlNodeType.Element:				return lwXmlNodeType.Element;
			case XmlNodeType.EndElement:			return lwXmlNodeType.EndElement;
			case XmlNodeType.CDATA:					return lwXmlNodeType.CDATA;
			case XmlNodeType.ProcessingInstruction:	return lwXmlNodeType.ProcessingInstruction;
			case XmlNodeType.Comment:				return lwXmlNodeType.Comment;
			case XmlNodeType.Whitespace:			return lwXmlNodeType.WhiteSpace;
			case XmlNodeType.XmlDeclaration:		return lwXmlNodeType.XmlDeclaration;
		}
		Debug.LogWarning( "Type not yet implemented in lwXmlReader ! : " + m_xr.NodeType );
		return lwXmlNodeType.None;
#endif
	}
	
	public string GetName()
	{
		return m_xr.Name;
	}
	
	public string GetValue()
	{
		return m_xr.Value;
	}

	public string[] GetAttributeNames()
	{
#if USE_LWXMLPARSER
		return m_xr.GetAttributeNames();
#else
		int nCount = m_xr.AttributeCount;
		string[] sNames = new string[nCount];
		for( int i=0; i<nCount; i++ )
		{
			m_xr.MoveToAttribute( i );
			sNames[i] = m_xr.Name;
		}
		m_xr.MoveToElement();
		return sNames;
#endif
	}
	
	public string[] GetAttributeValues()
	{
#if USE_LWXMLPARSER
		return m_xr.GetAttributeValues();
#else
		int nCount = m_xr.AttributeCount;
		string[] sValues = new string[nCount];
		for( int i=0; i<nCount; i++ )
		{
			m_xr.MoveToAttribute( i );
			sValues[i] = m_xr.Value;
		}
		m_xr.MoveToElement();
		return sValues;
#endif
	}

	public string GetAttribute( string sAttr )
	{
		return GetAttribute( sAttr, null );
	}
	
	public string GetAttribute( string sAttr, string sDefault )
	{
#if USE_LWXMLPARSER
		string sValue = m_xr.GetAttribute( sAttr );
		if( string.IsNullOrEmpty( sValue ) )
			return sDefault;
		else
			return sValue;
#else
		return lwTools.GetXmlAttribute( m_xr, sAttr, sDefault );
#endif
	}
	
	public float GetAttributeFloat( string sAttr, float fDefault )
	{
#if USE_LWXMLPARSER
		return float.Parse( GetAttribute( sAttr, fDefault.ToString() ), System.Globalization.CultureInfo.InvariantCulture);
#else
		return lwTools.GetXmlAttributeFloat( m_xr, sAttr, fDefault );
#endif
	}
	
	public int GetAttributeInt( string sAttr, int nDefault )
	{
#if USE_LWXMLPARSER
		return int.Parse( GetAttribute( sAttr, nDefault.ToString() ) );
#else
		return lwTools.GetXmlAttributeInt( m_xr, sAttr, nDefault );
#endif
	}
	
	public bool GetAttributeBool( string sAttr, bool bDefault )
	{
#if USE_LWXMLPARSER
		return lwTools.ReadBool( GetAttribute( sAttr, bDefault.ToString() ), bDefault );
#else
		return lwTools.GetXmlAttributeBool( m_xr, sAttr, bDefault );
#endif
	}

	public T GetAttributeEnum<T>( string sAttr, T nDefault )
	{
#if USE_LWXMLPARSER
		return (T)System.Enum.Parse( typeof(T), GetAttribute( sAttr, nDefault.ToString() ) );
#else
		return lwTools.GetXmlAttributeEnum<T>( m_xr, sAttr, nDefault );
#endif
	}

	public Vector2 GetAttributeVector2( Vector2 vDefault )
	{
#if USE_LWXMLPARSER
		return new Vector2( GetAttributeFloat( "x", vDefault.x ),		
							GetAttributeFloat( "y", vDefault.y ) );
#else
		return lwTools.GetXmlAttributeVector2( m_xr, vDefault );
#endif
	}

	public Vector3 GetAttributeVector3( Vector3 vDefault )
	{
#if USE_LWXMLPARSER
		return new Vector3( GetAttributeFloat( "x", vDefault.x ),		
							GetAttributeFloat( "y", vDefault.y ),
							GetAttributeFloat( "z", vDefault.z ) );
#else
		return lwTools.GetXmlAttributeVector3( m_xr, vDefault );
#endif
	}

	public void ParseElement( string sName, ItemParseDelegate Dlg )
	{
		while( Read() && ( GetNodeType()!=lwXmlNodeType.EndElement || GetName().ToUpper()!=sName.ToUpper() ) )
		{
			if( GetNodeType()==lwXmlNodeType.Element )
				Dlg( this );
		}
	}
	
	// ------------------------------------------------------------------
	// imported from MB 17/10/11
	// ------------------------------------------------------------------
	public bool FindStartElement( string sName )
	{
		sName = sName.ToUpper();
		while( Read() && !( IsNodeElement() && Name.ToUpper()==sName ) );
		if( !IsNodeElement() || Name.ToUpper()!=sName )
		{
			//Debug.Log( "Error in XML File, can't find expected " + sName );
			return false;
		}
		return true;
	}
	
	// ------------------------------------------------------------------
	// imported from MB 17/10/11
	// ------------------------------------------------------------------
	public float ReadFloatFromXml()
	{
		if( Read() )
			return float.Parse( GetValue() );
		else
			return 0f;
	}
	
	// ------------------------------------------------------------------
	// imported from MB 17/10/11
	// ------------------------------------------------------------------
	public Vector3 ReadVector3()
	{
		Vector3 vResult = Vector3.zero;
		if( FindStartElement( "X" ) ) vResult.x = ReadFloatFromXml();
		if( FindStartElement( "Y" ) ) vResult.y = ReadFloatFromXml();
		if( FindStartElement( "Z" ) ) vResult.z = ReadFloatFromXml();
		return vResult;
	}
	
	// ------------------------------------------------------------------
	// imported from MB 17/10/11
	// ------------------------------------------------------------------
	public Vector2 ReadVector2()
	{
		Vector2 vResult = Vector2.zero;
		if( FindStartElement( "X" ) ) vResult.x = ReadFloatFromXml();
		if( FindStartElement( "Y" ) ) vResult.y = ReadFloatFromXml();
		return vResult;
	}
}
