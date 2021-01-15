using UnityEngine;
using System.Collections.Generic;

public sealed class lwCountry
{
	public SystemLanguage m_eLang = SystemLanguage.Unknown;
	public string m_sLanguageISO6391 = string.Empty;
	public string m_sLanguageCulture = string.Empty;
	public string m_sNativeLanguageName = string.Empty;
	public string m_sEnglishLanguageName = string.Empty;
	public bool m_bIsLatin = true;

	public lwCountry( SystemLanguage eLang, string sLanguageISO6391, string sLanguageCulture, string sNativeLanguageName, string sEnglishLanguageName, bool bIsLatin )
	{
		m_eLang = eLang;
		m_sLanguageISO6391 = sLanguageISO6391;
		m_sLanguageCulture = sLanguageCulture;
		m_sNativeLanguageName = sNativeLanguageName;
		m_sEnglishLanguageName = sEnglishLanguageName;
		m_bIsLatin = bIsLatin;
	}
}

public sealed class lwCountryCode
{
	private const string LANGS_PATH = "Assets/Resources/Texts/Langs.txt";

	private static readonly lwCountry[] s_countries = 
	{
		new lwCountry( SystemLanguage.German,				"de", "de-DE", "Deutsch",		"German",		true ),
		new lwCountry( SystemLanguage.English,				"en", "en-US", "English",		"English",		true ),
        new lwCountry( SystemLanguage.English,              "en", "en-UK", "English",       "English",      true ),
        new lwCountry( SystemLanguage.Spanish,				"es", "es-ES", "Español",		"Spanish",		true ),
		new lwCountry( SystemLanguage.French,				"fr", "fr-FR", "Français",		"French",		true ),
		new lwCountry( SystemLanguage.Italian,				"it", "it-IT", "Italiano",		"Italian",		true ),
		new lwCountry( SystemLanguage.Japanese,				"ja", "ja-JP", "日本語",			"Japanese",		false ),
		new lwCountry( SystemLanguage.Korean,				"ko", "ko-KR", "한국의",			"Korean",		false ),
		new lwCountry( SystemLanguage.Dutch,				"nl", "nl-NL", "Nederlands",	"Dutch",		true ),
		new lwCountry( SystemLanguage.Polish,				"pl", "pl-PL", "Polski",		"Polish",		true ),
		new lwCountry( SystemLanguage.Portuguese,			"pt", "pt-PT", "Português",		"Portuguese",	true ),
		new lwCountry( SystemLanguage.Russian,				"ru", "ru-RU", "Pусский",		"Russian",		false ),
		new lwCountry( SystemLanguage.Swedish,				"sv", "sv-SE", "Svenska",		"Swedish",		true ),
		new lwCountry( SystemLanguage.Turkish,				"tr", "tr-TR", "Türkçe",		"Turkish",		true ),
		new lwCountry( SystemLanguage.Chinese,				"zh", "zh-CN", "中文",			"Chinese",		false ),
#if UNITY_4_6 || UNITY_5
		new lwCountry( SystemLanguage.ChineseSimplified,	"zh", "zh-CN", "中文",			"Chinese",		false ),
		new lwCountry( SystemLanguage.ChineseTraditional,	"zh", "zh-CN", "中文",			"Chinese",		false ),
#endif
		new lwCountry( SystemLanguage.Hungarian,			"hu", "hu-HU", "Magyar",		"Hungarian",	true ),
		new lwCountry( SystemLanguage.Romanian,				"ro", "ro-RO", "Română",		"Romanian",		true ),
		new lwCountry( SystemLanguage.Czech,				"cs", "cs-CZ", "Čeština",		"Czech",		true ),
		new lwCountry( SystemLanguage.Portuguese,			"pt", "pt-BR", "Português BR",	"Brazilian",	true ),
		new lwCountry( SystemLanguage.Slovak,				"sk", "sk-SK", "Slovenčina",	"Slovak",		true ),
		new lwCountry( SystemLanguage.Finnish,				"fi", "fi-FI", "Suomi",			"Finnish",		true ),
		new lwCountry( SystemLanguage.Norwegian,			"nn", "nn-NO", "Norsk",			"Norwegian",	true ),
		new lwCountry( SystemLanguage.Danish,				"da", "da-DK", "Dansk",			"Danish",		true ),
		new lwCountry( SystemLanguage.Greek,				"el", "el-GR", "ελληνικά",		"Greek",		false )
	};

