#if !UNITY_IPHONE && !UNITY_ANDROID && !UNITY_WEBPLAYER && !UNITY_FLASH && !UNITY_XBOX360 && !UNITY_PS3
	// Defined for platforms having a local filesystem (Windows, Mac, Linux)
	#define HAS_LOCALFS
#endif

// Enable the following define only if you want to use System.Xml class,
// which is not available on Unity Flash builds
//#define SYSTEMXML_SERIALIZER

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
#if SYSTEMXML_SERIALIZER
using System.Xml;
using System.Xml.Serialization;
#endif

public sealed class lwGameSave
{
	private bool m_bUsePlayerPrefs = false;
#if !UNITY_FLASH && !UNITY_WEBPLAYER
	private bool m_bRc4Encoding = false;
#endif
	private bool m_bAutoSaveLoad = true;
	private bool m_bIsSaving = false;

	private Dictionary<string,string> m_savedData = null;
#if HAS_LOCALFS || UNITY_XBOX360 || UNITY_PS3
	private string m_sDistribName = ""; 
	private string m_sGameSeriesName = ""; 
#endif
#if UNITY_XBOX360 && !UNITY_EDITOR
	X360SaveGame m_X360sg = null;
	X360StorageOpResult m_nX360Result;
	byte[] m_pX360Buffer = null;
	bool m_bX360HasResult = false;
#endif
	
	public lwGameSave( string sDistribName, string sGameSeriesName )
	{
		Init( sDistribName, sGameSeriesName, !HasLocalStorage(), false, true );
	}
	
	public lwGameSave( string sDistribName, string sGameSeriesName, bool bUsePlayerPrefs )
	{
		Init( sDistribName, sGameSeriesName, bUsePlayerPrefs, false, true );
	}
	
	public lwGameSave( string sDistribName, string sGameSeriesName, bool bUsePlayerPrefs, bool bRc4Encoding )
	{
		Init( sDistribName, sGameSeriesName, bUsePlayerPrefs, bRc4Encoding, true );
	}

	public lwGameSave( string sDistribName, string sGameSeriesName, bool bUsePlayerPrefs, bool bRc4Encoding, bool bAutoSaveLoad )
	{
		Init( sDistribName, sGameSeriesName, bUsePlayerPrefs, bRc4Encoding, bAutoSaveLoad );
	}
	
	private void Init( string sDistribName, string sGameSeriesName, bool bUsePlayerPrefs, bool bRc4Encoding, bool bAutoSaveLoad )
	{
#if UNITY_IPHONE
		System.Environment.SetEnvironmentVariable( "MONO_REFLECTION_SERIALIZER", "yes" );
#endif

#if !UNITY_XBOX360 && !UNITY_PS3
		m_bUsePlayerPrefs = bUsePlayerPrefs;
#endif
#if !UNITY_FLASH && !UNITY_WEBPLAYER
		m_bRc4Encoding = bRc4Encoding;
#endif
		m_bAutoSaveLoad = bAutoSaveLoad;
		
		if( !m_bUsePlayerPrefs )
		{
#if HAS_LOCALFS || UNITY_XBOX360 || UNITY_PS3
			m_sDistribName = sDistribName;
			m_sGameSeriesName = sGameSeriesName;
#endif
			if( m_savedData==null )
			{
				m_savedData = new Dictionary<string, string>();
				CreateFolder();
				if( m_bAutoSaveLoad ) Load();
			}
		}
	}

	public void Destroy()
	{
#if UNITY_XBOX360
		X360Storage.Unmount( GetFileName() );
#endif
	}

	public bool HasKey( string sKey )
	{
		if( m_bUsePlayerPrefs )
			return PlayerPrefs.HasKey( sKey );
		else
			return ( m_savedData!=null && m_savedData.ContainsKey( sKey ) );
	}
	
