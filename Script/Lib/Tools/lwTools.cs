using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

public sealed class lwTools
{
#if !UNITY_FLASH || UNITY_EDITOR
	public static readonly System.DateTime UnixEpoch = new System.DateTime( 1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc );
#endif
	
	public static byte[] LoadBytesFromURL( string sFullPath )
	{
#if !UNITY_FLASH && !UNITY_WEBPLAYER
		WWW stream = new WWW( sFullPath );
		while( !stream.isDone );
		if( string.IsNullOrEmpty( stream.error ) ) return stream.bytes;
#endif
		return null;
	}
	
	public static string LoadTextFromURL( string sFullPath )
	{
#if !UNITY_FLASH && !UNITY_WEBPLAYER
		WWW stream = new WWW( sFullPath );
		while( !stream.isDone );
		if( string.IsNullOrEmpty( stream.error ) ) return stream.text;
#endif
		return null;
	}
	
	public static byte[] LoadBinaryFileAtFullPath( string sFullPath )
	{
#if UNITY_FLASH || UNITY_WEBPLAYER
		Debug.LogError( "Tools.LoadBinaryFile not implemented in Flash & WebPlayer" );
		return null;
#else
		if( File.Exists(sFullPath) )
		{
			return File.ReadAllBytes( sFullPath );
		}
#if UNITY_ANDROID && !UNITY_EDITOR
		else
		{
			return LoadBytesFromURL( sFullPath );
		}
#else
		return null;
#endif
#endif
	}

	public static string LoadTextFileAtFullPath( string sFullPath )
	{
		string sText = null;
#if UNITY_WEBPLAYER
		if( File.Exists( sFullPath ) )
		{
			StreamReader sr = File.OpenText( sFullPath );
			if( sr!=null ) sText = sr.ReadToEnd();
		}
#elif !UNITY_EDITOR && UNITY_ANDROID
		sText = LoadTextFromURL( sFullPath );
#elif UNITY_EDITOR || !UNITY_FLASH
		if( File.Exists( sFullPath ) )
		{
			sText = File.ReadAllText( sFullPath );
		}
#endif
		return sText;
	}

	public static string LoadTextFile( string sFileName, string sExtension = "", string sSubPath = "" )
	{
		if ( string.IsNullOrEmpty( sFileName ) )
			return null;

		string sText = null;

		// Test if the file is present in the local directory (for testing)
#if UNITY_EDITOR || ( !UNITY_WEBPLAYER && !UNITY_FLASH )
		string sCompleteFileName = Application.dataPath + "/" + sSubPath + sFileName + '.' + sExtension;
		sText = LoadTextFileAtFullPath( sCompleteFileName );
		if( sText==null )
#endif
		{
			TextAsset textFile = null;
			int nStart = sFileName.LastIndexOf( '.' );
			if( nStart>=0 ) 
				sFileName = sFileName.Substring( 0, nStart );
			textFile = Resources.Load( sFileName ) as TextAsset;
			
			if( textFile==null )
				Debug.LogWarning( "Can't open resource " + sFileName + " !" );
			else
				sText = textFile.text;
		}

#if !UNITY_FLASH
		if( !string.IsNullOrEmpty( sText ) && sText[0].GetHashCode()==0xFEFF )
		{
			sText = sText.Substring( 1 );
		}
#endif
		
		return sText;
	}

	public static string LoadAllTextFile( string sFolder, string sExtension )
	{
		if( sFolder == null || sExtension == null )
			return null;

		string sAllTxt = string.Empty;
		
#if !UNITY_WEBPLAYER && !UNITY_FLASH
		string sCompleteFileName = Application.dataPath 
#if !UNITY_ANDROID
			+ "/.."
#endif
			+ "/" + sFolder;

		if( Directory.Exists( sCompleteFileName ) )
		{
			string[] sAllFiles = Directory.GetFiles( sCompleteFileName, sExtension );
			if( sAllFiles.Length > 0 )
			{
				foreach( string sFile in sAllFiles )
				{
					sAllTxt = sAllTxt + File.ReadAllText( sFile ) + "\n";
				}
			}
		}
#endif		

		if( string.IsNullOrEmpty( sAllTxt ) )
		{
			UnityEngine.Object[] sAllObjects = Resources.LoadAll( sFolder.TrimEnd( '*', '/' ), typeof( TextAsset ) );
			foreach( Object ta in sAllObjects )
			{
				sAllTxt += ( ta as TextAsset ) + "\n";
			}
		}

		return sAllTxt;
	}
	
#if UNITY_EDITOR
	public static void SaveTextFile( string sFilePath, string sText )
	{
#if !UNITY_WEBPLAYER
		File.WriteAllText( sFilePath, sText );
#endif
	}
#endif
	
