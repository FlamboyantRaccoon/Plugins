// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2017/03/28

using UnityEngine;

using System;

/// <summary>
/// Base class for localization with text components
/// </summary>
public abstract class lwLocalizedTextBase : MonoBehaviour
{
	public delegate void OnTranslationUpdated( string sText );
	public OnTranslationUpdated	m_onTranslationUpdatedCbk = null;

	[SerializeField]
	private string m_sTextID;
	[SerializeField]
	private bool m_bForceUpperCase;

	public bool hasLocalizationKey
	{
		get{ return String.IsNullOrEmpty( m_sTextID )==false; }
	}

	public string text
	{
		get{ return m_sText; }
	}

	public string textID
	{
		get { return m_sTextID; }
	}

	public void SetText( string sText )
	{
		if( m_sTextID!=null  ||  String.CompareOrdinal( sText, m_sText )!=0 )
		{
			m_sTextID = null;
			m_sText = sText;

			UpdateText();
		}
	}

	public void SetTextId( string sTextId )
	{
		if( String.CompareOrdinal( sTextId, m_sTextID )!=0 )
		{
			m_sTextID = sTextId;
			m_sText = sTextId;

			UpdateText();
		}
	}

#region Unity callbacks
	protected void Init( string sCurrentTextInTextComponent )
	{
		m_sText = sCurrentTextInTextComponent;
		
		lwLanguageManager.instance.m_onLanguageChangedEvent += UpdateText;

		UpdateText();
	}

	protected virtual void OnDestroy()
	{
		if(lwLanguageManager.IsInstanceValid())
		{
			lwLanguageManager.instance.m_onLanguageChangedEvent -= UpdateText;
		}
	}
#endregion

#region Protected
	protected abstract void UpdateTextComponent();
#endregion

#region Private
	#region Methods
	private void UpdateText()
	{
		if( string.IsNullOrEmpty( m_sTextID ) )
		{
			if( m_bForceUpperCase )
			{
				m_sText = m_sText.ToUpper();
			}
		}
		else
		{
			m_sText = lwLanguageManager.instance.GetString( m_sTextID );
			if( m_bForceUpperCase )
			{
				m_sText = m_sText.ToUpper();
			}
		}
		
		UpdateTextComponent();

		if( m_onTranslationUpdatedCbk!=null )
		{
			m_onTranslationUpdatedCbk( m_sText );
		}
	}
	#endregion

	#region Attributes
	private string m_sText;
	#endregion
#endregion
}