	public bool RemoveKey( string sKey )
	{
		if( HasKey( sKey ) )
		{
			if( m_bUsePlayerPrefs )
				PlayerPrefs.DeleteKey( sKey );
			else
				return m_savedData.Remove( sKey );
			return true;
		}
		return false;
	}
	
	
	public int GetInt( string sKey, int nDefault )
	{
		if( m_bUsePlayerPrefs )
		{
			return PlayerPrefs.GetInt( sKey, nDefault );
		}
		else
		{
			int nValue = nDefault;
			if( m_savedData!=null )
			{
				string sValue;
				m_savedData.TryGetValue( sKey, out sValue );
				if( !string.IsNullOrEmpty(sValue) )
				{
					nValue = lwParseTools.ParseIntSafe( sValue );
				}
			}
			return nValue;
		}
	}
	
	public void SetBool( string sKey, bool bValue )
	{
		SetInt( sKey, bValue ? 1 : 0 );
	}

	public bool GetBool( string sKey, bool bDefault )
	{
		return ( GetInt( sKey, bDefault ? 1 : 0 )!=0 );
	}

	public void SetBooleans( string sKey, params bool[] bValues )
	{
		StringBuilder sSerializedString = new StringBuilder( bValues.Length );
		for( int nIndex = 0; nIndex<bValues.Length; ++nIndex )
		{
			sSerializedString.Append( bValues[nIndex] ? 1 : 0 );
		}
		SetString( sKey, sSerializedString.ToString() );
	}

	public bool[] GetBooleans( string sKey, bool[] bDefaultValues )
	{
		StringBuilder sDefaultSerializedString = new StringBuilder(bDefaultValues.Length);
		for( int nIndex = 0; nIndex<bDefaultValues.Length; ++nIndex )
		{
			sDefaultSerializedString.Append( bDefaultValues[nIndex] ? 1 : 0 );
		}

		string sSerializedString = GetString( sKey, sDefaultSerializedString.ToString() );
		bool[] bValues = new bool[sSerializedString.Length];
		for( int nCharIndex = 0; nCharIndex<sSerializedString.Length; ++nCharIndex )
		{
			bValues[nCharIndex] = ( sSerializedString[nCharIndex]!='0' );
		}

		return bValues;
	}
	
	public void SetInt( string sKey, int nValue )
	{
		if( m_bUsePlayerPrefs )
		{
			PlayerPrefs.SetInt( sKey, nValue );
		}
		else
		{
			UpdateKeyValue( sKey, nValue.ToString() );
		}
	}
	
	public float GetFloat( string sKey, float fDefault )
	{
		if( m_bUsePlayerPrefs )
		{
			return PlayerPrefs.GetFloat( sKey, fDefault );
		}
		else
		{
			float fValue = fDefault;
			if( m_savedData!=null )
			{
				string sValue;
				m_savedData.TryGetValue( sKey, out sValue );
				if( sValue!=null && sValue!=string.Empty )
				{
					fValue = lwParseTools.ParseFloatSafe( sValue );
				}
			}
			return fValue;
		}
	}
	
	public void SetFloat( string sKey, float fValue )
	{
		if( m_bUsePlayerPrefs )
		{
			PlayerPrefs.SetFloat( sKey, fValue );
		}
		else
		{
			UpdateKeyValue( sKey, fValue.ToString() );
		}
	}

	public string GetString( string sKey, string sDefault )
	{
		if( m_bUsePlayerPrefs )
		{
			return PlayerPrefs.GetString( sKey, sDefault );
		}
		else
		{
			string sValue = sDefault;
			if( m_savedData!=null )
			{
				m_savedData.TryGetValue( sKey, out sValue );
				if( sValue==null )
				{
					sValue = sDefault;
				}
			}
			return sValue;
		}
	}
	
	public void SetString( string sKey, string sValue )
	{
		if( m_bUsePlayerPrefs )
		{
			PlayerPrefs.SetString( sKey, sValue );
		}
		else
		{
			UpdateKeyValue( sKey, sValue );
		}
	}