	public static string GetCompleteFileName( string sFileName, string sExtension )
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return "jar:file://" + Application.dataPath + "!/assets/" + sFileName + sExtension;
#elif UNITY_IPHONE && !UNITY_EDITOR
		return Application.dataPath + "/Raw/" + sFileName + sExtension;
#elif UNITY_XBOX360 && !UNITY_EDITOR
		return Application.dataPath + "\\Raw\\" + sFileName.Replace( "/", "\\" ) + sExtension;
#elif UNITY_PS3 && !UNITY_EDITOR
		return Application.dataPath + "Media/Raw/" + sFileName + sExtension;
#elif (UNITY_WEBPLAYER || UNITY_FLASH) && !UNITY_EDITOR
		return Application.dataPath + "/StreamingAssets/" + sFileName + sExtension;
#elif UNITY_EDITOR
		return Application.dataPath + "/../External/" + sFileName + sExtension;
#else
		return Application.streamingAssetsPath + '/' + sFileName + sExtension;
#endif
	}
	
	public static byte[] LoadBinaryFile( string sFileName, string sExtension )
	{
#if UNITY_FLASH || UNITY_WEBPLAYER
		Debug.LogError( "Tools.LoadBinaryFile not implemented in Flash & WebPlayer" );
#else
		string sCompleteFileName = GetCompleteFileName( sFileName, sExtension );
#if UNITY_ANDROID && !UNITY_EDITOR
		byte[] bytes = LoadBytesFromURL( sCompleteFileName );
		if( bytes!=null ) return bytes;
#else
		if( File.Exists( sCompleteFileName ) )
		{
			return File.ReadAllBytes( sCompleteFileName );
		}
		Debug.LogWarning( "Binary file not found : " + sCompleteFileName );
#endif
#endif
		return null;
	}
	
	public static bool ReadBool( string sValue, bool bDefault )
	{
		if( !string.IsNullOrEmpty( sValue ) )
		{
			switch( sValue.ToUpper() )
			{
			case "FALSE":
			case "OFF":
			case "NO":
			case "0":
				return false;
			case "TRUE":
			case "ON":
			case "YES":
			case "1":
			case "-1":
				return true;
			}
		}
		return bDefault;
	}

    public static void DestroyAllChildren( GameObject obj )
    {
        // clear children
        int nChildCount = obj.transform.childCount;
        for (int i = nChildCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(obj.transform.GetChild(i).gameObject);
        }
    }

    static public string GetXmlAttribute(XmlReader xr, string sAttribute, string sDefault)
    {
        try
        {
            string sResult = xr.GetAttribute(sAttribute);
            if (string.IsNullOrEmpty(sResult))
                return sDefault;
            else
                return sResult;
        }
        catch
        {
            return sDefault;
        }
    }

    static public string GetXmlAttribute(XmlNode node, string sAttribute, string sDefault)
    {
        try
        {
            string sResult = node.Attributes[sAttribute].InnerXml;
            if (string.IsNullOrEmpty(sResult))
                return sDefault;
            else
                return sResult;
        }
        catch
        {
            return sDefault;
        }
    }

    static public T GetXmlAttributeEnum<T>(XmlReader xr, string sAttribute, T nDefault)
    {
        return (T)System.Enum.Parse(typeof(T), GetXmlAttribute(xr, sAttribute, nDefault.ToString()));
    }

    static public int GetXmlAttributeInt(XmlReader xr, string sAttribute, int nDefault)
    {
        return int.Parse(GetXmlAttribute(xr, sAttribute, nDefault.ToString()));
    }
    static public int GetXmlAttributeInt(XmlNode node, string sAttribute, int nDefault)
    {
        return int.Parse(GetXmlAttribute(node, sAttribute, nDefault.ToString()));
    }

    static public float GetXmlAttributeFloat(XmlReader xr, string sAttribute, float fDefault)
    {
        return float.Parse(GetXmlAttribute(xr, sAttribute, fDefault.ToString()));
    }

    static public bool GetXmlAttributeBool(XmlReader xr, string sAttribute, bool bDefault)
    {
        return ReadBool(GetXmlAttribute(xr, sAttribute, bDefault.ToString()), bDefault);
    }

    static public Vector2 GetXmlAttributeVector2(XmlReader xr, Vector2 vDefault)
    {
        return new Vector2(GetXmlAttributeFloat(xr, "x", vDefault.x),
                            GetXmlAttributeFloat(xr, "y", vDefault.y));
    }
    static public Vector3 GetXmlAttributeVector3(XmlReader xr, Vector3 vDefault)
    {
        return new Vector3(GetXmlAttributeFloat(xr, "x", vDefault.x),
                            GetXmlAttributeFloat(xr, "y", vDefault.y),
                            GetXmlAttributeFloat(xr, "z", vDefault.z));
    }
    static public Vector3 GetXmlAttributeVectorScale3(XmlReader xr, Vector3 vDefault)
    {
        return new Vector3(GetXmlAttributeFloat(xr, "scalex", vDefault.x),
                            GetXmlAttributeFloat(xr, "scaley", vDefault.y),
                            GetXmlAttributeFloat(xr, "scalez", vDefault.z));
    }

    public static void SaveEncryptedTextFile(string sPath, string sContent)
    {
        if (sPath == null)
            return;

#if UNITY_FLASH || UNITY_WEBPLAYER
		Debug.LogError( "Tools.SaveEncryptedTextFile not implemented in Flash & WebPlayer" );
#else
        lwRC4 rc4 = new lwRC4();
        byte[] content = rc4.Encrypt(sContent);
        File.WriteAllBytes(sPath, content);
#endif
    }

    public static string LoadEncryptedTextFile(string sPath)
    {
        lwRC4 rc4 = new lwRC4();
        byte[] bBinary = LoadBinaryFile(sPath, ".enc");
        if (bBinary == null)
        {
            Debug.LogWarning("Unable to load " + sPath + ".enc");
            return null;
        }
        string sTxt = rc4.EncryptToString(bBinary);
#if !UNITY_FLASH
        if (!string.IsNullOrEmpty(sTxt) && sTxt[0].GetHashCode() == 0xFEFF)
        {
            sTxt = sTxt.Substring(1);
        }
#endif
        return sTxt;
    }

    public static string TimeToString( uint nTime, bool bTrimHour = false, bool bSeconds = true )
	{
		StringBuilder sbTime = new StringBuilder();
		uint nMin = nTime / 60;
		uint nSec = nTime % 60;
		uint nHours = nMin / 60;

		nMin -= 60 * nHours;
		if ( bTrimHour && nHours==0 )
		{
			if ( bSeconds )
				sbTime = sbTime.AppendFormat("{0:D}:{1:D2}", nMin, nSec);
			else
				sbTime = sbTime.AppendFormat("{0:D}", nMin);
		}
		else
		{
			if ( bSeconds )
				sbTime = sbTime.AppendFormat("{0:D}:{1:D2}:{2:D2}", nHours, nMin, nSec);
			else
				sbTime = sbTime.AppendFormat("{0:D}:{1:D2}", nHours, nMin);
		}
		return sbTime.ToString();
	}

