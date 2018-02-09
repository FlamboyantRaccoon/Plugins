// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2017/03/30

using UnityEngine;

using System;

/// <summary>
/// Base class for localization with image components
/// </summary>
public abstract class lwLocalizedImageBase : MonoBehaviour
{
	public delegate void OnTranslationUpdated( Sprite a_sprite );
	public OnTranslationUpdated	m_onTranslationUpdatedCbk = null;

	[SerializeField]
	private string m_sLocalizationID;

	public bool hasLocalizationKey
	{
		get{ return String.IsNullOrEmpty( m_sLocalizationID )==false; }
	}
		
	public Sprite sprite
	{
		get{ return m_sprite; }
	}

	public void SetSprite( Sprite sprite )
	{
		if( m_sLocalizationID!=null  ||  sprite!=m_sprite )
		{
			m_sLocalizationID = null;
			m_sprite = sprite;

			UpdateImage();
		}
	}

	public void SetLocalizationId( string sLocalizationId )
	{
		if( String.CompareOrdinal( sLocalizationId, m_sLocalizationID )!=0 )
		{
			m_sLocalizationID = sLocalizationId;
			m_sprite = null;

			UpdateImage();
		}
	}

#region Unity callbacks
	protected void Init( Sprite sCurrentSpriteInImageComponent )
	{
		m_sprite = sCurrentSpriteInImageComponent;

		lwLanguageManager.instance.m_onLanguageChangedEvent += UpdateImage;

		UpdateImage();
	}

	protected virtual void OnDestroy()
	{
		if(lwLanguageManager.IsInstanceValid())
		{
			lwLanguageManager.instance.m_onLanguageChangedEvent -= UpdateImage;
		}
	}
#endregion

#region Protected
	protected abstract void UpdateImageComponent();
#endregion

#region Private
	#region Methods
	private void UpdateImage()
	{
		if( string.IsNullOrEmpty( m_sLocalizationID )==false )
		{
			Debug.LogWarning( "not integrated yet" );
		}

		UpdateImageComponent();

		if( m_onTranslationUpdatedCbk!=null )
		{
			m_onTranslationUpdatedCbk( m_sprite );
		}
	}
	#endregion

	#region Attributes
	private Sprite m_sprite;
	#endregion
#endregion
}
