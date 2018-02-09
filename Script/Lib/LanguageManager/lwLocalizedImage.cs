// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2017/03/30

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Localization component used with Unity image component
/// </summary>
[RequireComponent( typeof( Image ) )]
[AddComponentMenu( "LWS/Lang/LocalizedImage" )]
public sealed class lwLocalizedImage : lwLocalizedImageBase
{
#region Unity callbacks
	private void Awake()
	{
		m_imageComponent = GetComponent<Image>();
		lwTools.AssertFormat( m_imageComponent!=null, "There is a localization component on object '{0}' but no Unity image component attached.", name );

		base.Init( m_imageComponent.sprite );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		m_imageComponent = null;
	}
#endregion

#region Protected
	protected override void UpdateImageComponent()
	{
		m_imageComponent.sprite = base.sprite;
	}
#endregion

#region Private
	#region Attributes
	private Image m_imageComponent;
	#endregion
#endregion
}