#if UNITY_EDITOR || !UNITY_FLASH
	public static IEnumerable<string> GetFiles( string sPath, string sFilter=null, bool bFromResources=false )
	{
		Queue<string> queue = new Queue<string>();
		queue.Enqueue( sPath );
		while( queue.Count>0 )
		{
			sPath = queue.Dequeue();
			try
			{
				foreach( string sSubDir in Directory.GetDirectories( sPath ) )
				{
					queue.Enqueue( sSubDir );
				}
			}
#if UNITY_EDITOR
			catch( System.Exception ex ) { Debug.Log( ex.ToString() ); }
#else
			catch( System.Exception ) {}
#endif
			string[] sFiles = null;
			try
			{
				if( sFilter==null )
					sFiles = Directory.GetFiles( sPath );
				else
					sFiles = Directory.GetFiles( sPath, sFilter );
			}
#if UNITY_EDITOR
			catch( System.Exception ex ) { Debug.Log( ex.ToString() ); }
#else
			catch( System.Exception ) {}
#endif
			if( sFiles!=null )
			{
				for( int i=0; i<sFiles.Length; i++ )
				{
					string sFile = sFiles[i];
					if( bFromResources )
					{
						if( !sFile.EndsWith( ".meta" ) )
						{
							yield return( sFile.Replace( "\\", "/" ).Substring( sFile.IndexOf( "Resources" )+9 ) );
						}
					}
					else
					{
						yield return sFile;
					}
				}
			}
		}

	}
