// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2017/03/22

using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Manages the language available in a game
/// Load and parse text files to register all keys and their localized text for each language.
/// /!\ Text files must be retrieved from resource paths.
/// /!\ Text files are csv formated list where the first column is the localization key and the second column is :
/// 	- the localized text for texts (the key must start with 'str_')
/// 	- the name of asset array and the path of the asset in the asset array separated by a '|' (the key must not start with 'str_')
/// </summary>
public class lwLanguageManager : lwSingleton<lwLanguageManager>
{
	public delegate void OnLanguageChanged();
	public OnLanguageChanged m_onLanguageChangedEvent;

	public enum TimeUnitTextId
	{
		Days,
		Day,
		Hours,
		Hour,
		Minutes,
		Minute,
		Seconds,
		Second,
	}
	public static readonly int timeUnitTextIdCount = System.Enum.GetValues( typeof( TimeUnitTextId ) ).Length;

	/// <summary>
	/// Gets the index of the current language among all available languages.
	/// </summary>
	/// <value>The index of the current language.</value>
	public int nCurrentLanguageIndex
	{
		get
		{
			lwTools.Assert( m_nLanguageIndex>=0 && m_nLanguageIndex<m_availableLanguages.Length );
			return m_nLanguageIndex;
		}
	}

	/// <summary>
	/// Gets the current language.
	/// </summary>
	/// <value>The current language.</value>
	public lwCountry currentLanguage
	{
		get
		{
			lwTools.Assert( m_nLanguageIndex>=0 && m_nLanguageIndex<m_availableLanguages.Length );
			return m_availableLanguages[m_nLanguageIndex];
		}
	}

	/// <summary>
	/// Gets the language at specified index.
	/// </summary>
	/// <param name="nLanguageIndex">Index.</param>
	public lwCountry this[int nLanguageIndex]
	{
		get
		{
			lwTools.Assert( nLanguageIndex>=0 && nLanguageIndex<m_availableLanguages.Length );
			return m_availableLanguages[nLanguageIndex];
		}
	}