	private void UpdateKeyValue( string sKey, string sValue )
	{
		if( m_savedData!=null )
		{
			m_savedData[sKey] = sValue;
		}
		if( m_bAutoSaveLoad ) Save();
	}
	
	private string GetFileName()
	{
		return GetFileName( false );
	}
	
	private string GetFileName( bool bSerialized )
	{
#if UNITY_XBOX360 || UNITY_PS3
		return m_sDistribName + "_" + m_sGameSeriesName;
#else
#if HAS_LOCALFS
		string sAppData = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
		sAppData += "/" + m_sDistribName + "/" + m_sGameSeriesName; 
#else
		string sAppData = Application.persistentDataPath;
#endif
		
		if( m_bUsePlayerPrefs ) return "";
		
		sAppData += "/SavedData" 
			+ ( bSerialized ? "_bin" : "" );

		return sAppData;
#endif
	}

	private void CreateFolder()
	{
#if UNITY_XBOX360 && !UNITY_EDITOR
		if( m_X360sg==null )
		{
			//Debug.Log( "Retrieving save game..." );
			m_bX360HasResult = false;
			m_X360sg = X360Storage.GetSaveGameDesc( 0, 0 );
			if( m_X360sg==null )
			{
				//Debug.Log( "Creating save game..." );
				if( X360Storage.CreateSaveGame( 0, m_sGameSeriesName, "SaveData", GetFileName(), OnStorageResult ) )
					while( !m_bX360HasResult );
				else
					Debug.LogWarning( "Can't create save game !" );
				m_X360sg = X360Storage.GetSaveGameDesc( 0, 0 );
			}
			if( m_X360sg!=null )
			{
				//Debug.Log( "Opening save game..." );
				m_X360sg.Open( GetFileName(), OnStorageResult );
				while( !m_bX360HasResult );
			}
		}
#elif UNITY_PS3 && !UNITY_EDITOR
		PS3SaveDataUtility.Init();
		PS3SaveDataUtility.SetSaveParams( 0, "Icon.png", m_sGameSeriesName, GetFileName(), "Description" );
		PS3SaveDataUtility.SetLoadParams( 0 );
#else
		if( m_bUsePlayerPrefs ) return;
#if HAS_LOCALFS
		string sAppData = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
		sAppData += "/" + m_sDistribName;
		if( !Directory.Exists( sAppData ) ) Directory.CreateDirectory( sAppData );
		sAppData += "/" + m_sGameSeriesName;
		if( !Directory.Exists( sAppData ) ) Directory.CreateDirectory( sAppData );
#endif
#endif
	}
	
	public bool Save()
	{
		if( m_bIsSaving ) return false;
		m_bIsSaving = true;
		bool bSuccess = Save_Internal();
		m_bIsSaving = false;
		return bSuccess;
	}
	
	private bool Save_Internal()
	{
		if( m_bUsePlayerPrefs )
		{
#if UNITY_5
			PlayerPrefs.Save();
#endif
			return true;
		}
#if !UNITY_FLASH && !UNITY_WEBPLAYER
		if( m_savedData!=null )
		{
			StringWriter sw = new StringWriter();
			foreach( KeyValuePair<string,string> entry in m_savedData )
			{
				sw.WriteLine( entry.Key + "=" + entry.Value );
			}
			sw.Close();
			
			string sFileName = GetFileName();
			string sContent = sw.ToString();
#if UNITY_XBOX360 && !UNITY_EDITOR
			byte[] pBuffer = null;
			if( m_bRc4Encoding )
			{
				lwRC4 rc4 = new lwRC4();
				pBuffer = rc4.Encrypt( sContent );
			}
			else
			{
				pBuffer = System.Text.Encoding.ASCII.GetBytes( sContent );
			}
			if( m_X360sg==null ) return false;
			m_bX360HasResult = false;
			X360Storage.WriteBuffer( sFileName, pBuffer, OnStorageResult );
			while( !m_bX360HasResult );
			return ( m_nX360Result==X360StorageOpResult.Success );
#elif UNITY_PS3&& !UNITY_EDITOR
			while( !PS3SaveDataUtility.HasCompleted() );
			PS3SaveDataUtility.DoAutoSave( "APPDATA.DAT", sContent );
			while( !PS3SaveDataUtility.HasCompleted() );
			return ( PS3SaveDataUtility.GetResult()==0 );
#else
			if( m_bRc4Encoding )
				lwTools.SaveEncryptedTextFile( sFileName + ".enc" , sContent );
			else
				File.WriteAllText( sFileName + ".sav", sContent );
				
			return true;
#endif
		}
#endif
		return false;
	}
	