#endif



	public static void Resize<T>( ref T[] array, int nNewSize )
	{
#if !UNITY_FLASH
		System.Array.Resize( ref array, nNewSize );
#else
		array = ResizeArray( array, nNewSize );
#endif
	}
	
	public static T[] ResizeArray<T>( T[] array, int nNewSize )
	{
#if !UNITY_FLASH
		System.Array.Resize( ref array, nNewSize );
		return array;
#else
		if( nNewSize<0 )
		{
			Debug.LogWarning( "Array resize length is negative " );
			return array;
		}
		if( array==null )
		{
			return new T[nNewSize];
		}
		int nLen = array.Length;
		if( nLen!=nNewSize )
		{
			T[] array2 = new T[nNewSize];
			System.Array.Copy( array, 0, array2, 0, ( nLen>nNewSize ) ? nNewSize : nLen );
			return array2;
		}
		return array;
#endif
	}
	
#if UNITY_FLASH || (UNITY_WEBGL && !UNITY_EDITOR)
	public static void Assert( bool bToEvaluate, string sMessage )
	{
		if( !bToEvaluate )
		{
			Debug.LogError( "ASSERT: "+sMessage );
		}
	}
	
	public static void AssertFormat( bool bToEvaluate, string sMessage, params object[] arguments )
	{
		if( !bToEvaluate )
		{
			Debug.LogError( "ASSERT: "+string.Format( sMessage, arguments ) );
		}
	}

	public static void Assert( bool bToEvaluate )
	{
		if( !bToEvaluate )
		{
			Debug.LogError( "ASSERT" );
		}
	}
#else
	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void Assert( bool bToEvaluate, string sMessage )
	{
		if( !bToEvaluate )
		{
			throw new System.Exception( "ASSERT: "+sMessage );
		}
	}

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void AssertFormat( bool bToEvaluate, string sMessage, params object[] arguments )
	{
		if( !bToEvaluate )
		{
			throw new System.Exception( "ASSERT: "+string.Format( sMessage, arguments ) );
		}
	}
	
	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void Assert( bool bToEvaluate )
	{
		Assert( bToEvaluate, string.Empty );
	}
