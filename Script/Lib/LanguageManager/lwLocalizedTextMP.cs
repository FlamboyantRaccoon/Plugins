// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2017/03/28

using TMPro;
using UnityEngine;

/// <summary>
/// Localization component used with TextMesh Pro text component
/// </summary>
[RequireComponent( typeof(TMP_Text) )]
[AddComponentMenu( "LWS/Lang/LocalizedTextMP" )]
public sealed class lwLocalizedTextMP : lwLocalizedTextBase
{
#region Unity callbacks
	private void Awake()
	{
		m_textComponent = GetComponent<TMP_Text>();
		lwTools.AssertFormat( m_textComponent!=null, "There is a localization component on object '{0}' but no TextMesh Pro text component attached.", name );

		//m_sMaterialName = m_textComponent.fontSharedMaterial.name;

		base.Init( m_textComponent.text );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		m_textComponent = null;
	}
#endregion

#region Protected
	protected override void UpdateTextComponent()
	{
	//	lwCountry lang = lwLanguageManager.instance.currentLanguage;
	//	string sTextToSet;
	//	if( lang.m_bIsLatin || base.hasLocalizationKey==false )
	//	{
	//		sTextToSet = base.text;
	//	}
	//	else
	//	{
	//		if( string.IsNullOrEmpty( m_sMaterialName ) )
	//		{
	//			sTextToSet = string.Format( "<font=\"{0}\">{1}", lang.m_sLanguageCulture, base.text );
	//		}
	//		else
	//		{
	//			sTextToSet = string.Format( "<font=\"{0}\" material=\"{1}\">{2}", lang.m_sLanguageCulture, string.Format( "{0}_{1}", m_sMaterialName, lang.m_sLanguageCulture ), base.text );
	//		}
	//	}
	//  if( m_textComponent!=null ) m_textComponent.text = sTextToSet;
		if( m_textComponent!=null ) m_textComponent.text = base.text;
	}
#endregion

#region Private
	#region Attributes
	private TMP_Text m_textComponent;
	//private string m_sMaterialName;
	#endregion
#endregion
}
