// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/07/08

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

//! @class lwToolPreferences
//!
//!	@brief	Preferences for LWS tools
public static class lwToolPreferences
{
	public static void RegisterPreferences( string sName, System.Action cbkOnGuiCallback )
	{
		if( m_cbkOnGuiCallbacks.ContainsKey( sName )==false )
		{
			m_cbkOnGuiCallbacks.Add( sName, cbkOnGuiCallback );
		}
	}
	
#region Private
	#region Methods
	[PreferenceItem("LWS")]
	private static void OnGUI()
	{
		string[] sKeys = new string[m_cbkOnGuiCallbacks.Count];
		m_cbkOnGuiCallbacks.Keys.CopyTo( sKeys, 0 );

		switch( m_cbkOnGuiCallbacks.Count )
		{
			case 0: return;
			case 1:
			{
				EditorGUILayout.LabelField( sKeys[0], EditorStyles.boldLabel );
				EditorGUILayout.BeginVertical( GUI.skin.box, GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
			}
			break;
			default:
			{
				m_nTabSelectionIndex = GUILayout.Toolbar( m_nTabSelectionIndex, sKeys );
				EditorGUILayout.BeginVertical( GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
			}
			break;
		}

		m_cbkOnGuiCallbacks[sKeys[m_nTabSelectionIndex]]();
		EditorGUILayout.EndVertical();
	}
	#endregion

	#region Attributes
	private static SortedList<string, System.Action> m_cbkOnGuiCallbacks = new SortedList<string, System.Action>();

	private static int m_nTabSelectionIndex = 0;
	#endregion
#endregion
}