#endif
	
	public static void SetLayerRecursively( GameObject rootObject, int nLayer )
	{
		if( rootObject == null )
			return;

		rootObject.layer = nLayer;
		
		foreach( Transform childTransform in rootObject.transform )
			SetLayerRecursively( childTransform.gameObject, nLayer );
	}
	
	public static void SetLossyScale( Transform tr, Vector3 vScale )
	{
		Vector3 v1 = tr.lossyScale;
		if( Mathf.Approximately( v1.x, 0 ) || Mathf.Approximately( v1.y, 0 ) || Mathf.Approximately( v1.z, 0 ) )
		{
			tr.localScale = Vector3.zero;
		}
		else
		{
			float fX = tr.localScale.x * vScale.x / v1.x;
			float fY = tr.localScale.y * vScale.y / v1.y;
			float fZ = tr.localScale.z * vScale.z / v1.z;
			tr.localScale = new Vector3( fX, fY, fZ );
		}
	}
	
	/// <summary>
	/// Get real time in seconds (based on System.DateTime)
	/// </summary>
	public static uint GetTimeSinceUnixEpoch( bool bLocal=false )
	{
#if !UNITY_FLASH || UNITY_EDITOR
		System.DateTime dtNow = bLocal ? System.DateTime.Now : System.DateTime.UtcNow;
		return (uint)( dtNow - UnixEpoch ).TotalSeconds;
#else
		if( bLocal )
			return (uint)( UnityEngine.Flash.ActionScript.Expression<double>("new Date().time/1000-new Date().timezoneOffset*60") );
		else
			return (uint)( UnityEngine.Flash.ActionScript.Expression<double>("new Date().time/1000") );
#endif
	}
	
	/// <summary>
	/// Convert a time in seconds into days/hours/minutes/seconds
	/// </summary>
	public static void ConvertTime( uint nTimeInSeconds, out uint nDays, out uint nHours, out uint nMinutes, out uint nSeconds )
	{
		nDays = nTimeInSeconds / 86400;
		uint nHoursInSeconds = nTimeInSeconds - 86400*nDays;
		nHours = nHoursInSeconds / 3600;
		uint nMinutesInSeconds = nHoursInSeconds - 3600*nHours;
		nMinutes = nMinutesInSeconds / 60;
		nSeconds = nMinutesInSeconds - 60*nMinutes;
	}

	public static void EnableRendererRecursively( Transform trParent, bool bEnabled )
	{
		if ( trParent==null )
			return;

		Renderer rd = trParent.GetComponent<Renderer>();
		if( rd!=null ) rd.enabled = bEnabled;
		foreach( Transform trChild in trParent )
		{
			EnableRendererRecursively( trChild, bEnabled );
		}
	}

	// Get a component by its type, and add it when not found
	// Useful with Flash builds, because it doesn't automatically create required components
	public static T GetComponentSafe<T>( GameObject go ) where T : Component
	{
		if (go == null) return null;

		T cpt = go.GetComponent<T>();
		if( cpt==null ) cpt = go.AddComponent<T>();
		return cpt;
	}

	// Get the nearest power of two of a number
	public static int GetNearestPow2( int nNumber )
	{
		return Mathf.RoundToInt( Mathf.Pow( 2f, Mathf.Ceil( Mathf.Log( nNumber ) / Mathf.Log( 2f ) ) ) );
	}

	public static void SetParent( Transform child, Transform parent )
	{
		if( child==null || parent==null )
			return;
		child.SetParent( parent );
	}

	public static void SetParent( Transform child, Transform parent, Vector3 vPos, Vector3 vScale )
	{
		if (child == null || parent == null)
			return;
		SetParent( child, parent );
		child.localScale = vScale;
		child.localPosition = vPos;
	}

#if UNITY_5 || UNITY_2017_1_OR_NEWER
	public static void SetParent( RectTransform child, Transform parent, Vector2 vPos, Vector3 vScale )
	{
		if (child == null || parent == null)
			return;
		child.SetParent( parent );
		child.localScale = vScale;
		child.anchoredPosition = vPos;
	}

	public static void SetParent( RectTransform child, Transform parent, Vector3 vPos, Vector3 vScale )
	{
		if (child == null || parent == null)
			return;
		child.SetParent( parent );
		child.localScale = vScale;
		child.anchoredPosition3D = vPos;
	}

	public static void ReplaceGameObject( GameObject instance, Transform destination )
	{
		instance.transform.SetParent(destination.parent);
		instance.SetActive(destination.gameObject.activeSelf);
		instance.transform.localPosition = destination.localPosition;
		instance.transform.localScale = destination.localScale;
		instance.transform.localRotation = destination.localRotation;

		instance.transform.SetSiblingIndex(destination.GetSiblingIndex());

		RectTransform instanceRectTransform = instance.GetComponent<RectTransform>();
		RectTransform destinationRectTransform = destination.GetComponent<RectTransform>();
		if( instanceRectTransform!=null && destinationRectTransform!=null )
		{
			instanceRectTransform.anchoredPosition = destinationRectTransform.anchoredPosition;
			instanceRectTransform.sizeDelta = destinationRectTransform.sizeDelta;
			instanceRectTransform.anchorMin = destinationRectTransform.anchorMin;
			instanceRectTransform.anchorMax = destinationRectTransform.anchorMax;
			instanceRectTransform.pivot = destinationRectTransform.pivot;
		}

		GameObject.Destroy(destination.gameObject);
	}