	public bool Load()
	{
		if( m_bUsePlayerPrefs ) return true;
#if !UNITY_FLASH && !UNITY_WEBPLAYER
		if( m_savedData!=null )
		{
			string sFileName = GetFileName();
			string sContent = "";

#if UNITY_XBOX360 && !UNITY_EDITOR
			if( m_X360sg==null ) return false;
			m_bX360HasResult = false;
			X360Storage.ReadBuffer( sFileName, OnStorageResult );
			while( !m_bX360HasResult );
			if( m_nX360Result!=X360StorageOpResult.Success )
			{
				Debug.LogWarning( "Unable to load saved data !" );
				return false;
			}
			if( m_bRc4Encoding )
			{
				lwRC4 rc4 = new lwRC4();
				sContent = rc4.EncryptToString( m_pX360Buffer );
			}
			else
			{
				sContent = System.Text.Encoding.ASCII.GetString( m_pX360Buffer );
			}
#elif UNITY_PS3 && !UNITY_EDITOR
			while( !PS3SaveDataUtility.HasCompleted() );
			PS3SaveDataUtility.DoAutoLoad( "APPDATA.DAT" );
			while( !PS3SaveDataUtility.HasCompleted() );
			if( PS3SaveDataUtility.GetResult()==0 )
			{
				sContent = PS3SaveDataUtility.GetData();
			}
			else
			{
				Debug.LogWarning( "Unable to load saved data !" );
				return false;
			}
#else
			if( m_bRc4Encoding )
				sFileName += ".enc";
			else
				sFileName += ".sav";
			
			if( !File.Exists( sFileName ) )
			{
				if( m_bAutoSaveLoad )
					Save();
				else
					return false;
			}
			
			if( m_bRc4Encoding )
			{
				byte[] bBinary = File.ReadAllBytes( sFileName );
				if( bBinary==null )
				{
					Debug.LogWarning( "Unable to load " + sFileName );
					return false;
				}
				else
				{
					lwRC4 rc4 = new lwRC4();
					sContent = rc4.EncryptToString( bBinary );
				}
			}
			else
			{
				sContent = File.ReadAllText( sFileName );
			}
#endif
			
			StringReader sr = new StringReader( sContent );
			if( sr!=null )
			{
				string sLine = "-";
				while( ( sLine=sr.ReadLine() )!=null )
				{
					int nOffset = sLine.IndexOf( "=" );
					if( nOffset>0 )
					{
						string sKey = sLine.Substring( 0, nOffset );
						string sValue = sLine.Substring( nOffset+1 );
						m_savedData[sKey] = sValue;
					}
				}
				sr.Close();
				Debug.Log( "Saved data loaded from " + sFileName );
			}
			
			return true;
		}
#endif
		return false;
	}
	
	private string GetPrefix()
	{
		return "slot" + GetCurrentSlotID().ToString();
	}
	
	public int GetIntValueWithPrefix( string sKey, int nDefault )
	{
		if( HaveCurrentSlotID() )
		{
			return GetInt( GetPrefix() + sKey, nDefault );
		}
		return nDefault;
	}

	public void SetIntValueWithPrefix( string sKey, int nValue )
	{
		if( HaveCurrentSlotID() )
		{
			SetInt( GetPrefix() + sKey, nValue );
		}
	}

