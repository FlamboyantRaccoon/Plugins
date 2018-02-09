using UnityEngine;
using System.Collections.Generic;

public class lwTextureMemory : MonoBehaviour
{
	public Rect m_rectTextGui = new Rect(5,5,800,800);
	public Rect m_rectButtonGui = new Rect(5,805,800,50);
	public GUISkin m_skin;

	private lwMemory<Texture2D> m_texMemory = null;
	
	void Awake()
	{
		lwMemory<Texture2D>.MAX_LINE_COUNT = 20;
		m_texMemory = new lwMemory<Texture2D>();
		m_texMemory.Init();
	}

	void Start()
	{
		if( m_texMemory!=null )
			m_texMemory.SnapMemory();
	}

	void OnDestroy()
	{
		if( m_texMemory!=null )
		{
			m_texMemory.Destroy();
			m_texMemory = null;
		}
	}

	//void Update()
	//{
	//	if( Input.GetKeyDown( KeyCode.M ) )
	//	{
	//		if( m_texMemory!=null )
	//			m_texMemory.SnapMemory();
	//	}
	//}

	void OnGUI()
	{
		if( m_skin!=null ) GUI.skin = m_skin;
		string sInfo = string.Empty;
		if( m_texMemory!=null )
		{
			sInfo = "Texture Memory (" + ConvertSizeToText( m_texMemory.nDataSize ) + ")";
			for( int i = 0; i < m_texMemory.nDataCount; ++i )
			{
				string sName = m_texMemory.dataArray[i].m_sName;
				sName = sName.Replace( "SpriteAtlasTexture-", "[ATLAS] " );
				if( sName.Length>30 ) sName = sName.Substring( 0, 30 );
				while( sName.Length<30 ) sName += " ";
				sInfo += "\n" + sName + ":\t" + ConvertSizeToText( m_texMemory.dataArray[i].m_nBytes );
			}
		}
		GUI.Label( m_rectTextGui, sInfo, GUI.skin.box );
		if( GUI.Button( m_rectButtonGui, "SNAP MEMORY" ) )
		{
			if( m_texMemory!=null )
				m_texMemory.SnapMemory();
		}
	}

	private string ConvertSizeToText( long nBytes )
	{
		float fMegas = (float)nBytes / (1024f * 1024f);
		return fMegas.ToString( "0.###" ) + "MB";
	}
}