#endif

#if UNITY_EDITOR
	public static string GetCommandLineArg( string sParam, string sDefault = "" )
	{
		sParam = "-" + sParam + "=";
		foreach ( string sArg in System.Environment.GetCommandLineArgs() )
		{
			if ( sArg.ToUpper().StartsWith( sParam ) )
			{
				return sArg.Substring( sParam.Length );
			}
		}
		return sDefault;
	}

	public static bool CommandLineArgExist( string sParam )
	{
		sParam = "-" + sParam;
		string[] commandLineArray = System.Environment.GetCommandLineArgs();
		for( int i = 0 ; i < commandLineArray.Length ; ++i )
		{
			if ( commandLineArray[i].ToUpper() == sParam )
				return true;
		}
		return false;
	}
#endif

	public static void EnableParticleSystemEmission( ParticleSystem ps, bool bEnable )
	{
		if( ps == null )
			return;

#if UNITY_5_3_OR_NEWER
		ParticleSystem.EmissionModule emission = ps.emission;
		emission.enabled = bEnable;
#else
		ps.enableEmission = bEnable;
#endif
	}

	/// <summary>
	/// Function to remove no graphic component of a gameobject and his children
	/// Usefull when you want to duplicate an object just to show it in other space (Tooltip or hud)
	/// </summary>
	/// <param name="obj">the gameObject you want to change, ref is syntaxic useless, it's just to indicate you will modify this object </param>
	public static void RemoveNoGraphicComponents( ref GameObject obj )
	{
		Component[] components = obj.GetComponentsInChildren<Component>();
		if( components!=null )
		{
			int nComponentIterator = components.Length-1;
			while( nComponentIterator >= 0 )
			{
				if( !(components[nComponentIterator] is UnityEngine.UI.Graphic) &&
					 !(components[nComponentIterator] is Transform) &&
					 !(components[nComponentIterator] is Canvas) &&
					 !(components[nComponentIterator] is CanvasRenderer))
				{
					GameObject.Destroy( components[nComponentIterator] );
				}
				nComponentIterator--;
			}
		}
	}

    public static Color ConvertHexaToColor(int hexaColor)
    {
        int r = (hexaColor >> 16) & 255;
        int g = (hexaColor >> 8) & 255;
        int b = (hexaColor >> 0) & 255;

        return new Color(((float)r) / 255f, ((float)g) / 255f, ((float)b) / 255f);
    }

    public static float ComputeAngleFromVector(Vector2 vNormalized)
    {
        float fAngle = Mathf.Acos(vNormalized.x) * Mathf.Rad2Deg;
        if ((fAngle < 180 && vNormalized.y < 0) || (fAngle > 180f && vNormalized.y > 0))
        {
            fAngle = 360f - fAngle;
        }
        return fAngle;
    }

    public static int ComputePGCD( int a, int b )
    {
        while( a*b != 0 )
        {
            if( a>b )
            {
                a = a - b;
            }
            else
            {
                b = b - a;
            }
        }
        return a == 0 ? b : a;
    }

    public static int ComputePPCM(int a, int b)
    {
        int nPGCD = ComputePGCD(a, b);
        return a * b / nPGCD;
    }

    public static int ComputePPCM(int[] array)
    {
        int nPPCM = ComputePPCM(array[0], array[1]);
        for( int i=2; i<array.Length; i++ )
        {
            nPPCM = ComputePPCM(nPPCM, array[i]);
        }
        return nPPCM;
    }
}