	public float GetFloatValueWithPrefix( string sKey, float fDefault )
	{
		if( HaveCurrentSlotID() )
		{
			return GetFloat( GetPrefix() + sKey, fDefault );
		}
		return fDefault;
	}

	public void SetFloatValueWithPrefix( string sKey, float fValue )
	{
		if( HaveCurrentSlotID() )
		{
			SetFloat( GetPrefix() + sKey, fValue );
		}
	}
	
	public bool HaveCurrentSlotID()
	{
		return HasKey( "currentSlotID" );
	}
	
	public int GetCurrentSlotID()
	{
		return GetInt( "currentSlotID", 0 );
	}

	public void SetCurrentSlotID( int nID)
	{
		SetInt( "currentSlotID", nID );
	}
	
	public string GetPlayerName( int nId )
	{
		string sName = string.Empty;
		string sKey = "PlayerName" + nId.ToString();
		if( nId>=0 && nId<=3 && HasKey( sKey ) )
		{
			sName = GetString( sKey, string.Empty );
		}
		return sName;
	}
	
	public void SetPlayerName( int nId, string sName )
	{
		if( nId>=0 && nId<=3 )
		{
			string sKey = "PlayerName" + nId.ToString();
			SetString( sKey, sName );
		}
	}
	
	/// <summary>
	/// To save complex object, the object MUST be xml serializable
	/// </summary>
	public void SetSerialisableObject( string sKey, object obj, Type type )
	{
#if SYSTEMXML_SERIALIZER
		XmlSerializer xmlSerializer = new XmlSerializer( type );
		StringWriter stringWriter = new StringWriter();
		xmlSerializer.Serialize( stringWriter, obj );
		string serializedXML = stringWriter.ToString();
		serializedXML = serializedXML.Replace( "\n", "" );
		serializedXML = serializedXML.Replace( "\r", "" );
		serializedXML = serializedXML.Replace( "\t", "" );
		SetString( GetPrefix() + sKey, serializedXML );
#else
		Debug.LogError( "Not supported" );
#endif
	}
	
	/// <summary>
	/// To load complex object, the object MUST be xml serializable
	/// </summary>
	public object GetSerializedObject( string sKey, Type type )
	{
#if SYSTEMXML_SERIALIZER
		string sValue = GetString( GetPrefix() + sKey, "" );
		if( sValue!="" )
		{
			XmlSerializer xmlSerializer = new XmlSerializer( type );
			StringReader stringReader = new StringReader( sValue );
			return xmlSerializer.Deserialize( stringReader );
		}
#else
		Debug.LogError( "Not supported" );
#endif
		return null;
	}
	
#if SYSTEMXML_SERIALIZER
	public bool Save( ISerializable data ) // Overloaded
	{
		if( m_bIsSaving ) return false;
		m_bIsSaving = true;
		
#if !UNITY_WEBPLAYER
		string sFileName = GetFileName( true );
		if( !File.Exists( sFileName ) )	CreateFolder();
#endif
		
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WEBPLAYER
		string sXml = SaveIntoXML( data );

#if UNITY_WEBPLAYER
		PlayerPrefs.SetString( "SaveData", sXml );
		PlayerPrefs.Save();
#else
		File.WriteAllText( sFileName, sXml );
#endif
		//Debug.Log( "Saved : " + sXml );

#else
		Stream stream = File.Open( sFileName, FileMode.Create );
		BinaryFormatter bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder(); 
		bformatter.Serialize( stream, data );
		stream.Close();
#endif
		m_bIsSaving = false;
		return true;
 	}

	public static string SaveIntoXML( ISerializable data )
	{
		MemoryStream memoryStream = new MemoryStream();
		XmlSerializer xs = new XmlSerializer( data.GetType() );
		XmlTextWriter xmlTextWriter = new XmlTextWriter( memoryStream, Encoding.UTF8 );
		xs.Serialize( xmlTextWriter, data );
		memoryStream = (MemoryStream) xmlTextWriter.BaseStream;
		return UTF8ByteArrayToString( memoryStream.ToArray() );
	}

