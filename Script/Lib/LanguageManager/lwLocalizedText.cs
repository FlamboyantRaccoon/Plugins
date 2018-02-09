// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2017/03/28

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Localization component used with Unity text component
/// </summary>
[RequireComponent( typeof( Text ) )]
[AddComponentMenu( "LWS/Lang/LocalizedText" )]
public sealed class lwLocalizedText : lwLocalizedTextBase
{
#region Unity callbacks
	private void Awake()
	{
		m_textComponent = GetComponent<Text>();
		lwTools.AssertFormat( m_textComponent!=null, "There is a localization component on object '{0}' but no Unity text component attached.", name );

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
		m_textComponent.text = base.text;
	}
#endregion

#region Private
	#region Attributes
	private Text m_textComponent;
	#endregion
#endregion
}