	// Font countries are fake latin country + non latin languages
	public static lwCountry[] GetFontCountries()
	{
		List<lwCountry> fontCountries = new List<lwCountry>();
		fontCountries.Add( new lwCountry( SystemLanguage.Unknown, "LN", "Latin", "Latin", "Latin", true ) );
		for( int i=0; i<s_countries.Length; i++ )
		{
			if( !s_countries[i].m_bIsLatin )
			{
				fontCountries.Add( s_countries[i] );
			}
		}
		return fontCountries.ToArray();
	}

	public static lwCountry[] GetAllCountries()
	{
		return s_countries;
	}

	public static lwCountry GetlwCountry( string sLanguageCulture )
	{
		for( int i=0; i<s_countries.Length; i++ )
		{
			if( s_countries[i].m_sLanguageCulture==sLanguageCulture )
			{
				return s_countries[i];
			}
		}
		return null;
	}
	
	public static lwCountry GetlwCountryFromISO6391( string sId )
	{
		for( int i=0; i<s_countries.Length; i++ )
		{
			if( s_countries[i].m_sLanguageISO6391==sId )
			{
				return s_countries[i];
			}
		}
		return null;
	}

	public static lwCountry GetlwCountry( SystemLanguage eLang )
	{
		for( int i=0; i<s_countries.Length; i++ )
		{
			if( s_countries[i].m_eLang==eLang )
			{
				return s_countries[i];
			}
		}
		return null;
	}

	public static SystemLanguage GetSystemLanguage( string sLanguageCulture )
	{
		lwCountry country = GetlwCountry( sLanguageCulture );
		return ( country!=null ) ? country.m_eLang : SystemLanguage.Unknown;
	}
	
	public static SystemLanguage GetSystemLanguageFromISO6391( string sId )
	{
		lwCountry country = GetlwCountryFromISO6391( sId );
		return ( country!=null ) ? country.m_eLang : SystemLanguage.Unknown;
	}

	public static string GetLanguageCulture( SystemLanguage eLang )
	{
		lwCountry country = GetlwCountry( eLang );
		return ( country!=null ) ? country.m_sLanguageCulture : string.Empty;
	}

	public static string GetLanguageISO6391( SystemLanguage eLang )
	{
		lwCountry country = GetlwCountry( eLang );
		return ( country!=null ) ? country.m_sLanguageISO6391 : string.Empty;
	}

	public static bool IsLatinLanguage( SystemLanguage eLang )
	{
		lwCountry country = GetlwCountry( eLang );
		return ( country!=null ) ? country.m_bIsLatin : true;
	}

#region Writing the languages
#if UNITY_EDITOR
	// return the first language culture that is not in lwCountryCode
	public static string CheckUnhandledLanguage( List<string> sLanguageCultures )
	{
		if( sLanguageCultures != null )
		{
			
			for( int i = 0; i < sLanguageCultures.Count; i++ )
			{
				lwCountry country = GetlwCountry( sLanguageCultures[i] );
				if( country == null )
					return sLanguageCultures[i];
			}
		}
		return string.Empty;
	}

	public static bool WriteLangs( List<string> sLanguageCultures, out string sError )
	{
		bool bError = false;
		sError = string.Empty;
		try
		{
			string sUnhandledLanguage = CheckUnhandledLanguage( sLanguageCultures );
			if( !string.IsNullOrEmpty( sUnhandledLanguage ) )
			{
				sError = sUnhandledLanguage + " is not a language handled in lwCountry";
				bError = true;
			}
			else
			{
				System.IO.StreamWriter sw = new System.IO.StreamWriter( LANGS_PATH );
				sw.Write( string.Join( "\n", sLanguageCultures.ToArray() ) );
				sw.Close();

				UnityEditor.AssetDatabase.Refresh();
			}
		}
		catch( System.IO.IOException )
		{
			sError = "Unable to write to '" + LANGS_PATH + "'.";
			bError = true;
		}

		return !bError;
	}
#endif
#endregion
}