	// Call this to load from a file into "data"
	public object Load( Type type ) // Overloaded
	{
		object data = null;
#if !UNITY_WEBPLAYER
		string sFileName = GetFileName( true );
#endif

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WEBPLAYER
		string sXml = null;

#if UNITY_WEBPLAYER
		sXml = PlayerPrefs.GetString( "SaveData", "" );
#else
		if( File.Exists( sFileName ) )
			sXml = File.ReadAllText( sFileName );
		else
			File.Create( sFileName );
#endif
//		Debug.Log( "Loaded : "+sXml );
		data = LoadFromXML( type, sXml );
#else
		data = Activator.CreateInstance( type );
//		Debug.Log( "Save data Loaded from "+sFileName );
		if( File.Exists( sFileName ) )
		{
			Stream stream = File.Open( sFileName, FileMode.Open );
			BinaryFormatter bformatter = new BinaryFormatter();
			bformatter.Binder = new VersionDeserializationBinder(); 
			data = bformatter.Deserialize( stream );
			stream.Close();
		}
#endif
		return data;
	}

	public static object LoadFromXML( Type type, string sXml )
	{
		object data = null;
		if( !string.IsNullOrEmpty( sXml ) )
		{
			XmlSerializer xs = new XmlSerializer( type );
			MemoryStream memoryStream = new MemoryStream( StringToUTF8ByteArray( sXml ) );
			//Debug.Log( "( memoryStream != null ) : " + ( memoryStream != null ) );
			
			try
			{
				data = xs.Deserialize( memoryStream );
			}
			catch( XmlException )
			{
				Debug.LogWarning( "XmlException : New player save data" );
				data = Activator.CreateInstance( type );
			}
			//data.DebugPrintSaveData();
		}
		else
		{
			Debug.Log( "New player save data" );
			data = Activator.CreateInstance( type );
		}
		
		if( data == null )
		{
			Debug.LogError( "SaveData loading failed, creating new one" );
			data = Activator.CreateInstance( type );
		}

		return data;
	}
#else
	public void Save( object data ) // Overloaded
	{
	}
	
	public object Load( object type ) // Overloaded
	{
		return null;
	}
#endif

	static public bool HasLocalStorage()
	{
#if HAS_LOCALFS
		return true;
#else
		return false;
#endif
	}
	
#if UNITY_XBOX360 && !UNITY_EDITOR
	private void OnStorageResult( X360StorageOpResult nResult, uint nErrorCode, byte[] pBuffer )
	{
		//Debug.Log( "OnStorageResult=" + nResult );
		m_nX360Result = nResult;
		m_pX360Buffer = pBuffer;
		m_bX360HasResult = true;
	}
#endif
	
	public static string UTF8ByteArrayToString( byte[] characters )
	{
		UTF8Encoding encoding = new UTF8Encoding();
		return encoding.GetString( characters );
	}

	public static byte[] StringToUTF8ByteArray( string pXmlString )
	{
		UTF8Encoding encoding = new UTF8Encoding();
		return encoding.GetBytes( pXmlString );
	}
}

#if UNITY_EDITOR || !UNITY_FLASH
// === This is required to guarantee a fixed serialization assembly name, which Unity likes to randomize on each compile
// Do not change this
public sealed class VersionDeserializationBinder : SerializationBinder
{
	public override Type BindToType( string sAssemblyName, string sTypeName )
	{
		if( !string.IsNullOrEmpty( sAssemblyName ) && !string.IsNullOrEmpty( sTypeName ) )
		{
			sAssemblyName = Assembly.GetExecutingAssembly().FullName;
			return Type.GetType( String.Format( "{0}, {1}", sTypeName, sAssemblyName ) );
		}
		return null;
	}
}
#endif