	/// <summary>
	/// Gets the amount of available languages
	/// </summary>
	/// <value>The language count.</value>
	public int nLanguageCount
	{
		get{ return m_availableLanguages.Length; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="lwLanguageManager"/> class.
	/// </summary>
	public lwLanguageManager()
	{
        m_availableLanguages = new lwCountry[0];
		m_sTextFilesPerLanguage = new HashSet<AssetHolderKey>[0];

		TextAsset languageListAsset = Resources.Load<TextAsset>( "Texts/Langs" );
		if( languageListAsset==null )
		{
			Debug.LogError( "Language Manager : the file 'Langs' has not been found in any 'Resources/Texts/' folder !" );
		}
		else
		{
			string[] sLanguageCodes = languageListAsset.text.Split( new char[]{ '\n' }, System.StringSplitOptions.RemoveEmptyEntries );
			List<lwCountry> availableLanguages = new List<lwCountry>();
			for( int nLanguageCodeIndex = 0; nLanguageCodeIndex<sLanguageCodes.Length; ++nLanguageCodeIndex )
			{
				lwCountry countryStructure = lwCountryCode.GetlwCountry( sLanguageCodes[nLanguageCodeIndex].Replace("\r", ""));
				if( countryStructure!= null )
				{
					availableLanguages.Add( countryStructure );
                }
			}

			m_availableLanguages = availableLanguages.ToArray();
			m_sTextFilesPerLanguage = new HashSet<AssetHolderKey>[m_availableLanguages.Length];
			for( int nLanguageIndex = 0; nLanguageIndex<m_sTextFilesPerLanguage.Length; ++nLanguageIndex )
			{
				m_sTextFilesPerLanguage[nLanguageIndex] = new HashSet<AssetHolderKey>();
			}
		}

		m_localizedTexts = new Dictionary<string, string>();
		m_localizedResources = new Dictionary<string, AssetHolderKey>();
	}

	/// <summary>
	/// Adds a text file to a language.
	/// </summary>
	/// <param name="sLanguageCode">Language code.</param>
	/// <param name="sAssetArrayName">Name of the asset array in the AssetHolder.</param>
	/// <param name="sFileResourcePath">Resource path of the file.</param>
	public void AddTextFile( string sLanguageCode, string sAssetArrayName, string sFileResourcePath = "" )
	{
		lwTools.Assert( String.IsNullOrEmpty( sLanguageCode )==false );
		lwTools.Assert( String.IsNullOrEmpty( sAssetArrayName )==false );

        int nLanguageIndex = System.Array.FindIndex( m_availableLanguages, item => item.m_sLanguageCulture==sLanguageCode );
		if( nLanguageIndex>=0 && nLanguageIndex<m_availableLanguages.Length )
		{
			AssetHolderKey assetHolderKey = new AssetHolderKey( sAssetArrayName, sFileResourcePath );
            if(!m_sTextFilesPerLanguage[nLanguageIndex].Contains(assetHolderKey) )
            {
                m_sTextFilesPerLanguage[nLanguageIndex].Add(assetHolderKey);

                // if the language is the current one, load the file immediately
                if (nLanguageIndex == m_nLanguageIndex)
                {
                    List<string> sFilesContent = new List<string>();
                    FillCsvContents(ref sFilesContent, assetHolderKey);

                    for (int nFileContentIndex = 0; nFileContentIndex < sFilesContent.Count; ++nFileContentIndex)
                    {
                        FillDictionaryWithCsvContent(sFilesContent[nFileContentIndex]);
                    }
                }
            }
		}
		else
		{
			Debug.LogErrorFormat( "Language Manager : language code '{0}' is not an available language code.\nThis error happened while registering file at path '{1}'.", sLanguageCode, sFileResourcePath );
		}
	}

	/// <summary>
	/// Sets the default language.
	/// </summary>
	public void SetDefaultLanguage()
	{
		lwTools.Assert( m_availableLanguages.Length>0, "Language manager : no available language registered." );

		// Look for system language and if the language is not available, switch to the first english language.
		// If there is no english language, switch to the first available language.

		int nSystemLanguageIndex = -1;
		int nEnglishLanguageIndex = -1;

		int nLanguageIndex = 0;
		while( nLanguageIndex<m_availableLanguages.Length && nSystemLanguageIndex<0 )
		{
			if(IsSystemCountry(m_availableLanguages[nLanguageIndex]) )
			{
				nSystemLanguageIndex = nLanguageIndex;
			}
			else
			{
				if( m_availableLanguages[nLanguageIndex].m_eLang==SystemLanguage.English && nEnglishLanguageIndex<0 )
				{
					nEnglishLanguageIndex = nLanguageIndex;
				}
				++nLanguageIndex;
			}
		}

		if( nSystemLanguageIndex>=0 )
		{
			SetLanguage( nSystemLanguageIndex );
		}
		else if( nEnglishLanguageIndex>= 0 )
		{
			SetLanguage( nEnglishLanguageIndex );
		}
		else
		{
			SetLanguage( 0 );
		}
	}

    public bool IsSystemCountry( lwCountry country )
    {
        if( country.m_eLang != Application.systemLanguage )
        {
            return false;
        }

        if( country.m_eLang!= SystemLanguage.English)
        {
            return true;
        }

        try
        {
            System.Globalization.RegionInfo regionInfo = System.Globalization.RegionInfo.CurrentRegion;
            if (regionInfo != null )
            {
                if(regionInfo.TwoLetterISORegionName.ToUpper() == "GB" )
                {
                    return country.m_sLanguageCulture == "en-UK";
                }
                return country.m_sLanguageCulture == "en-US";
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("System.Globalization doesn't work");
            return true;
        }
    }

	/// <summary>
	/// Sets the language.
	/// </summary>
	/// <param name="sLanguageCode">Code of the language to set.</param>
	public void SetLanguage( string sLanguageCode )
	{
		int nLanguageIndex = System.Array.FindIndex( m_availableLanguages, item => item.m_sLanguageCulture==sLanguageCode );
		lwTools.AssertFormat( nLanguageIndex>=0 && nLanguageIndex<m_availableLanguages.Length, "Language '{0}' not found in available languages.", sLanguageCode );
		SetAndLoadLanguage( nLanguageIndex );
	}

	/// <summary>
	/// Sets the language.
	/// </summary>
	/// <param name="systemLanguage">System language.</param>
	public void SetLanguage( SystemLanguage systemLanguage )
	{
		int nLanguageIndex = System.Array.FindIndex( m_availableLanguages, item => item.m_eLang==systemLanguage );
		lwTools.AssertFormat( nLanguageIndex>=0 && nLanguageIndex<m_availableLanguages.Length, "Language '{0}' not found in available languages.", systemLanguage );
		SetAndLoadLanguage( nLanguageIndex );
	}

	/// <summary>
	/// Sets the language.
	/// </summary>
	/// <param name="nLanguageIndex">Index of the language to set among all available languages.</param>
	public void SetLanguage( int nLanguageIndex )
	{
		if( nLanguageIndex==-1 )
		{
			SetDefaultLanguage();
		}
		else
		{
			SetAndLoadLanguage( nLanguageIndex );
		}
	}

	/// <summary>
	/// Sets the language to next available.
	/// </summary>
	public void SetLanguageToNextAvailable()
	{
		lwTools.Assert( m_nLanguageIndex>=0 && m_nLanguageIndex<m_availableLanguages.Length, "Language manager does not have a current language." );
		int nLanguageIndex = ( m_nLanguageIndex+1 )%m_availableLanguages.Length;
		SetAndLoadLanguage( nLanguageIndex );
	}

	/// <summary>
	/// Sets the language to previous available.
	/// </summary>
	public void SetLanguageToPreviousAvailable()
	{
		lwTools.Assert( m_nLanguageIndex>=0 && m_nLanguageIndex<m_availableLanguages.Length, "Language manager does not have a current language." );
		int nLanguageIndex = ( m_nLanguageIndex+m_availableLanguages.Length-1 )%m_availableLanguages.Length;
		SetAndLoadLanguage( nLanguageIndex );
	}

	/// <summary>
	/// Determines whether this instance has the specified key.
	/// </summary>
	/// <param name="sKey">Key.</param>
	public bool HasStringKey( string sKey )
	{
		lwTools.Assert( m_nLanguageIndex>=0 && m_nLanguageIndex<m_availableLanguages.Length, "Language manager does not have a current language." );
		return m_localizedTexts.ContainsKey( sKey );
	}

	/// <summary>
	/// Gets the localized text associated with the key given.
	/// </summary>
	/// <param name="sKey">Localization key.</param>
	public string GetString( string sKey )
	{
		lwTools.Assert( m_nLanguageIndex>=0 && m_nLanguageIndex<m_availableLanguages.Length, "Language manager does not have a current language." );

		string sValue;
		if( m_localizedTexts.TryGetValue( sKey, out sValue ) )
		{
			return sValue;
		}
		else
		{
			Debug.LogWarningFormat( "Language manager : key '{0}' not found for language '{1}'.", sKey, m_availableLanguages[m_nLanguageIndex].m_sLanguageCulture );
			return string.Empty;
		}
	}

	/// <summary>
	/// Determines whether this instance has the specified sKey.
	/// </summary>
	/// <param name="sKey">Key.</param>
	public bool HasResourceKey( string sKey )
	{
		lwTools.Assert( m_nLanguageIndex>=0 && m_nLanguageIndex<m_availableLanguages.Length, "Language manager does not have a current language." );
		return m_localizedResources.ContainsKey( sKey );
	}

	
	/// <summary>
	/// Gets the localized text associated to the given time.
	/// </summary>
	/// <param name="nTimeInSeconds">Time in seconds.</param>
	/// <param name="sTimeUnitKeys">Array of keys for each time unit defined by the enumeration TimeUnit.</param>
	/// <param name="sSeparator">Separator to use between each value.</param>
	/// <param name="sNumbersFont">Font used for numbers</param>
	public string GetTime( uint nTimeInSeconds, string[] sTimeUnitKeys, string sSeparator = " ", string sNumbersFont = null )
	{
		lwTools.Assert( sTimeUnitKeys!=null );
		lwTools.AssertFormat( sTimeUnitKeys.Length==timeUnitTextIdCount, "Language Manager : the length of the array of time unit keys given does not match the amount of time unit.\nArray length is {0} and the number of time units is {1}.", sTimeUnitKeys.Length, timeUnitTextIdCount );

		uint nDays = 0;
		uint nHours = 0;
		uint nMinutes = 0;
		uint nSeconds = 0;
		lwTools.ConvertTime( nTimeInSeconds, out nDays, out nHours, out nMinutes, out nSeconds );

		string sFontStart;
		string sFontEnd;
		if( string.IsNullOrEmpty( sNumbersFont ) )
		{
			sFontStart = "";
			sFontEnd = "";
		}
		else
		{
			sFontStart = "<font=\"" + sNumbersFont + "\">";
			sFontEnd = "</font>";
		}

		if( nDays>0 )
		{
			string sDays = GetString( sTimeUnitKeys[(int)( ( nDays>1 ) ? TimeUnitTextId.Days : TimeUnitTextId.Day )] );
			if( nHours>0 )
			{
				string sHours = GetString( sTimeUnitKeys[(int)( ( nHours>1 ) ? TimeUnitTextId.Hours : TimeUnitTextId.Hour )] );
				return String.Format( "{5}{1:D}{6}{0}{2} {5}{3:D}{6}{0}{4}", sSeparator, nDays, sDays, nHours, sHours, sFontStart, sFontEnd );
			}
			else
			{
				return String.Format( "{3}{1:D}{4}{0}{2}", sSeparator, nDays, sDays, sFontStart, sFontEnd );
			}
		}
		else if( nHours>0 )
		{
			string sHours = GetString( sTimeUnitKeys[(int)( ( nHours>1 ) ? TimeUnitTextId.Hours : TimeUnitTextId.Hour )] );
			if( nMinutes>0 )
			{
				string sMinutes = GetString( sTimeUnitKeys[(int)( ( nMinutes>1 ) ? TimeUnitTextId.Minutes : TimeUnitTextId.Minute )] );
				return String.Format( "{5}{1:D}{6}{0}{2} {5}{3:D}{6}{0}{4}", sSeparator, nHours, sHours, nMinutes, sMinutes, sFontStart, sFontEnd );
			}
			else
			{
				return String.Format( "{3}{1:D}{4}{0}{2}", sSeparator, nHours, sHours, sFontStart, sFontEnd );
			}
		}
		else if( nMinutes>0 )
		{
			string sMinutes = GetString( sTimeUnitKeys[(int)( ( nMinutes>1 ) ? TimeUnitTextId.Minutes : TimeUnitTextId.Minute )] );
			if( nSeconds>0 )
			{
				string sSeconds = GetString( sTimeUnitKeys[(int)( ( nSeconds>1 ) ? TimeUnitTextId.Seconds : TimeUnitTextId.Second )] );
				return String.Format( "{5}{1:D}{6}{0}{2} {5}{3:D}{6}{0}{4}", sSeparator, nMinutes, sMinutes, nSeconds, sSeconds, sFontStart, sFontEnd );
			}
			else
			{
				return String.Format( "{3}{1:D}{4}{0}{2}", sSeparator, nMinutes, sMinutes, sFontStart, sFontEnd );
			}
		}
		else
		{
			string sSeconds = GetString( sTimeUnitKeys[(int)( ( nSeconds>1 ) ? TimeUnitTextId.Seconds : TimeUnitTextId.Second )] );
			return String.Format( "{3}{1:D}{4}{0}{2}", sSeparator, nSeconds, sSeconds, sFontStart, sFontEnd );
		}
	}

	/// <summary>
	/// Reloads the texts from the resource text files.
	/// </summary>
	public void ReloadTexts()
	{
		lwTools.Assert( m_nLanguageIndex>=0 && m_nLanguageIndex<m_availableLanguages.Length, "Language manager does not have a current language." );

		LoadCurrentLanguage();

		if( m_onLanguageChangedEvent!=null )
		{
			m_onLanguageChangedEvent();
		}
	}

#region Private
	#region Declarations
	private struct AssetHolderKey
	{
		public readonly string m_sAssetArrayName;
		public readonly string m_sAssetName;

		public AssetHolderKey( string sAssetArrayName, string sAssetName )
		{
			lwTools.Assert( string.IsNullOrEmpty( sAssetArrayName )==false );
			m_sAssetArrayName = sAssetArrayName;
			m_sAssetName = sAssetName;
		}

		public AssetHolderKey( AssetHolderKey source )
		{
			lwTools.Assert( string.IsNullOrEmpty( source.m_sAssetArrayName )==false );
			m_sAssetArrayName = source.m_sAssetArrayName;
			m_sAssetName = source.m_sAssetName;
		}
	}
	#endregion

	#region Methods
	private void SetAndLoadLanguage( int nLanguageIndex )
	{
		lwTools.AssertFormat( nLanguageIndex>=0 && nLanguageIndex<m_availableLanguages.Length, "Invalid language index : {0}.", nLanguageIndex );
		if( m_nLanguageIndex!=nLanguageIndex )
		{
			m_nLanguageIndex = nLanguageIndex;

			LoadCurrentLanguage();

			if( m_onLanguageChangedEvent!=null )
			{
				m_onLanguageChangedEvent();
			}
		}
	}

	private void LoadCurrentLanguage()
	{
		m_localizedTexts.Clear();
		m_localizedResources.Clear();

		List<string> sFilesContent = new List<string>();
		HashSet<AssetHolderKey>.Enumerator filePathEnumerator = m_sTextFilesPerLanguage[m_nLanguageIndex].GetEnumerator();
		while( filePathEnumerator.MoveNext() )
		{
			FillCsvContents( ref sFilesContent, filePathEnumerator.Current );
		}

		for( int nFileContentIndex = 0; nFileContentIndex<sFilesContent.Count; ++nFileContentIndex )
		{
			FillDictionaryWithCsvContent( sFilesContent[nFileContentIndex] );
		}
	}

	private void FillCsvContents( ref List<string> sCsvContents, AssetHolderKey sAssetHolderKey )
	{
/*		if( lwAssetHolder.IsArrayAssetExist( sAssetHolderKey.m_sAssetArrayName ) )
		{
			if( String.IsNullOrEmpty( sAssetHolderKey.m_sAssetName ) )
			{
				int nAssetCount = lwAssetHolder.GetAssetArrayCount( sAssetHolderKey.m_sAssetArrayName );
				for( int nAssetIndex = 0; nAssetIndex<nAssetCount; ++nAssetIndex )
				{
					TextAsset textAsset = lwAssetHolder.GetArrayAssetElement<TextAsset>( sAssetHolderKey.m_sAssetArrayName, nAssetIndex );
					if( textAsset==null )
					{
						Debug.LogErrorFormat( "Language Manager : Resource at path '{0}' in the asset array '{1}' is not a TextAsset.", sAssetHolderKey.m_sAssetName, sAssetHolderKey.m_sAssetArrayName );
					}
					else
					{
						sCsvContents.Add( textAsset.text );
					}
				}
			}
			else
			{
				TextAsset textAsset = lwAssetHolder.GetArrayAssetElement<TextAsset>( sAssetHolderKey.m_sAssetArrayName, sAssetHolderKey.m_sAssetName );
				if( textAsset==null )
				{
					Debug.LogErrorFormat( "Language Manager : Resource not found at path '{0}' in the asset array '{1}'.", sAssetHolderKey.m_sAssetName, sAssetHolderKey.m_sAssetArrayName );
				}
				else
				{
					sCsvContents.Add( textAsset.text );
				}
			}
		}
		else
		{
			Debug.LogWarningFormat( "Language manager : asset array '{0}' does not exist in the asset holder.", sAssetHolderKey.m_sAssetArrayName );
		}*/

		
		TextAsset textAsset = Resources.Load<TextAsset>( "Texts/" + sAssetHolderKey.m_sAssetArrayName + "/" + sAssetHolderKey.m_sAssetName );
		if( textAsset==null )
		{
			Debug.LogErrorFormat( "Language Manager : Resource not found at path '{0}' in the asset array '{1}'.", sAssetHolderKey.m_sAssetName, sAssetHolderKey.m_sAssetArrayName );
		}
		else
		{
			sCsvContents.Add( textAsset.text );
		}
	}

	private void FillDictionaryWithCsvContent( string sCsvContent )
	{		
		string[] sLines = sCsvContent.Split( new char[]{ '\n' }, System.StringSplitOptions.RemoveEmptyEntries );
		// omit first line (header)
		for( int nLineIndex = 1; nLineIndex<sLines.Length; ++nLineIndex )
		{
			if( string.IsNullOrEmpty( sLines[nLineIndex] ) == false )
			{
                string[] sKeyValuePair = sLines[nLineIndex].Split( new char[]{ ';' } );
				if( sKeyValuePair.Length==2 )
				{
					if( string.IsNullOrEmpty( sKeyValuePair[0] ) )
					{
						Debug.LogErrorFormat( "Language Manager : empty localization key found while parsing '{0}'.", sLines[nLineIndex] );
					}
					else
					{
						// localized text
						if( sKeyValuePair[0].StartsWith( "str_", StringComparison.Ordinal ) )
						{
							if( m_localizedTexts.ContainsKey( sKeyValuePair[0] ) )
							{
								Debug.LogErrorFormat( "Language manager : localization key '{0}' already registered.", sKeyValuePair[0] );
							}
							else
							{
								m_localizedTexts.Add( sKeyValuePair[0], sKeyValuePair[1].Replace("\\n", "\n").Replace("\r","") );
							}
						}
						// localized resource
						else
						{
//							Debug.LogWarning( "Not integrate yet " + sKeyValuePair[0]); 
						}
					}
				}
				else
				{
					Debug.LogErrorFormat( "Language Manager : parsing a CSV file resulted in a parsing error with line '{0}'.", sLines[nLineIndex] );
				}
			}
		}
	}
	#endregion

	#region Attributes
	private int	m_nLanguageIndex;
	private readonly lwCountry[] m_availableLanguages;
	private readonly HashSet<AssetHolderKey>[] m_sTextFilesPerLanguage;

	private readonly Dictionary<string, string> m_localizedTexts;
	private readonly Dictionary<string, AssetHolderKey> m_localizedResources;
	#endregion
#endregion
}
